using Disco.Data.Repository;
using Disco.Models.Exporting;
using Disco.Models.Repository;
using Disco.Models.Services.Documents;
using Disco.Models.Services.Exporting;
using Disco.Services.Exporting;
using Disco.Services.Plugins.Features.DetailsProvider;
using Disco.Services.Tasks;
using Disco.Services.Users;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace Disco.Services.Documents
{
    public class DocumentExport : IExport<DocumentExportOptions, DocumentExportRecord>
    {
        public Guid Id { get; set; }
        public string Name { get; } = "Document Export";
        public DocumentExportOptions Options { get; set; }

        public string FilenamePrefix { get; } = "DocumentExport";
        public string ExcelWorksheetName { get; } = "DocumentExport";
        public string ExcelTableName { get; } = "Documents";

        public DocumentExport(DocumentExportOptions options)
        {
            Id = Guid.NewGuid();
            Options = options;
        }

        [JsonConstructor]
        public DocumentExport()
            : this(DocumentExportOptions.DefaultOptions())
        {
        }

        public ExportMetadata<DocumentExportOptions, DocumentExportRecord> BuildMetadata(DiscoDataContext database, List<DocumentExportRecord> records, IScheduledTaskStatus status)
        {
            var metadata = new ExportMetadata<DocumentExportOptions, DocumentExportRecord>(Options);

            // Document Template
            metadata.Add(o => o.Id, r => r.DocumentTemplate.Id);
            metadata.Add(o => o.Description, r => r.DocumentTemplate?.Description);
            metadata.Add(o => o.Scope, r => r.DocumentTemplate?.Scope);

            // Attachment
            metadata.Add(o => o.AttachmentId, r => r.Attachment.Id);
            metadata.Add(o => o.AttachmentCreatedDate, r => r.Attachment.Timestamp);
            metadata.Add(o => o.AttachmentCreatedUser, r => r.Attachment.TechUserId);
            metadata.Add(o => o.AttachmentFilename, r => r.Attachment.Filename);
            metadata.Add(o => o.AttachmentMimeType, r => r.Attachment.MimeType);
            metadata.Add(o => o.AttachmentComments, r => r.Attachment.Comments);

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
            metadata.Add(o => o.ModelId, r => r.Device?.DeviceModel?.Id);
            metadata.Add(o => o.ModelDescription, r => r.Device?.DeviceModel?.Description);
            metadata.Add(o => o.ModelManufacturer, r => r.Device?.DeviceModel?.Manufacturer);
            metadata.Add(o => o.ModelModel, r => r.Device?.DeviceModel?.Model);
            metadata.Add(o => o.ModelType, r => r.Device?.DeviceModel?.ModelType);

            // Batch
            metadata.Add(o => o.BatchId, r => r.Device?.DeviceBatch?.Id);
            metadata.Add(o => o.BatchName, r => r.Device?.DeviceBatch?.Name);
            metadata.Add(o => o.BatchPurchaseDate, r => r.Device?.DeviceBatch?.PurchaseDate);
            metadata.Add(o => o.BatchSupplier, r => r.Device?.DeviceBatch?.Supplier);
            metadata.Add(o => o.BatchUnitCost, r => r.Device?.DeviceBatch?.UnitCost, Exporter.CsvEncoders.NullableCurrencyEncoder);
            metadata.Add(o => o.BatchWarrantyValidUntilDate, r => r.Device?.DeviceBatch?.WarrantyValidUntil);
            metadata.Add(o => o.BatchInsuredDate, r => r.Device?.DeviceBatch?.InsuredDate);
            metadata.Add(o => o.BatchInsuranceSupplier, r => r.Device?.DeviceBatch?.InsuranceSupplier);
            metadata.Add(o => o.BatchInsuredUntilDate, r => r.Device?.DeviceBatch?.InsuredUntil);

            // Profile
            metadata.Add(o => o.ProfileId, r => r.Device?.DeviceProfile?.Id);
            metadata.Add(o => o.ProfileName, r => r.Device?.DeviceProfile?.Name);
            metadata.Add(o => o.ProfileShortName, r => r.Device?.DeviceProfile?.ShortName);

            // Job
            metadata.Add(o => o.JobId, r => r.Job?.Id);
            metadata.Add(o => o.JobStatus, r => r.JobStatus == null ? null : Job.JobStatusIds.StatusDescriptions.TryGetValue(r.JobStatus, out var jobStatus) ? jobStatus : "Unknown");
            metadata.Add(o => o.JobType, r => r.JobTypeDescription);
            metadata.Add(o => o.JobSubTypes, r => r.JobSubTypeDescriptions == null ? null : string.Join(", ", r.JobSubTypeDescriptions));
            metadata.Add(o => o.JobOpenedDate, r => r.Job?.OpenedDate);
            metadata.Add(o => o.JobOpenedUser, r => r.Job?.OpenedTechUserId);
            metadata.Add(o => o.JobExpectedClosedDate, r => r.Job?.ExpectedClosedDate);
            metadata.Add(o => o.JobClosedDate, r => r.Job?.ClosedDate);
            metadata.Add(o => o.JobClosedUser, r => r.Job?.ClosedTechUserId);

            // User
            metadata.Add(o => o.UserId, r => r.User?.UserId);
            metadata.Add(o => o.UserDisplayName, r => r.User?.DisplayName);
            metadata.Add(o => o.UserSurname, r => r.User?.Surname);
            metadata.Add(o => o.UserGivenName, r => r.User?.GivenName);
            metadata.Add(o => o.UserPhoneNumber, r => r.User?.PhoneNumber);
            metadata.Add(o => o.UserEmailAddress, r => r.User?.EmailAddress);

            // User Custom Details
            if (Options.UserDetailCustom?.Any() ?? false)
            {
                foreach (var key in Options.UserDetailCustom.OrderBy(k => k, StringComparer.OrdinalIgnoreCase))
                    metadata.Add($"User Detail {key.TrimEnd('*', '&')}", r => r.UserCustomDetails != null && r.UserCustomDetails.TryGetValue(key, out var value) ? value : null);
            }

            return metadata;
        }

        public List<DocumentExportRecord> BuildRecords(DiscoDataContext database, IScheduledTaskStatus status)
        {
            var records = new List<DocumentExportRecord>();
            var documentTemplates = database.DocumentTemplates.Where(t => Options.DocumentTemplateIds.Contains(t.Id)).ToList();

            foreach (var documentTemplate in documentTemplates)
            {
                var documentRecords = BuildDocumentRecords(database, documentTemplate, status);
                records.AddRange(documentRecords);
            }

            return records;
        }

        private List<DocumentExportRecord> BuildDocumentRecords(DiscoDataContext database, DocumentTemplate document, IScheduledTaskStatus status)
        {
            switch (document.AttachmentType)
            {
                case AttachmentTypes.Device:
                    return BuildDeviceDocumentRecords(database, document, status);
                case AttachmentTypes.Job:
                    return BuildJobDocumentRecords(database, document, status);
                case AttachmentTypes.User:
                    return BuildUserDocumentRecords(database, document, status);
                default:
                    throw new NotSupportedException($"Unsupported document scope: {document.Scope}");
            }
        }

        private List<DocumentExportRecord> BuildDeviceDocumentRecords(DiscoDataContext database, DocumentTemplate document, IScheduledTaskStatus status)
        {
            var query = database.DeviceAttachments
                .Include(a => a.Device);
            if (Options.HasDeviceBatchOptions())
                query = query.Include(a => a.Device.DeviceBatch);
            if (Options.HasDeviceModelOptions())
                query = query.Include(a => a.Device.DeviceModel);
            if (Options.HasDeviceProfileOptions())
                query = query.Include(a => a.Device.DeviceProfile);
            if (Options.HasUserOptions())
                query = query.Include(a => a.Device.AssignedUser);

            query = query.Where(a => a.DocumentTemplateId == document.Id);

            if (Options.HasUserOptions())
                RefreshAdUsers(database, query.Select(d => d.Device.AssignedUserId).Distinct().ToList(), status);

            status.UpdateStatus(15, "Extracting records from the database");
            var attachments = query.OrderBy(a => a.Timestamp).ToList();

            if (Options.LatestOnly)
            {
                attachments.Reverse();
                attachments = attachments.GroupBy(a => a.DeviceSerialNumber).Select(g => g.First()).OrderBy(a => a.Timestamp).ToList();
            }

            var records = attachments.Select(a => new DocumentExportRecord()
            {
                DocumentTemplate = document,
                Attachment = a,
                Device = a.Device,
                Job = null,
                User = a.Device.AssignedUser,
            }).ToList();

            if (Options.UserDetailCustom.Any())
                AddUserCustomDetails(database, records, status);

            return records;
        }

        private List<DocumentExportRecord> BuildJobDocumentRecords(DiscoDataContext database, DocumentTemplate document, IScheduledTaskStatus status)
        {
            var query = database.JobAttachments
                .Include(a => a.Job);
            if (Options.JobStatus)
            {
                query = query
                    .Include(a => a.Job.JobMetaWarranty)
                    .Include(a => a.Job.JobMetaNonWarranty)
                    .Include(a => a.Job.JobMetaInsurance);
            }
            if (Options.JobType)
                query = query.Include(a => a.Job.JobType);
            if (Options.JobSubTypes)
                query = query.Include(a => a.Job.JobSubTypes);
            if (Options.HasDeviceOptions())
                query = query.Include(a => a.Job.Device);
            if (Options.HasDeviceBatchOptions())
                query = query.Include(a => a.Job.Device.DeviceBatch);
            if (Options.HasDeviceModelOptions())
                query = query.Include(a => a.Job.Device.DeviceModel);
            if (Options.HasDeviceProfileOptions())
                query = query.Include(a => a.Job.Device.DeviceProfile);
            if (Options.HasUserOptions())
                query = query.Include(a => a.Job.User);

            query = query.Where(a => a.DocumentTemplateId == document.Id);

            if (Options.HasUserOptions())
                RefreshAdUsers(database, query.Select(d => d.Job.UserId).Distinct().ToList(), status);

            status.UpdateStatus(15, "Extracting records from the database");
            var attachments = query.OrderBy(a => a.Timestamp).ToList();

            if (Options.LatestOnly)
            {
                attachments.Reverse();
                attachments = attachments.GroupBy(a => a.JobId).Select(g => g.First()).OrderBy(a => a.Timestamp).ToList();
            }

            var records = attachments.Select(a => new DocumentExportRecord()
            {
                DocumentTemplate = document,
                Attachment = a,
                Device = a.Job.Device,
                Job = a.Job,
                User = a.Job.User,
            }).ToList();

            if (Options.UserDetailCustom.Any())
                AddUserCustomDetails(database, records, status);

            return records;
        }

        private List<DocumentExportRecord> BuildUserDocumentRecords(DiscoDataContext database, DocumentTemplate document, IScheduledTaskStatus status)
        {
            var query = database.UserAttachments
                .Include(a => a.User);
            
            if (Options.HasDeviceOptions())
                query = query.Include(a => a.User.DeviceUserAssignments.Select(u => u.Device));
            if (Options.HasDeviceBatchOptions())
                query = query.Include(a => a.User.DeviceUserAssignments.Select(u => u.Device.DeviceBatch));
            if (Options.HasDeviceModelOptions())
                query = query.Include(a => a.User.DeviceUserAssignments.Select(u => u.Device.DeviceModel));
            if (Options.HasDeviceProfileOptions())
                query = query.Include(a => a.User.DeviceUserAssignments.Select(u => u.Device.DeviceProfile));

            query = query.Where(a => a.DocumentTemplateId == document.Id);

            if (Options.HasUserOptions())
                RefreshAdUsers(database, query.Select(d => d.UserId).Distinct().ToList(), status);

            status.UpdateStatus(15, "Extracting records from the database");
            var attachments = query.OrderBy(a => a.Timestamp).ToList();

            if (Options.LatestOnly)
            {
                attachments.Reverse();
                attachments = attachments.GroupBy(a => a.UserId).Select(g => g.First()).OrderBy(a => a.Timestamp).ToList();
            }

            var records = attachments.Select(a => new DocumentExportRecord()
            {
                DocumentTemplate = document,
                Attachment = a,
                Device = a.User.DeviceUserAssignments?
                    .Where(u => !u.UnassignedDate.HasValue)
                    .OrderByDescending(u => u.AssignedDate)
                    .FirstOrDefault()?.Device,
                Job = null,
                User = a.User,
            }).ToList();

            if (Options.UserDetailCustom.Any())
                AddUserCustomDetails(database, records, status);

            return records;
        }

        private static void RefreshAdUsers(DiscoDataContext database, List<string> userIds, IScheduledTaskStatus status)
        {
            if (!userIds.Any())
                return;

            status.UpdateStatus(5, "Refreshing user details from Active Directory");
            foreach (var userId in userIds)
            {
                if (string.IsNullOrWhiteSpace(userId))
                    continue;
                try
                {
                    UserService.GetUser(userId, database, true);
                }
                catch (Exception) { } // Ignore Errors
            }
        }

        private static void AddUserCustomDetails(DiscoDataContext database, List<DocumentExportRecord> records, IScheduledTaskStatus status)
        {
            if (!records.Any(r => r.User != null))
                return;
            status.UpdateStatus(50, "Extracting custom user detail records");
            var detailsService = new DetailsProviderService(database);
            var cache = new Dictionary<string, Dictionary<string, string>>(StringComparer.Ordinal);
            foreach (var record in records)
            {
                var userId = record.User?.UserId;
                if (string.IsNullOrWhiteSpace(userId))
                    continue;
                if (!cache.TryGetValue(userId, out var details))
                    details = detailsService.GetDetails(record.User);
                record.UserCustomDetails = details;
            }
        }

        public ExportResult Export(DiscoDataContext database, IScheduledTaskStatus status)
            => Exporter.Export(this, database, status);
    }
}
