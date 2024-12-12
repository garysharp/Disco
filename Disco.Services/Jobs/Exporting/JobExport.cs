using Disco.Data.Repository;
using Disco.Models.Exporting;
using Disco.Models.Repository;
using Disco.Models.Services.Exporting;
using Disco.Models.Services.Jobs.Exporting;
using Disco.Services.Jobs.JobQueues;
using Disco.Services.Plugins.Features.DetailsProvider;
using Disco.Services.Tasks;
using Disco.Services.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;

namespace Disco.Services.Jobs.Exporting
{
    using Metadata = ExportFieldMetadata<JobExportRecord>;

    public static class JobExport
    {
        public static ExportResult GenerateExport(DiscoDataContext database, Func<IQueryable<Job>, IQueryable<Job>> filter, JobExportOptions options, IScheduledTaskStatus taskStatus)
        {
            database.Configuration.LazyLoadingEnabled = false;
            database.Configuration.ProxyCreationEnabled = false;

            var jobQuery = (IQueryable<Job>)database.Jobs;
            if (filter != null)
                jobQuery = filter(jobQuery);

            // Update Users
            if (options.UserDisplayName ||
                options.UserSurname ||
                options.UserGivenName ||
                options.UserPhoneNumber ||
                options.UserEmailAddress)
            {
                taskStatus.UpdateStatus(5, "Refreshing user details from Active Directory");
                var userIds = jobQuery.Where(d => d.UserId != null).Select(d => d.UserId).Distinct().ToList();
                foreach (var userId in userIds)
                {
                    try
                    {
                        UserService.GetUser(userId, database);
                    }
                    catch (Exception) { } // Ignore Errors
                }
            }

            // Update Last Network Logon Date
            if (options.DeviceLastNetworkLogon)
            {
                taskStatus.UpdateStatus(15, "Refreshing device last network logon dates from Active Directory");
                try
                {
                    Interop.ActiveDirectory.ADNetworkLogonDatesUpdateTask.UpdateLastNetworkLogonDates(database, ScheduledTaskMockStatus.Create("UpdateLastNetworkLogonDates"));
                    database.SaveChanges();
                }
                catch (Exception) { } // Ignore Errors
            }

            taskStatus.UpdateStatus(25, "Extracting records from the database");

            var records = BuildRecords(jobQuery).ToList();

            records.ForEach(r =>
            {
                if (options.JobStatus)
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

                if (options.UserDetailCustom && r.User != null)
                {
                    var detailsService = new DetailsProviderService(database);
                    r.UserCustomDetails = detailsService.GetDetails(r.User);
                }
            });

            taskStatus.UpdateStatus(70, "Building metadata");
            var metadata = options.BuildMetadata(records);

            if (metadata.Count == 0)
                throw new ArgumentException("At least one export field must be specified", "Options");

            taskStatus.UpdateStatus(80, $"Formatting {records.Count} records for export");

            return ExportHelpers.WriteExport(options, taskStatus, metadata, records);
        }

        public static ExportResult GenerateExport(DiscoDataContext database, JobExportOptions options, IScheduledTaskStatus taskStatus)
        {
            Func<IQueryable<Job>, IQueryable<Job>> filter = q =>
            {
                var r = q.Where(j => j.OpenedDate >= options.FilterStartDate);
                if (options.FilterEndDate.HasValue)
                    r = r.Where(j => j.OpenedDate <= options.FilterEndDate);

                if (options.FilterJobTypeId != null)
                    r = r.Where(j => j.JobTypeId == options.FilterJobTypeId);

                if (options.FilterJobSubTypeIds?.Any() ?? false)
                    r = r.Where(j => j.JobSubTypes.Any(st => options.FilterJobSubTypeIds.Contains(st.Id)));

                if (options.FilterJobQueueId.HasValue)
                    r = r.Where(j => j.JobQueues.Any(jq => !jq.RemovedDate.HasValue && jq.JobQueueId == options.FilterJobQueueId));

                if (options.FilterJobStatusId != null)
                {
                    if (options.FilterJobStatusId != Job.JobStatusIds.Closed)
                        r = r.Where(j => j.ClosedDate == null);

                    switch (options.FilterJobStatusId)
                    {
                        case Job.JobStatusIds.Open:
                            // already filtered
                            break;
                        case Job.JobStatusIds.AwaitingAccountingPayment:
                            r = r.Where(j => j.JobTypeId == JobType.JobTypeIds.HNWar && j.JobMetaNonWarranty.AccountingChargeAddedDate != null && j.JobMetaNonWarranty.AccountingChargePaidDate == null);
                            break;
                        case Job.JobStatusIds.AwaitingAccountingCharge:
                            r = r.Where(j => j.JobTypeId == JobType.JobTypeIds.HNWar && j.JobMetaNonWarranty.AccountingChargeRequiredDate == null && (j.JobMetaNonWarranty.AccountingChargePaidDate != null || j.JobMetaNonWarranty.AccountingChargeAddedDate != null));
                            break;
                        case Job.JobStatusIds.AwaitingDeviceReturn:
                            r = r.Where(j => j.DeviceReadyForReturn != null && j.DeviceReturnedDate == null);
                            break;
                        case Job.JobStatusIds.AwaitingInsuranceProcessing:
                            r = r.Where(j => j.JobTypeId == JobType.JobTypeIds.HNWar && j.JobMetaNonWarranty.IsInsuranceClaim && j.JobMetaInsurance.ClaimFormSentDate == null);
                            break;
                        case Job.JobStatusIds.AwaitingRepairs:
                            r = r.Where(j => j.JobTypeId == JobType.JobTypeIds.HNWar && j.JobMetaNonWarranty.RepairerLoggedDate != null && j.JobMetaNonWarranty.RepairerCompletedDate == null);
                            break;
                        case Job.JobStatusIds.AwaitingUserAction:
                            r = r.Where(j => j.WaitingForUserAction != null);
                            break;
                        case Job.JobStatusIds.AwaitingWarrantyRepair:
                            r = r.Where(j => j.JobTypeId == JobType.JobTypeIds.HWar && j.JobMetaWarranty.ExternalLoggedDate != null && j.JobMetaWarranty.ExternalCompletedDate == null);
                            break;
                        case Job.JobStatusIds.Closed:
                            r = r.Where(j => j.ClosedDate != null);
                            break;
                        default:
                            throw new ArgumentException($"Unknown Job Status Id: {options.FilterJobStatusId}", nameof(options.FilterJobStatusId));
                    }
                }

                return r;
            };

            return GenerateExport(database, filter, options, taskStatus);
        }

        public static ExportResult GenerateExport(DiscoDataContext database, JobExportOptions options)
        {
            return GenerateExport(database, options, ScheduledTaskMockStatus.Create("Job Export"));
        }

        private static IEnumerable<JobExportRecord> BuildRecords(IQueryable<Job> jobs)
        {
            return jobs.Select(j => new JobExportRecord()
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
            });
        }

        private static List<Metadata> BuildMetadata(this JobExportOptions options, List<JobExportRecord> records)
        {
            IEnumerable<string> userDetailCustomKeys = null;
            if (options.UserDetailCustom)
                userDetailCustomKeys = records.Where(r => r.UserCustomDetails != null).SelectMany(r => r.UserCustomDetails.Keys).Distinct(StringComparer.OrdinalIgnoreCase).ToList();

            var allAccessors = BuildRecordAccessors(userDetailCustomKeys);

            return typeof(JobExportOptions).GetProperties()
                .Where(p => p.PropertyType == typeof(bool))
                .Select(p => new
                {
                    property = p,
                    details = (DisplayAttribute)p.GetCustomAttributes(typeof(DisplayAttribute), false).FirstOrDefault()
                })
                .Where(p => p.details != null && (bool)p.property.GetValue(options))
                .SelectMany(p =>
                {
                    var fieldMetadata = allAccessors[p.property.Name];
                    fieldMetadata.ForEach(f =>
                    {
                        if (f.ColumnName == null)
                            f.ColumnName = (p.details.ShortName == "Job" || p.details.ShortName == "Job Details") ? p.details.Name : $"{p.details.ShortName} {p.details.Name}";
                    });
                    return fieldMetadata;
                }).ToList();
        }

        private static Dictionary<string, List<Metadata>> BuildRecordAccessors(IEnumerable<string> userDetailCustomKeys)
        {
            const string DateFormat = "yyyy-MM-dd";
            const string DateTimeFormat = DateFormat + " HH:mm:ss";

            Func<object, string> csvStringEncoded = (o) => o == null ? null : $"\"{((string)o).Replace("\"", "\"\"")}\"";
            Func<object, string> csvToStringEncoded = (o) => o == null ? null : o.ToString();
            Func<object, string> csvCurrencyEncoded = (o) => ((decimal?)o).HasValue ? ((decimal?)o).Value.ToString("C") : null;
            Func<object, string> csvDateEncoded = (o) => ((DateTime)o).ToString(DateFormat);
            Func<object, string> csvDateTimeEncoded = (o) => ((DateTime)o).ToString(DateTimeFormat);
            Func<object, string> csvNullableDateEncoded = (o) => ((DateTime?)o).HasValue ? csvDateEncoded(o) : null;
            Func<object, string> csvNullableDateTimeEncoded = (o) => ((DateTime?)o).HasValue ? csvDateTimeEncoded(o) : null;

            var metadata = new Dictionary<string, List<Metadata>>();

            // Job
            metadata.Add(nameof(JobExportOptions.JobId), new List<Metadata>() { new Metadata(nameof(JobExportOptions.JobId), typeof(int), r => r.Job.Id, csvToStringEncoded) });
            metadata.Add(nameof(JobExportOptions.JobStatus), new List<Metadata>() { new Metadata(nameof(JobExportOptions.JobStatus), typeof(string), r => Job.JobStatusIds.StatusDescriptions.TryGetValue(r.JobStatus, out var status) ? status : "Unknown", csvStringEncoded) });
            metadata.Add(nameof(JobExportOptions.JobType), new List<Metadata>() { new Metadata(nameof(JobExportOptions.JobType), typeof(string), r => r.JobTypeDescription, csvStringEncoded) });
            metadata.Add(nameof(JobExportOptions.JobSubTypes), new List<Metadata>() { new Metadata(nameof(JobExportOptions.JobSubTypes), typeof(string), r => string.Join(", ", r.JobSubTypeDescriptions), csvStringEncoded) });
            metadata.Add(nameof(JobExportOptions.JobOpenedDate), new List<Metadata>() { new Metadata(nameof(JobExportOptions.JobOpenedDate), typeof(DateTime), r => r.Job.OpenedDate, csvDateTimeEncoded) });
            metadata.Add(nameof(JobExportOptions.JobOpenedUser), new List<Metadata>() { new Metadata(nameof(JobExportOptions.JobOpenedUser), typeof(string), r => r.Job.OpenedTechUserId, csvStringEncoded) });
            metadata.Add(nameof(JobExportOptions.JobExpectedClosedDate), new List<Metadata>() { new Metadata(nameof(JobExportOptions.JobExpectedClosedDate), typeof(DateTime), r => r.Job.ExpectedClosedDate, csvNullableDateTimeEncoded) });
            metadata.Add(nameof(JobExportOptions.JobClosedDate), new List<Metadata>() { new Metadata(nameof(JobExportOptions.JobClosedDate), typeof(DateTime), r => r.Job.ClosedDate, csvNullableDateTimeEncoded) });
            metadata.Add(nameof(JobExportOptions.JobClosedUser), new List<Metadata>() { new Metadata(nameof(JobExportOptions.JobClosedUser), typeof(string), r => r.Job.ClosedTechUserId, csvStringEncoded) });

            // Job Details
            metadata.Add(nameof(JobExportOptions.JobDeviceHeldDate), new List<Metadata>() { new Metadata(nameof(JobExportOptions.JobDeviceHeldDate), typeof(DateTime), r => r.Job.DeviceHeld, csvNullableDateTimeEncoded) });
            metadata.Add(nameof(JobExportOptions.JobDeviceHeldUser), new List<Metadata>() { new Metadata(nameof(JobExportOptions.JobDeviceHeldUser), typeof(string), r => r.Job.DeviceHeldTechUserId, csvStringEncoded) });
            metadata.Add(nameof(JobExportOptions.JobDeviceHeldLocation), new List<Metadata>() { new Metadata(nameof(JobExportOptions.JobDeviceHeldLocation), typeof(string), r => r.Job.DeviceHeldLocation, csvStringEncoded) });
            metadata.Add(nameof(JobExportOptions.JobDeviceReadyForReturnDate), new List<Metadata>() { new Metadata(nameof(JobExportOptions.JobDeviceReadyForReturnDate), typeof(DateTime), r => r.Job.DeviceReadyForReturn, csvNullableDateTimeEncoded) });
            metadata.Add(nameof(JobExportOptions.JobDeviceReadyForReturnUser), new List<Metadata>() { new Metadata(nameof(JobExportOptions.JobDeviceReadyForReturnUser), typeof(string), r => r.Job.DeviceReadyForReturnTechUserId, csvStringEncoded) });
            metadata.Add(nameof(JobExportOptions.JobDeviceReturnedDate), new List<Metadata>() { new Metadata(nameof(JobExportOptions.JobDeviceReturnedDate), typeof(DateTime), r => r.Job.DeviceReturnedDate, csvNullableDateTimeEncoded) });
            metadata.Add(nameof(JobExportOptions.JobDeviceReturnedUser), new List<Metadata>() { new Metadata(nameof(JobExportOptions.JobDeviceReturnedUser), typeof(string), r => r.Job.DeviceReturnedTechUserId, csvStringEncoded) });
            metadata.Add(nameof(JobExportOptions.JobWaitingForUserActionDate), new List<Metadata>() { new Metadata(nameof(JobExportOptions.JobWaitingForUserActionDate), typeof(DateTime), r => r.Job.WaitingForUserAction, csvNullableDateTimeEncoded) });

            // Job Logs
            metadata.Add(nameof(JobExportOptions.LogCount), new List<Metadata>() { new Metadata(nameof(JobExportOptions.LogCount), typeof(int), r => r.LogCount, csvToStringEncoded) });
            metadata.Add(nameof(JobExportOptions.LogFirstDate), new List<Metadata>() { new Metadata(nameof(JobExportOptions.LogFirstDate), typeof(DateTime), r => r.FirstLog?.Timestamp, csvNullableDateTimeEncoded) });
            metadata.Add(nameof(JobExportOptions.LogFirstUser), new List<Metadata>() { new Metadata(nameof(JobExportOptions.LogFirstUser), typeof(string), r => r.FirstLog?.TechUserId, csvStringEncoded) });
            metadata.Add(nameof(JobExportOptions.LogFirstContent), new List<Metadata>() { new Metadata(nameof(JobExportOptions.LogFirstContent), typeof(string), r => r.FirstLog?.Comments, csvStringEncoded) });
            metadata.Add(nameof(JobExportOptions.LogLastDate), new List<Metadata>() { new Metadata(nameof(JobExportOptions.LogLastDate), typeof(DateTime), r => r.LastLog?.Timestamp, csvNullableDateTimeEncoded) });
            metadata.Add(nameof(JobExportOptions.LogLastUser), new List<Metadata>() { new Metadata(nameof(JobExportOptions.LogLastUser), typeof(string), r => r.LastLog?.TechUserId, csvStringEncoded) });
            metadata.Add(nameof(JobExportOptions.LogLastContent), new List<Metadata>() { new Metadata(nameof(JobExportOptions.LogLastContent), typeof(string), r => r.LastLog?.Comments, csvStringEncoded) });

            // Attachments
            metadata.Add(nameof(JobExportOptions.AttachmentsCount), new List<Metadata>() { new Metadata(nameof(JobExportOptions.AttachmentsCount), typeof(int), r => r.AttachmentsCount, csvToStringEncoded) });

            // Job Queues
            metadata.Add(nameof(JobExportOptions.JobQueueCount), new List<Metadata>() { new Metadata(nameof(JobExportOptions.JobQueueCount), typeof(int), r => r.QueueCount, csvToStringEncoded) });
            metadata.Add(nameof(JobExportOptions.JobQueueActiveCount), new List<Metadata>() { new Metadata(nameof(JobExportOptions.JobQueueActiveCount), typeof(int), r => r.QueueActiveCount, csvToStringEncoded) });
            metadata.Add(nameof(JobExportOptions.JobQueueActiveLatest), new List<Metadata>() { new Metadata(nameof(JobExportOptions.JobQueueActiveLatest), typeof(DateTime), r => r.QueueLatestActive?.JobQueueId == null ? null : JobQueueService.GetQueue(r.QueueLatestActive.JobQueueId).JobQueue.Name, csvNullableDateTimeEncoded) });
            metadata.Add(nameof(JobExportOptions.JobQueueActiveLatestAddedDate), new List<Metadata>() { new Metadata(nameof(JobExportOptions.JobQueueActiveLatestAddedDate), typeof(DateTime), r => r.QueueLatestActive?.AddedDate, csvNullableDateTimeEncoded) });
            metadata.Add(nameof(JobExportOptions.JobQueueActiveLatestAddedUser), new List<Metadata>() { new Metadata(nameof(JobExportOptions.JobQueueActiveLatestAddedUser), typeof(string), r => r.QueueLatestActive?.AddedUserId, csvStringEncoded) });

            // Warranty
            metadata.Add(nameof(JobExportOptions.JobWarrantyExternalName), new List<Metadata>() { new Metadata(nameof(JobExportOptions.JobWarrantyExternalName), typeof(string), r => r.JobMetaWarranty?.ExternalName, csvStringEncoded) });
            metadata.Add(nameof(JobExportOptions.JobWarrantyExternalReference), new List<Metadata>() { new Metadata(nameof(JobExportOptions.JobWarrantyExternalReference), typeof(string), r => r.JobMetaWarranty?.ExternalReference, csvStringEncoded) });
            metadata.Add(nameof(JobExportOptions.JobWarrantyExternalLoggedDate), new List<Metadata>() { new Metadata(nameof(JobExportOptions.JobWarrantyExternalLoggedDate), typeof(DateTime), r => r.JobMetaWarranty?.ExternalLoggedDate, csvNullableDateTimeEncoded) });
            metadata.Add(nameof(JobExportOptions.JobWarrantyExternalCompletedDate), new List<Metadata>() { new Metadata(nameof(JobExportOptions.JobWarrantyExternalCompletedDate), typeof(DateTime), r => r.JobMetaWarranty?.ExternalCompletedDate, csvNullableDateTimeEncoded) });

            // Non-Warranty
            metadata.Add(nameof(JobExportOptions.JobNonWarrantyAccountingChargeRequiredDate), new List<Metadata>() { new Metadata(nameof(JobExportOptions.JobNonWarrantyAccountingChargeRequiredDate), typeof(DateTime), r => r.JobMetaNonWarranty?.AccountingChargeRequiredDate, csvNullableDateTimeEncoded) });
            metadata.Add(nameof(JobExportOptions.JobNonWarrantyAccountingChargeAddedDate), new List<Metadata>() { new Metadata(nameof(JobExportOptions.JobNonWarrantyAccountingChargeAddedDate), typeof(DateTime), r => r.JobMetaNonWarranty?.AccountingChargeAddedDate, csvNullableDateTimeEncoded) });
            metadata.Add(nameof(JobExportOptions.JobNonWarrantyAccountingChargePaidDate), new List<Metadata>() { new Metadata(nameof(JobExportOptions.JobNonWarrantyAccountingChargePaidDate), typeof(DateTime), r => r.JobMetaNonWarranty?.AccountingChargePaidDate, csvNullableDateTimeEncoded) });
            metadata.Add(nameof(JobExportOptions.JobNonWarrantyPurchaseOrderRaisedDate), new List<Metadata>() { new Metadata(nameof(JobExportOptions.JobNonWarrantyPurchaseOrderRaisedDate), typeof(DateTime), r => r.JobMetaNonWarranty?.PurchaseOrderRaisedDate, csvNullableDateTimeEncoded) });
            metadata.Add(nameof(JobExportOptions.JobNonWarrantyPurchaseOrderReference), new List<Metadata>() { new Metadata(nameof(JobExportOptions.JobNonWarrantyPurchaseOrderReference), typeof(string), r => r.JobMetaNonWarranty?.PurchaseOrderReference, csvStringEncoded) });
            metadata.Add(nameof(JobExportOptions.JobNonWarrantyPurchaseOrderSentDate), new List<Metadata>() { new Metadata(nameof(JobExportOptions.JobNonWarrantyPurchaseOrderSentDate), typeof(DateTime), r => r.JobMetaNonWarranty?.PurchaseOrderSentDate, csvNullableDateTimeEncoded) });
            metadata.Add(nameof(JobExportOptions.JobNonWarrantyInvoiceReceivedDate), new List<Metadata>() { new Metadata(nameof(JobExportOptions.JobNonWarrantyInvoiceReceivedDate), typeof(DateTime), r => r.JobMetaNonWarranty?.InvoiceReceivedDate, csvNullableDateTimeEncoded) });
            metadata.Add(nameof(JobExportOptions.JobNonWarrantyRepairerName), new List<Metadata>() { new Metadata(nameof(JobExportOptions.JobNonWarrantyRepairerName), typeof(string), r => r.JobMetaNonWarranty?.RepairerName, csvStringEncoded) });
            metadata.Add(nameof(JobExportOptions.JobNonWarrantyRepairerLoggedDate), new List<Metadata>() { new Metadata(nameof(JobExportOptions.JobNonWarrantyRepairerLoggedDate), typeof(DateTime), r => r.JobMetaNonWarranty?.RepairerLoggedDate, csvNullableDateTimeEncoded) });
            metadata.Add(nameof(JobExportOptions.JobNonWarrantyRepairerReference), new List<Metadata>() { new Metadata(nameof(JobExportOptions.JobNonWarrantyRepairerReference), typeof(string), r => r.JobMetaNonWarranty?.RepairerReference, csvStringEncoded) });
            metadata.Add(nameof(JobExportOptions.JobNonWarrantyRepairerCompletedDate), new List<Metadata>() { new Metadata(nameof(JobExportOptions.JobNonWarrantyRepairerCompletedDate), typeof(DateTime), r => r.JobMetaNonWarranty?.RepairerCompletedDate, csvNullableDateTimeEncoded) });

            // Insurance
            metadata.Add(nameof(JobExportOptions.JobMetaInsuranceLossOrDamageDate), new List<Metadata>() { new Metadata(nameof(JobExportOptions.JobMetaInsuranceLossOrDamageDate), typeof(DateTime), r => r.JobMetaInsurance?.LossOrDamageDate, csvNullableDateTimeEncoded) });
            metadata.Add(nameof(JobExportOptions.JobMetaInsuranceEventLocation), new List<Metadata>() { new Metadata(nameof(JobExportOptions.JobMetaInsuranceEventLocation), typeof(string), r => r.JobMetaInsurance?.EventLocation, csvStringEncoded) });
            metadata.Add(nameof(JobExportOptions.JobMetaInsuranceDescription), new List<Metadata>() { new Metadata(nameof(JobExportOptions.JobMetaInsuranceDescription), typeof(string), r => r.JobMetaInsurance?.Description, csvStringEncoded) });
            metadata.Add(nameof(JobExportOptions.JobMetaInsuranceThirdPartyCausedName), new List<Metadata>() { new Metadata(nameof(JobExportOptions.JobMetaInsuranceThirdPartyCausedName), typeof(string), r => r.JobMetaInsurance?.ThirdPartyCausedName, csvStringEncoded) });
            metadata.Add(nameof(JobExportOptions.JobMetaInsuranceThirdPartyCausedWhy), new List<Metadata>() { new Metadata(nameof(JobExportOptions.JobMetaInsuranceThirdPartyCausedWhy), typeof(string), r => r.JobMetaInsurance?.ThirdPartyCausedWhy, csvStringEncoded) });
            metadata.Add(nameof(JobExportOptions.JobMetaInsuranceWitnessesNamesAddresses), new List<Metadata>() { new Metadata(nameof(JobExportOptions.JobMetaInsuranceWitnessesNamesAddresses), typeof(string), r => r.JobMetaInsurance?.WitnessesNamesAddresses, csvStringEncoded) });
            metadata.Add(nameof(JobExportOptions.JobMetaInsuranceBurglaryTheftMethodOfEntry), new List<Metadata>() { new Metadata(nameof(JobExportOptions.JobMetaInsuranceBurglaryTheftMethodOfEntry), typeof(string), r => r.JobMetaInsurance?.BurglaryTheftMethodOfEntry, csvStringEncoded) });
            metadata.Add(nameof(JobExportOptions.JobMetaInsurancePropertyLastSeenDate), new List<Metadata>() { new Metadata(nameof(JobExportOptions.JobMetaInsurancePropertyLastSeenDate), typeof(DateTime), r => r.JobMetaInsurance?.PropertyLastSeenDate, csvNullableDateTimeEncoded) });
            metadata.Add(nameof(JobExportOptions.JobMetaInsurancePoliceNotifiedStation), new List<Metadata>() { new Metadata(nameof(JobExportOptions.JobMetaInsurancePoliceNotifiedStation), typeof(string), r => r.JobMetaInsurance?.PoliceNotifiedStation, csvStringEncoded) });
            metadata.Add(nameof(JobExportOptions.JobMetaInsurancePoliceNotifiedDate), new List<Metadata>() { new Metadata(nameof(JobExportOptions.JobMetaInsurancePoliceNotifiedDate), typeof(DateTime), r => r.JobMetaInsurance?.PoliceNotifiedDate, csvNullableDateTimeEncoded) });
            metadata.Add(nameof(JobExportOptions.JobMetaInsurancePoliceNotifiedCrimeReportNo), new List<Metadata>() { new Metadata(nameof(JobExportOptions.JobMetaInsurancePoliceNotifiedCrimeReportNo), typeof(string), r => r.JobMetaInsurance?.PoliceNotifiedCrimeReportNo, csvStringEncoded) });
            metadata.Add(nameof(JobExportOptions.JobMetaInsuranceRecoverReduceAction), new List<Metadata>() { new Metadata(nameof(JobExportOptions.JobMetaInsuranceRecoverReduceAction), typeof(string), r => r.JobMetaInsurance?.RecoverReduceAction, csvStringEncoded) });
            metadata.Add(nameof(JobExportOptions.JobMetaInsuranceOtherInterestedParties), new List<Metadata>() { new Metadata(nameof(JobExportOptions.JobMetaInsuranceOtherInterestedParties), typeof(string), r => r.JobMetaInsurance?.OtherInterestedParties, csvStringEncoded) });
            metadata.Add(nameof(JobExportOptions.JobMetaInsuranceDateOfPurchase), new List<Metadata>() { new Metadata(nameof(JobExportOptions.JobMetaInsuranceDateOfPurchase), typeof(DateTime), r => r.JobMetaInsurance?.DateOfPurchase, csvNullableDateTimeEncoded) });
            metadata.Add(nameof(JobExportOptions.JobMetaInsuranceClaimFormSentDate), new List<Metadata>() { new Metadata(nameof(JobExportOptions.JobMetaInsuranceClaimFormSentDate), typeof(DateTime), r => r.JobMetaInsurance?.ClaimFormSentDate, csvNullableDateTimeEncoded) });
            metadata.Add(nameof(JobExportOptions.JobMetaInsuranceInsurer), new List<Metadata>() { new Metadata(nameof(JobExportOptions.JobMetaInsuranceInsurer), typeof(string), r => r.JobMetaInsurance?.Insurer, csvStringEncoded) });
            metadata.Add(nameof(JobExportOptions.JobMetaInsuranceInsurerReference), new List<Metadata>() { new Metadata(nameof(JobExportOptions.JobMetaInsuranceInsurerReference), typeof(string), r => r.JobMetaInsurance?.InsurerReference, csvStringEncoded) });

            // User Management
            metadata.Add(nameof(JobExportOptions.JobUserManagementFlags), new List<Metadata>() { new Metadata(nameof(JobExportOptions.JobUserManagementFlags), typeof(string), r => r.Job.Flags, csvToStringEncoded) });

            // User
            metadata.Add(nameof(JobExportOptions.UserId), new List<Metadata>() { new Metadata(nameof(JobExportOptions.UserId), typeof(string), r => r.User?.UserId, csvStringEncoded) });
            metadata.Add(nameof(JobExportOptions.UserDisplayName), new List<Metadata>() { new Metadata(nameof(JobExportOptions.UserDisplayName), typeof(string), r => r.User?.DisplayName, csvStringEncoded) });
            metadata.Add(nameof(JobExportOptions.UserSurname), new List<Metadata>() { new Metadata(nameof(JobExportOptions.UserSurname), typeof(string), r => r.User?.Surname, csvStringEncoded) });
            metadata.Add(nameof(JobExportOptions.UserGivenName), new List<Metadata>() { new Metadata(nameof(JobExportOptions.UserGivenName), typeof(string), r => r.User?.GivenName, csvStringEncoded) });
            metadata.Add(nameof(JobExportOptions.UserPhoneNumber), new List<Metadata>() { new Metadata(nameof(JobExportOptions.UserPhoneNumber), typeof(string), r => r.User?.PhoneNumber, csvStringEncoded) });
            metadata.Add(nameof(JobExportOptions.UserEmailAddress), new List<Metadata>() { new Metadata(nameof(JobExportOptions.UserEmailAddress), typeof(string), r => r.User?.EmailAddress, csvStringEncoded) });
            if (userDetailCustomKeys != null)
            {
                var assignedUserDetailCustomFields = new List<Metadata>();
                foreach (var detailKey in userDetailCustomKeys.OrderBy(k => k, StringComparer.OrdinalIgnoreCase))
                {
                    var key = detailKey;
                    assignedUserDetailCustomFields.Add(new Metadata(detailKey, detailKey, typeof(string), r => r.UserCustomDetails != null && r.UserCustomDetails.TryGetValue(key, out var value) ? value : null, csvStringEncoded));
                }
                metadata.Add(nameof(JobExportOptions.UserDetailCustom), assignedUserDetailCustomFields);
            }

            // Device
            metadata.Add(nameof(JobExportOptions.DeviceSerialNumber), new List<Metadata>() { new Metadata(nameof(JobExportOptions.DeviceSerialNumber), typeof(string), r => r.Device?.SerialNumber, csvStringEncoded) });
            metadata.Add(nameof(JobExportOptions.DeviceAssetNumber), new List<Metadata>() { new Metadata(nameof(JobExportOptions.DeviceAssetNumber), typeof(string), r => r.Device?.AssetNumber, csvStringEncoded) });
            metadata.Add(nameof(JobExportOptions.DeviceLocation), new List<Metadata>() { new Metadata(nameof(JobExportOptions.DeviceLocation), typeof(string), r => r.Device?.Location, csvStringEncoded) });
            metadata.Add(nameof(JobExportOptions.DeviceComputerName), new List<Metadata>() { new Metadata(nameof(JobExportOptions.DeviceComputerName), typeof(string), r => r.Device?.DeviceDomainId, csvStringEncoded) });
            metadata.Add(nameof(JobExportOptions.DeviceLastNetworkLogon), new List<Metadata>() { new Metadata(nameof(JobExportOptions.DeviceLastNetworkLogon), typeof(DateTime), r => r.Device?.LastNetworkLogonDate, csvNullableDateTimeEncoded) });
            metadata.Add(nameof(JobExportOptions.DeviceCreatedDate), new List<Metadata>() { new Metadata(nameof(JobExportOptions.DeviceCreatedDate), typeof(DateTime), r => r.Device?.CreatedDate, csvDateTimeEncoded) });
            metadata.Add(nameof(JobExportOptions.DeviceFirstEnrolledDate), new List<Metadata>() { new Metadata(nameof(JobExportOptions.DeviceFirstEnrolledDate), typeof(DateTime), r => r.Device?.EnrolledDate, csvNullableDateTimeEncoded) });
            metadata.Add(nameof(JobExportOptions.DeviceLastEnrolledDate), new List<Metadata>() { new Metadata(nameof(JobExportOptions.DeviceLastEnrolledDate), typeof(DateTime), r => r.Device?.LastEnrolDate, csvNullableDateTimeEncoded) });
            metadata.Add(nameof(JobExportOptions.DeviceAllowUnauthenticatedEnrol), new List<Metadata>() { new Metadata(nameof(JobExportOptions.DeviceAllowUnauthenticatedEnrol), typeof(bool), r => r.Device?.AllowUnauthenticatedEnrol, csvToStringEncoded) });
            metadata.Add(nameof(JobExportOptions.DeviceDecommissionedDate), new List<Metadata>() { new Metadata(nameof(JobExportOptions.DeviceDecommissionedDate), typeof(DateTime), r => r.Device?.DecommissionedDate, csvNullableDateTimeEncoded) });
            metadata.Add(nameof(JobExportOptions.DeviceDecommissionedReason), new List<Metadata>() { new Metadata(nameof(JobExportOptions.DeviceDecommissionedReason), typeof(string), r => r.Device?.DecommissionReason, csvToStringEncoded) });

            // Model
            metadata.Add(nameof(JobExportOptions.DeviceModelId), new List<Metadata>() { new Metadata(nameof(JobExportOptions.DeviceModelId), typeof(int), r => r.DeviceModelId, csvToStringEncoded) });
            metadata.Add(nameof(JobExportOptions.DeviceModelDescription), new List<Metadata>() { new Metadata(nameof(JobExportOptions.DeviceModelDescription), typeof(string), r => r.DeviceModelDescription, csvStringEncoded) });
            metadata.Add(nameof(JobExportOptions.DeviceModelManufacturer), new List<Metadata>() { new Metadata(nameof(JobExportOptions.DeviceModelManufacturer), typeof(string), r => r.DeviceModelManufacturer, csvStringEncoded) });
            metadata.Add(nameof(JobExportOptions.DeviceModelModel), new List<Metadata>() { new Metadata(nameof(JobExportOptions.DeviceModelModel), typeof(string), r => r.DeviceModelModel, csvStringEncoded) });
            metadata.Add(nameof(JobExportOptions.DeviceModelType), new List<Metadata>() { new Metadata(nameof(JobExportOptions.DeviceModelType), typeof(string), r => r.DeviceModelType, csvStringEncoded) });

            // Batch
            metadata.Add(nameof(JobExportOptions.DeviceBatchId), new List<Metadata>() { new Metadata(nameof(JobExportOptions.DeviceBatchId), typeof(int), r => r.DeviceBatchId, csvToStringEncoded) });
            metadata.Add(nameof(JobExportOptions.DeviceBatchName), new List<Metadata>() { new Metadata(nameof(JobExportOptions.DeviceBatchName), typeof(string), r => r.DeviceBatchName, csvStringEncoded) });
            metadata.Add(nameof(JobExportOptions.DeviceBatchPurchaseDate), new List<Metadata>() { new Metadata(nameof(JobExportOptions.DeviceBatchPurchaseDate), typeof(DateTime), r => r.DeviceBatchPurchaseDate, csvNullableDateEncoded) });
            metadata.Add(nameof(JobExportOptions.DeviceBatchSupplier), new List<Metadata>() { new Metadata(nameof(JobExportOptions.DeviceBatchSupplier), typeof(string), r => r.DeviceBatchSupplier, csvStringEncoded) });
            metadata.Add(nameof(JobExportOptions.DeviceBatchUnitCost), new List<Metadata>() { new Metadata(nameof(JobExportOptions.DeviceBatchUnitCost), typeof(decimal), r => r.DeviceBatchUnitCost, csvCurrencyEncoded) });
            metadata.Add(nameof(JobExportOptions.DeviceBatchWarrantyValidUntilDate), new List<Metadata>() { new Metadata(nameof(JobExportOptions.DeviceBatchWarrantyValidUntilDate), typeof(DateTime), r => r.DeviceBatchWarrantyValidUntilDate, csvNullableDateEncoded) });
            metadata.Add(nameof(JobExportOptions.DeviceBatchInsuredDate), new List<Metadata>() { new Metadata(nameof(JobExportOptions.DeviceBatchInsuredDate), typeof(DateTime), r => r.DeviceBatchInsuredDate, csvNullableDateEncoded) });
            metadata.Add(nameof(JobExportOptions.DeviceBatchInsuranceSupplier), new List<Metadata>() { new Metadata(nameof(JobExportOptions.DeviceBatchInsuranceSupplier), typeof(string), r => r.DeviceBatchInsuranceSupplier, csvStringEncoded) });
            metadata.Add(nameof(JobExportOptions.DeviceBatchInsuredUntilDate), new List<Metadata>() { new Metadata(nameof(JobExportOptions.DeviceBatchInsuredUntilDate), typeof(DateTime), r => r.DeviceBatchInsuredUntilDate, csvNullableDateEncoded) });

            // Profile
            metadata.Add(nameof(JobExportOptions.DeviceProfileId), new List<Metadata>() { new Metadata(nameof(JobExportOptions.DeviceProfileId), typeof(int), r => r.DeviceProfileId, csvToStringEncoded) });
            metadata.Add(nameof(JobExportOptions.DeviceProfileName), new List<Metadata>() { new Metadata(nameof(JobExportOptions.DeviceProfileName), typeof(string), r => r.DeviceProfileName, csvStringEncoded) });
            metadata.Add(nameof(JobExportOptions.DeviceProfileShortName), new List<Metadata>() { new Metadata(nameof(JobExportOptions.DeviceProfileShortName), typeof(string), r => r.DeviceProfileShortName, csvStringEncoded) });

            return metadata;
        }

    }
}
