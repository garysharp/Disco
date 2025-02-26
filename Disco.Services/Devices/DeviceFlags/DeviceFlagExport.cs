using Disco.Data.Repository;
using Disco.Models.Exporting;
using Disco.Models.Services.Devices.DeviceFlag;
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

namespace Disco.Services.Devices.DeviceFlags
{
    public class DeviceFlagExport : IExport<DeviceFlagExportOptions, DeviceFlagExportRecord>
    {
        public Guid Id { get; set; }
        public string Name { get; } = "Device Flag Export";
        public DeviceFlagExportOptions Options { get; set; }

        public string FilenamePrefix { get; } = "DeviceFlagExport";
        public string ExcelWorksheetName { get; } = "DeviceFlagExport";
        public string ExcelTableName { get; } = "DeviceFlags";

        public DeviceFlagExport(DeviceFlagExportOptions options)
        {
            Id = Guid.NewGuid();
            Options = options;
        }

        [JsonConstructor]
        public DeviceFlagExport()
            : this(DeviceFlagExportOptions.DefaultOptions())
        {
        }

        public ExportResult Export(DiscoDataContext database, IScheduledTaskStatus status)
            => Exporter.Export(this, database, status);

        public List<DeviceFlagExportRecord> BuildRecords(DiscoDataContext database, IScheduledTaskStatus status)
        {
            var query = database.DeviceFlagAssignments
                .Include(a => a.DeviceFlag);

            if (Options.HasDeviceOptions())
                query = query.Include(a => a.Device);
            if (Options.HasDeviceModelOptions())
                query = query.Include(a => a.Device.DeviceModel);
            if (Options.HasDeviceBatchOptions())
                query = query.Include(a => a.Device.DeviceBatch);
            if (Options.HasDeviceProfileOptions())
                query = query.Include(a => a.Device.DeviceProfile);
            if (Options.HasAssignedUserOptions())
                query = query.Include(a => a.Device.AssignedUser);
            if (Options.UserDetailCustom?.Any() ?? false)
                query = query.Include(a => a.Device.AssignedUser.UserDetails);

            query = query.Where(a => Options.DeviceFlagIds.Contains(a.DeviceFlagId));

            if (Options.CurrentOnly)
            {
                query = query.Where(a => !a.RemovedDate.HasValue);
            }

            // Update Users
            if (Options.HasAssignedUserOptions())
            {
                status.UpdateStatus(5, "Refreshing user details from Active Directory");
                var userIds = query.Where(d => d.Device.AssignedUserId != null).Select(d => d.Device.AssignedUserId).Distinct().ToList();
                foreach (var userId in userIds)
                {
                    try
                    {
                        UserService.GetUser(userId, database, true);
                    }
                    catch (Exception) { } // Ignore Errors
                }
            }

            status.UpdateStatus(15, "Extracting records from the database");
            var assignments = query.ToList();
            var records = assignments.Select(a => new DeviceFlagExportRecord()
            {
                Assignment = a
            }).ToList();

            if (Options.UserDetailCustom?.Any() ?? false)
            {
                status.UpdateStatus(50, "Extracting custom user detail records");

                var detailsService = new DetailsProviderService(database);
                var cache = new Dictionary<string, Dictionary<string, string>>(StringComparer.Ordinal);
                foreach (var record in records)
                {
                    var userId = record.Assignment.Device.AssignedUserId;

                    if (userId == null)
                        continue;

                    if (!cache.TryGetValue(userId, out var details))
                        details = detailsService.GetDetails(record.Assignment.Device.AssignedUser);
                    record.AssignedUserCustomDetails = details;
                }
            }

            return records;
        }

        public ExportMetadata<DeviceFlagExportOptions, DeviceFlagExportRecord> BuildMetadata(DiscoDataContext database, List<DeviceFlagExportRecord> records, IScheduledTaskStatus status)
        {
            var metadata = new ExportMetadata<DeviceFlagExportOptions, DeviceFlagExportRecord>(Options);
            metadata.IgnoreGroupNames.Add("Device Flag");

            // Device Flag
            metadata.Add(o => o.Id, r => r.Assignment.DeviceFlagId);
            metadata.Add(o => o.Name, r => r.Assignment.DeviceFlag.Name);
            metadata.Add(o => o.Description, r => r.Assignment.DeviceFlag.Description);
            metadata.Add(o => o.Icon, r => r.Assignment.DeviceFlag.Icon);
            metadata.Add(o => o.IconColour, r => r.Assignment.DeviceFlag.IconColour);
            metadata.Add(o => o.AssignmentId, r => r.Assignment.Id);
            metadata.Add(o => o.AddedDate, r => r.Assignment.AddedDate);
            metadata.Add(o => o.AddedUserId, r => r.Assignment.AddedUserId);
            metadata.Add(o => o.RemovedUserId, r => r.Assignment.RemovedUserId);
            metadata.Add(o => o.RemovedDate, r => r.Assignment.RemovedDate);
            metadata.Add(o => o.Comments, r => r.Assignment.Comments);

            // Device
            metadata.Add(o => o.DeviceSerialNumber, r => r.Assignment.Device.SerialNumber);
            metadata.Add(o => o.DeviceAssetNumber, r => r.Assignment.Device.AssetNumber);
            metadata.Add(o => o.DeviceLocation, r => r.Assignment.Device.Location);
            metadata.Add(o => o.DeviceComputerName, r => r.Assignment.Device.DeviceDomainId);
            metadata.Add(o => o.DeviceLastNetworkLogon, r => r.Assignment.Device.LastNetworkLogonDate);
            metadata.Add(o => o.DeviceCreatedDate, r => r.Assignment.Device.CreatedDate);
            metadata.Add(o => o.DeviceFirstEnrolledDate, r => r.Assignment.Device.EnrolledDate);
            metadata.Add(o => o.DeviceLastEnrolledDate, r => r.Assignment.Device.LastEnrolDate);
            metadata.Add(o => o.DeviceAllowUnauthenticatedEnrol, r => r.Assignment.Device.AllowUnauthenticatedEnrol);
            metadata.Add(o => o.DeviceDecommissionedDate, r => r.Assignment.Device.DecommissionedDate);
            metadata.Add(o => o.DeviceDecommissionedReason, r => r.Assignment.Device.DecommissionReason?.ToString());

            // Model
            metadata.Add(o => o.ModelId, r => r.Assignment.Device.DeviceModel.Id);
            metadata.Add(o => o.ModelDescription, r => r.Assignment.Device.DeviceModel.Description);
            metadata.Add(o => o.ModelManufacturer, r => r.Assignment.Device.DeviceModel.Manufacturer);
            metadata.Add(o => o.ModelModel, r => r.Assignment.Device.DeviceModel.Model);
            metadata.Add(o => o.ModelType, r => r.Assignment.Device.DeviceModel.ModelType);

            // Batch
            metadata.Add(o => o.BatchId, r => r.Assignment.Device.DeviceBatch?.Id);
            metadata.Add(o => o.BatchName, r => r.Assignment.Device.DeviceBatch?.Name);
            metadata.Add(o => o.BatchPurchaseDate, r => r.Assignment.Device.DeviceBatch?.PurchaseDate);
            metadata.Add(o => o.BatchSupplier, r => r.Assignment.Device.DeviceBatch?.Supplier);
            metadata.Add(o => o.BatchUnitCost, r => r.Assignment.Device.DeviceBatch?.UnitCost, Exporter.CsvEncoders.NullableCurrencyEncoder);
            metadata.Add(o => o.BatchWarrantyValidUntilDate, r => r.Assignment.Device.DeviceBatch?.WarrantyValidUntil);
            metadata.Add(o => o.BatchInsuredDate, r => r.Assignment.Device.DeviceBatch?.InsuredDate);
            metadata.Add(o => o.BatchInsuranceSupplier, r => r.Assignment.Device.DeviceBatch?.InsuranceSupplier);
            metadata.Add(o => o.BatchInsuredUntilDate, r => r.Assignment.Device.DeviceBatch?.InsuredUntil);

            // Profile
            metadata.Add(o => o.ProfileId, r => r.Assignment.Device.DeviceProfile?.Id);
            metadata.Add(o => o.ProfileName, r => r.Assignment.Device.DeviceProfile?.Name);
            metadata.Add(o => o.ProfileShortName, r => r.Assignment.Device.DeviceProfile?.ShortName);

            // User
            metadata.Add(o => o.AssignedUserId, r => r.Assignment.Device?.AssignedUser?.UserId);
            metadata.Add(o => o.AssignedUserDisplayName, r => r.Assignment.Device?.AssignedUser?.DisplayName);
            metadata.Add(o => o.AssignedUserSurname, r => r.Assignment.Device?.AssignedUser?.Surname);
            metadata.Add(o => o.AssignedUserGivenName, r => r.Assignment.Device?.AssignedUser?.GivenName);
            metadata.Add(o => o.AssignedUserPhoneNumber, r => r.Assignment.Device?.AssignedUser?.PhoneNumber);
            metadata.Add(o => o.AssignedUserEmailAddress, r => r.Assignment.Device?.AssignedUser?.EmailAddress);

            // User Custom Details
            if (Options.UserDetailCustom?.Any() ?? false)
            {
                foreach (var key in Options.UserDetailCustom.OrderBy(k => k, StringComparer.OrdinalIgnoreCase))
                    metadata.Add($"Assigned User Detail {key.TrimEnd('*', '&')}", r => r.AssignedUserCustomDetails != null && r.AssignedUserCustomDetails.TryGetValue(key, out var value) ? value : null);
            }

            return metadata;
        }
    }
}
