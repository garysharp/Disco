using Disco.Data.Repository;
using Disco.Models.Exporting;
using Disco.Models.Repository;
using Disco.Models.Services.Exporting;
using Disco.Models.Services.Jobs;
using Disco.Services.Exporting;
using Disco.Services.Jobs.JobQueues;
using Disco.Services.Plugins.Features.DetailsProvider;
using Disco.Services.Tasks;
using Disco.Services.Users;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Disco.Services.Jobs
{
    public class JobExport : IExport<JobExportOptions, JobExportRecord>
    {
        public Guid Id { get; set; }
        public string Name { get; } = "Job Export";
        public JobExportOptions Options { get; set; }

        public string FilenamePrefix { get; } = "JobExport";
        public string ExcelWorksheetName { get; } = "JobExport";
        public string ExcelTableName { get; } = "Jobs";

        public JobExport(JobExportOptions options)
        {
            Id = Guid.NewGuid();
            Options = options;
        }


        [JsonConstructor]
        public JobExport()
            : this(JobExportOptions.DefaultOptions())
        {
        }

        public ExportResult Export(DiscoDataContext database, IScheduledTaskStatus status)
            => Exporter.Export(this, database, status);

        private IQueryable<Job> BuildFilteredRecords(DiscoDataContext database)
        {
            var o = Options;

            var q = database.Jobs.Where(j => j.OpenedDate >= o.FilterStartDate);
            if (o.FilterEndDate.HasValue)
                q = q.Where(j => j.OpenedDate <= o.FilterEndDate);

            if (o.FilterJobTypeId != null)
                q = q.Where(j => j.JobTypeId == o.FilterJobTypeId);

            if (o.FilterJobSubTypeIds?.Any() ?? false)
                q = q.Where(j => j.JobSubTypes.Any(st => o.FilterJobSubTypeIds.Contains(st.Id)));

            if (o.FilterJobQueueId.HasValue)
                q = q.Where(j => j.JobQueues.Any(jq => !jq.RemovedDate.HasValue && jq.JobQueueId == o.FilterJobQueueId));

            if (o.FilterJobStatusId != null)
            {
                if (o.FilterJobStatusId != Job.JobStatusIds.Closed)
                    q = q.Where(j => j.ClosedDate == null);

                switch (o.FilterJobStatusId)
                {
                    case Job.JobStatusIds.Open:
                        // already filtered
                        break;
                    case Job.JobStatusIds.AwaitingAccountingPayment:
                        q = q.Where(j => j.JobTypeId == JobType.JobTypeIds.HNWar && j.JobMetaNonWarranty.AccountingChargeAddedDate != null && j.JobMetaNonWarranty.AccountingChargePaidDate == null);
                        break;
                    case Job.JobStatusIds.AwaitingAccountingCharge:
                        q = q.Where(j => j.JobTypeId == JobType.JobTypeIds.HNWar && j.JobMetaNonWarranty.AccountingChargeRequiredDate == null && (j.JobMetaNonWarranty.AccountingChargePaidDate != null || j.JobMetaNonWarranty.AccountingChargeAddedDate != null));
                        break;
                    case Job.JobStatusIds.AwaitingDeviceReturn:
                        q = q.Where(j => j.DeviceReadyForReturn != null && j.DeviceReturnedDate == null);
                        break;
                    case Job.JobStatusIds.AwaitingInsuranceProcessing:
                        q = q.Where(j => j.JobTypeId == JobType.JobTypeIds.HNWar && j.JobMetaNonWarranty.IsInsuranceClaim && j.JobMetaInsurance.ClaimFormSentDate == null);
                        break;
                    case Job.JobStatusIds.AwaitingRepairs:
                        q = q.Where(j => j.JobTypeId == JobType.JobTypeIds.HNWar && j.JobMetaNonWarranty.RepairerLoggedDate != null && j.JobMetaNonWarranty.RepairerCompletedDate == null);
                        break;
                    case Job.JobStatusIds.AwaitingUserAction:
                        q = q.Where(j => j.WaitingForUserAction != null);
                        break;
                    case Job.JobStatusIds.AwaitingWarrantyRepair:
                        q = q.Where(j => j.JobTypeId == JobType.JobTypeIds.HWar && j.JobMetaWarranty.ExternalLoggedDate != null && j.JobMetaWarranty.ExternalCompletedDate == null);
                        break;
                    case Job.JobStatusIds.Closed:
                        q = q.Where(j => j.ClosedDate != null);
                        break;
                    default:
                        throw new ArgumentException($"Unknown Job Status Id: {o.FilterJobStatusId}", nameof(o.FilterJobStatusId));
                }
            }

            return q;
        }

        public List<JobExportRecord> BuildRecords(DiscoDataContext database, IScheduledTaskStatus status)
        {
            database.Configuration.LazyLoadingEnabled = false;
            database.Configuration.ProxyCreationEnabled = false;

            var query = BuildFilteredRecords(database);

            // Update Users
            if (Options.UserDisplayName ||
                Options.UserSurname ||
                Options.UserGivenName ||
                Options.UserPhoneNumber ||
                Options.UserEmailAddress)
            {
                status.UpdateStatus(5, "Refreshing user details from Active Directory");
                var userIds = query.Where(d => d.UserId != null).Select(d => d.UserId).Distinct().ToList();
                foreach (var userId in userIds)
                {
                    try
                    {
                        UserService.GetUser(userId, database, true);
                    }
                    catch (Exception) { } // Ignore Errors
                }
            }

            // Update Last Network Logon Date
            if (Options.DeviceLastNetworkLogon)
            {
                status.UpdateStatus(15, "Refreshing device last network logon dates from Active Directory");
                try
                {
                    Interop.ActiveDirectory.ADNetworkLogonDatesUpdateTask.UpdateLastNetworkLogonDates(database, ScheduledTaskMockStatus.Create("UpdateLastNetworkLogonDates"));
                    database.SaveChanges();
                }
                catch (Exception) { } // Ignore Errors
            }

            status.UpdateStatus(25, "Extracting records from the database");

            var records = query.Select(j => new JobExportRecord()
            {
                Job = j,
                JobTypeDescription = j.JobType.Description,
                JobSubTypeDescriptions = j.JobSubTypes.Select(st => st.Description),

                LogCount = j.JobLogs.Count(),
                FirstLog = j.JobLogs.OrderBy(l => l.Id).FirstOrDefault(),
                LastLog = j.JobLogs.OrderByDescending(l => l.Id).FirstOrDefault(),

                AttachmentsCount = j.JobAttachments.Count(),

                QueueCount = j.JobQueues.Count(),
                QueueActiveCount = j.JobQueues.Count(q => !q.RemovedDate.HasValue),
                QueueLatestActive = j.JobQueues.Where(q => !q.RemovedDate.HasValue).OrderByDescending(q => q.Id).FirstOrDefault(),

                JobMetaWarranty = j.JobMetaWarranty,
                JobMetaNonWarranty = j.JobMetaNonWarranty,
                JobMetaInsurance = j.JobMetaInsurance,

                User = j.User,

                Device = j.Device,

                DeviceModelId = j.Device.DeviceModelId,
                DeviceModelDescription = j.Device.DeviceModel.Description,
                DeviceModelManufacturer = j.Device.DeviceModel.Manufacturer,
                DeviceModelModel = j.Device.DeviceModel.Model,
                DeviceModelType = j.Device.DeviceModel.ModelType,

                DeviceBatchId = j.Device.DeviceBatchId,
                DeviceBatchName = j.Device.DeviceBatch.Name,
                DeviceBatchPurchaseDate = j.Device.DeviceBatch.PurchaseDate,
                DeviceBatchSupplier = j.Device.DeviceBatch.Supplier,
                DeviceBatchUnitCost = j.Device.DeviceBatch.UnitCost,
                DeviceBatchWarrantyValidUntilDate = j.Device.DeviceBatch.WarrantyValidUntil,
                DeviceBatchInsuredDate = j.Device.DeviceBatch.InsuredDate,
                DeviceBatchInsuranceSupplier = j.Device.DeviceBatch.InsuranceSupplier,
                DeviceBatchInsuredUntilDate = j.Device.DeviceBatch.InsuredUntil,

                DeviceProfileId = j.Device.DeviceProfileId,
                DeviceProfileName = j.Device.DeviceProfile.Name,
                DeviceProfileShortName = j.Device.DeviceProfile.ShortName,
            }).ToList();

            records.ForEach(r =>
            {
                if (Options.JobStatus)
                {
                    r.JobStatus = JobExtensions.CalculateStatusId(
                        r.Job.ClosedDate,
                        r.Job.JobTypeId,
                        r.JobMetaWarranty?.ExternalLoggedDate,
                        r.JobMetaWarranty?.ExternalCompletedDate,
                        r.JobMetaNonWarranty?.RepairerLoggedDate,
                        r.JobMetaNonWarranty?.RepairerCompletedDate,
                        r.JobMetaNonWarranty?.AccountingChargeRequiredDate,
                        r.JobMetaNonWarranty?.AccountingChargeAddedDate,
                        r.JobMetaNonWarranty?.AccountingChargePaidDate,
                        r.JobMetaNonWarranty?.IsInsuranceClaim,
                        r.JobMetaInsurance?.ClaimFormSentDate,
                        r.Job.WaitingForUserAction,
                    r.Job.DeviceReadyForReturn,
                        r.Job.DeviceReturnedDate);
                }

                if (Options.UserDetailCustom && r.User != null)
                {
                    var detailsService = new DetailsProviderService(database);
                    r.UserCustomDetails = detailsService.GetDetails(r.User);
                }
            });

            return records;
        }

        public ExportMetadata<JobExportOptions, JobExportRecord> BuildMetadata(DiscoDataContext database, List<JobExportRecord> records, IScheduledTaskStatus status)
        {
            var metadata = new ExportMetadata<JobExportOptions, JobExportRecord>(Options);
            metadata.IgnoreShortNames.Add("Job");
            metadata.IgnoreShortNames.Add("Job Details");

            // Job
            metadata.Add(o => o.JobId, r => r.Job.Id);
            metadata.Add(o => o.JobStatus, r => Job.JobStatusIds.StatusDescriptions.TryGetValue(r.JobStatus, out var jobStatus) ? jobStatus : "Unknown");
            metadata.Add(o => o.JobType, r => r.JobTypeDescription);
            metadata.Add(o => o.JobSubTypes, r => string.Join(", ", r.JobSubTypeDescriptions));
            metadata.Add(o => o.JobOpenedDate, r => r.Job.OpenedDate);
            metadata.Add(o => o.JobOpenedUser, r => r.Job.OpenedTechUserId);
            metadata.Add(o => o.JobExpectedClosedDate, r => r.Job.ExpectedClosedDate);
            metadata.Add(o => o.JobClosedDate, r => r.Job.ClosedDate);
            metadata.Add(o => o.JobClosedUser, r => r.Job.ClosedTechUserId);

            // Job Details
            metadata.Add(o => o.JobDeviceHeldDate, r => r.Job.DeviceHeld);
            metadata.Add(o => o.JobDeviceHeldUser, r => r.Job.DeviceHeldTechUserId);
            metadata.Add(o => o.JobDeviceHeldLocation, r => r.Job.DeviceHeldLocation);
            metadata.Add(o => o.JobDeviceReadyForReturnDate, r => r.Job.DeviceReadyForReturn);
            metadata.Add(o => o.JobDeviceReadyForReturnUser, r => r.Job.DeviceReadyForReturnTechUserId);
            metadata.Add(o => o.JobDeviceReturnedDate, r => r.Job.DeviceReturnedDate);
            metadata.Add(o => o.JobDeviceReturnedUser, r => r.Job.DeviceReturnedTechUserId);
            metadata.Add(o => o.JobWaitingForUserActionDate, r => r.Job.WaitingForUserAction);

            // Job Logs
            metadata.Add(o => o.LogCount, r => r.LogCount);
            metadata.Add(o => o.LogFirstDate, r => r.FirstLog?.Timestamp);
            metadata.Add(o => o.LogFirstUser, r => r.FirstLog?.TechUserId);
            metadata.Add(o => o.LogFirstContent, r => r.FirstLog?.Comments);
            metadata.Add(o => o.LogLastDate, r => r.LastLog?.Timestamp);
            metadata.Add(o => o.LogLastUser, r => r.LastLog?.TechUserId);
            metadata.Add(o => o.LogLastContent, r => r.LastLog?.Comments);

            // Attachments
            metadata.Add(o => o.AttachmentsCount, r => r.AttachmentsCount);

            // Job Queues
            metadata.Add(o => o.JobQueueCount, r => r.QueueCount);
            metadata.Add(o => o.JobQueueActiveCount, r => r.QueueActiveCount);
            metadata.Add(o => o.JobQueueActiveLatest, r => r.QueueLatestActive?.JobQueueId == null ? null : JobQueueService.GetQueue(r.QueueLatestActive.JobQueueId).JobQueue.Name);
            metadata.Add(o => o.JobQueueActiveLatestAddedDate, r => r.QueueLatestActive?.AddedDate);
            metadata.Add(o => o.JobQueueActiveLatestAddedUser, r => r.QueueLatestActive?.AddedUserId);

            // Warranty
            metadata.Add(o => o.JobWarrantyExternalName, r => r.JobMetaWarranty?.ExternalName);
            metadata.Add(o => o.JobWarrantyExternalReference, r => r.JobMetaWarranty?.ExternalReference);
            metadata.Add(o => o.JobWarrantyExternalLoggedDate, r => r.JobMetaWarranty?.ExternalLoggedDate);
            metadata.Add(o => o.JobWarrantyExternalCompletedDate, r => r.JobMetaWarranty?.ExternalCompletedDate);

            // Non-Warranty
            metadata.Add(o => o.JobNonWarrantyAccountingChargeRequiredDate, r => r.JobMetaNonWarranty?.AccountingChargeRequiredDate);
            metadata.Add(o => o.JobNonWarrantyAccountingChargeAddedDate, r => r.JobMetaNonWarranty?.AccountingChargeAddedDate);
            metadata.Add(o => o.JobNonWarrantyAccountingChargePaidDate, r => r.JobMetaNonWarranty?.AccountingChargePaidDate);
            metadata.Add(o => o.JobNonWarrantyPurchaseOrderRaisedDate, r => r.JobMetaNonWarranty?.PurchaseOrderRaisedDate);
            metadata.Add(o => o.JobNonWarrantyPurchaseOrderReference, r => r.JobMetaNonWarranty?.PurchaseOrderReference);
            metadata.Add(o => o.JobNonWarrantyPurchaseOrderSentDate, r => r.JobMetaNonWarranty?.PurchaseOrderSentDate);
            metadata.Add(o => o.JobNonWarrantyInvoiceReceivedDate, r => r.JobMetaNonWarranty?.InvoiceReceivedDate);
            metadata.Add(o => o.JobNonWarrantyRepairerName, r => r.JobMetaNonWarranty?.RepairerName);
            metadata.Add(o => o.JobNonWarrantyRepairerLoggedDate, r => r.JobMetaNonWarranty?.RepairerLoggedDate);
            metadata.Add(o => o.JobNonWarrantyRepairerReference, r => r.JobMetaNonWarranty?.RepairerReference);
            metadata.Add(o => o.JobNonWarrantyRepairerCompletedDate, r => r.JobMetaNonWarranty?.RepairerCompletedDate);

            // Insurance
            metadata.Add(o => o.JobMetaInsuranceLossOrDamageDate, r => r.JobMetaInsurance?.LossOrDamageDate);
            metadata.Add(o => o.JobMetaInsuranceEventLocation, r => r.JobMetaInsurance?.EventLocation);
            metadata.Add(o => o.JobMetaInsuranceDescription, r => r.JobMetaInsurance?.Description);
            metadata.Add(o => o.JobMetaInsuranceThirdPartyCausedName, r => r.JobMetaInsurance?.ThirdPartyCausedName);
            metadata.Add(o => o.JobMetaInsuranceThirdPartyCausedWhy, r => r.JobMetaInsurance?.ThirdPartyCausedWhy);
            metadata.Add(o => o.JobMetaInsuranceWitnessesNamesAddresses, r => r.JobMetaInsurance?.WitnessesNamesAddresses);
            metadata.Add(o => o.JobMetaInsuranceBurglaryTheftMethodOfEntry, r => r.JobMetaInsurance?.BurglaryTheftMethodOfEntry);
            metadata.Add(o => o.JobMetaInsurancePropertyLastSeenDate, r => r.JobMetaInsurance?.PropertyLastSeenDate);
            metadata.Add(o => o.JobMetaInsurancePoliceNotifiedStation, r => r.JobMetaInsurance?.PoliceNotifiedStation);
            metadata.Add(o => o.JobMetaInsurancePoliceNotifiedDate, r => r.JobMetaInsurance?.PoliceNotifiedDate);
            metadata.Add(o => o.JobMetaInsurancePoliceNotifiedCrimeReportNo, r => r.JobMetaInsurance?.PoliceNotifiedCrimeReportNo);
            metadata.Add(o => o.JobMetaInsuranceRecoverReduceAction, r => r.JobMetaInsurance?.RecoverReduceAction);
            metadata.Add(o => o.JobMetaInsuranceOtherInterestedParties, r => r.JobMetaInsurance?.OtherInterestedParties);
            metadata.Add(o => o.JobMetaInsuranceDateOfPurchase, r => r.JobMetaInsurance?.DateOfPurchase);
            metadata.Add(o => o.JobMetaInsuranceClaimFormSentDate, r => r.JobMetaInsurance?.ClaimFormSentDate);
            metadata.Add(o => o.JobMetaInsuranceInsurer, r => r.JobMetaInsurance?.Insurer);
            metadata.Add(o => o.JobMetaInsuranceInsurerReference, r => r.JobMetaInsurance?.InsurerReference);

            // User Management
            metadata.Add(o => o.JobUserManagementFlags, r => r.Job.Flags?.ToString());

            // User
            metadata.Add(o => o.UserId, r => r.User?.UserId);
            metadata.Add(o => o.UserDisplayName, r => r.User?.DisplayName);
            metadata.Add(o => o.UserSurname, r => r.User?.Surname);
            metadata.Add(o => o.UserGivenName, r => r.User?.GivenName);
            metadata.Add(o => o.UserPhoneNumber, r => r.User?.PhoneNumber);
            metadata.Add(o => o.UserEmailAddress, r => r.User?.EmailAddress);

            // User Custom Details
            if (Options.UserDetailCustom)
            {
                var keys = records.Where(r => r.UserCustomDetails != null).SelectMany(r => r.UserCustomDetails.Keys).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
                foreach (var key in keys.OrderBy(k => k, StringComparer.OrdinalIgnoreCase))
                {
                    metadata.Add(key, r => r.UserCustomDetails != null && r.UserCustomDetails.TryGetValue(key, out var value) ? value : null);
                }
            }

            // Device
            metadata.Add(o => o.DeviceSerialNumber, r => r.Device?.SerialNumber);
            metadata.Add(o => o.DeviceAssetNumber, r => r.Device?.AssetNumber);
            metadata.Add(o => o.DeviceLocation, r => r.Device?.Location);
            metadata.Add(o => o.DeviceComputerName, r => r.Device?.DeviceDomainId);
            metadata.Add(o => o.DeviceLastNetworkLogon, r => r.Device?.LastNetworkLogonDate);
            metadata.Add(o => o.DeviceCreatedDate, r => r.Device?.CreatedDate);
            metadata.Add(o => o.DeviceFirstEnrolledDate, r => r.Device?.EnrolledDate);
            metadata.Add(o => o.DeviceLastEnrolledDate, r => r.Device?.LastEnrolDate);
            metadata.Add(o => o.DeviceAllowUnauthenticatedEnrol, r => r.Device?.AllowUnauthenticatedEnrol);
            metadata.Add(o => o.DeviceDecommissionedDate, r => r.Device?.DecommissionedDate);
            metadata.Add(o => o.DeviceDecommissionedReason, r => r.Device?.DecommissionReason?.ToString());

            // Model
            metadata.Add(o => o.DeviceModelId, r => r.DeviceModelId);
            metadata.Add(o => o.DeviceModelDescription, r => r.DeviceModelDescription);
            metadata.Add(o => o.DeviceModelManufacturer, r => r.DeviceModelManufacturer);
            metadata.Add(o => o.DeviceModelModel, r => r.DeviceModelModel);
            metadata.Add(o => o.DeviceModelType, r => r.DeviceModelType);

            // Batch
            metadata.Add(o => o.DeviceBatchId, r => r.DeviceBatchId);
            metadata.Add(o => o.DeviceBatchName, r => r.DeviceBatchName);
            metadata.Add(o => o.DeviceBatchPurchaseDate, r => r.DeviceBatchPurchaseDate);
            metadata.Add(o => o.DeviceBatchSupplier, r => r.DeviceBatchSupplier);
            metadata.Add(o => o.DeviceBatchUnitCost, r => r.DeviceBatchUnitCost);
            metadata.Add(o => o.DeviceBatchWarrantyValidUntilDate, r => r.DeviceBatchWarrantyValidUntilDate);
            metadata.Add(o => o.DeviceBatchInsuredDate, r => r.DeviceBatchInsuredDate);
            metadata.Add(o => o.DeviceBatchInsuranceSupplier, r => r.DeviceBatchInsuranceSupplier);
            metadata.Add(o => o.DeviceBatchInsuredUntilDate, r => r.DeviceBatchInsuredUntilDate);

            // Profile
            metadata.Add(o => o.DeviceProfileId, r => r.DeviceProfileId);
            metadata.Add(o => o.DeviceProfileName, r => r.DeviceProfileName);
            metadata.Add(o => o.DeviceProfileShortName, r => r.DeviceProfileShortName);

            return metadata;
        }
    }
}
