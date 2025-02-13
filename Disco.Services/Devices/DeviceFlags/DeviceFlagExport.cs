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
            if (Options.AssignedUserDetailCustom)
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

            if (Options.AssignedUserDetailCustom)
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

        public ExportMetadata<DeviceFlagExportRecord> BuildMetadata(DiscoDataContext database, List<DeviceFlagExportRecord> records, IScheduledTaskStatus status)
        {
            var metadata = new ExportMetadata<DeviceFlagExportRecord>();
            metadata.IgnoreShortNames.Add("Device Flag");

            // Device Flag
            metadata.Add(Options, o => o.Id, r => r.Assignment.DeviceFlagId);
            metadata.Add(Options, o => o.Name, r => r.Assignment.DeviceFlag.Name);
            metadata.Add(Options, o => o.Description, r => r.Assignment.DeviceFlag.Description);
            metadata.Add(Options, o => o.Icon, r => r.Assignment.DeviceFlag.Icon);
            metadata.Add(Options, o => o.IconColour, r => r.Assignment.DeviceFlag.IconColour);
            metadata.Add(Options, o => o.AssignmentId, r => r.Assignment.Id);
            metadata.Add(Options, o => o.AddedDate, r => r.Assignment.AddedDate);
            metadata.Add(Options, o => o.AddedUserId, r => r.Assignment.AddedUserId);
            metadata.Add(Options, o => o.RemovedUserId, r => r.Assignment.RemovedUserId);
            metadata.Add(Options, o => o.RemovedDate, r => r.Assignment.RemovedDate);
            metadata.Add(Options, o => o.Comments, r => r.Assignment.Comments);

            // Device
            metadata.Add(Options, o => o.DeviceSerialNumber, r => r.Assignment.Device.SerialNumber);
            metadata.Add(Options, o => o.DeviceAssetNumber, r => r.Assignment.Device.AssetNumber);
            metadata.Add(Options, o => o.DeviceLocation, r => r.Assignment.Device.Location);
            metadata.Add(Options, o => o.DeviceComputerName, r => r.Assignment.Device.DeviceDomainId);
            metadata.Add(Options, o => o.DeviceLastNetworkLogon, r => r.Assignment.Device.LastNetworkLogonDate);
            metadata.Add(Options, o => o.DeviceCreatedDate, r => r.Assignment.Device.CreatedDate);
            metadata.Add(Options, o => o.DeviceFirstEnrolledDate, r => r.Assignment.Device.EnrolledDate);
            metadata.Add(Options, o => o.DeviceLastEnrolledDate, r => r.Assignment.Device.LastEnrolDate);
            metadata.Add(Options, o => o.DeviceAllowUnauthenticatedEnrol, r => r.Assignment.Device.AllowUnauthenticatedEnrol);
            metadata.Add(Options, o => o.DeviceDecommissionedDate, r => r.Assignment.Device.DecommissionedDate);
            metadata.Add(Options, o => o.DeviceDecommissionedReason, r => r.Assignment.Device.DecommissionReason?.ToString());

            // Model
            metadata.Add(Options, o => o.ModelId, r => r.Assignment.Device.DeviceModel.Id);
            metadata.Add(Options, o => o.ModelDescription, r => r.Assignment.Device.DeviceModel.Description);
            metadata.Add(Options, o => o.ModelManufacturer, r => r.Assignment.Device.DeviceModel.Manufacturer);
            metadata.Add(Options, o => o.ModelModel, r => r.Assignment.Device.DeviceModel.Model);
            metadata.Add(Options, o => o.ModelType, r => r.Assignment.Device.DeviceModel.ModelType);

            // Batch
            metadata.Add(Options, o => o.BatchId, r => r.Assignment.Device.DeviceBatch?.Id);
            metadata.Add(Options, o => o.BatchName, r => r.Assignment.Device.DeviceBatch?.Name);
            metadata.Add(Options, o => o.BatchPurchaseDate, r => r.Assignment.Device.DeviceBatch?.PurchaseDate);
            metadata.Add(Options, o => o.BatchSupplier, r => r.Assignment.Device.DeviceBatch?.Supplier);
            metadata.Add(Options, o => o.BatchUnitCost, r => r.Assignment.Device.DeviceBatch?.UnitCost, Exporter.CsvEncoders.NullableCurrencyEncoder);
            metadata.Add(Options, o => o.BatchWarrantyValidUntilDate, r => r.Assignment.Device.DeviceBatch?.WarrantyValidUntil);
            metadata.Add(Options, o => o.BatchInsuredDate, r => r.Assignment.Device.DeviceBatch?.InsuredDate);
            metadata.Add(Options, o => o.BatchInsuranceSupplier, r => r.Assignment.Device.DeviceBatch?.InsuranceSupplier);
            metadata.Add(Options, o => o.BatchInsuredUntilDate, r => r.Assignment.Device.DeviceBatch?.InsuredUntil);

            // Profile
            metadata.Add(Options, o => o.ProfileId, r => r.Assignment.Device.DeviceProfile?.Id);
            metadata.Add(Options, o => o.ProfileName, r => r.Assignment.Device.DeviceProfile?.Name);
            metadata.Add(Options, o => o.ProfileShortName, r => r.Assignment.Device.DeviceProfile?.ShortName);

            // User
            metadata.Add(Options, o => o.AssignedUserId, r => r.Assignment.Device?.AssignedUser?.UserId);
            metadata.Add(Options, o => o.AssignedUserDisplayName, r => r.Assignment.Device?.AssignedUser?.DisplayName);
            metadata.Add(Options, o => o.AssignedUserSurname, r => r.Assignment.Device?.AssignedUser?.Surname);
            metadata.Add(Options, o => o.AssignedUserGivenName, r => r.Assignment.Device?.AssignedUser?.GivenName);
            metadata.Add(Options, o => o.AssignedUserPhoneNumber, r => r.Assignment.Device?.AssignedUser?.PhoneNumber);
            metadata.Add(Options, o => o.AssignedUserEmailAddress, r => r.Assignment.Device?.AssignedUser?.EmailAddress);

            // User Custom Details
            if (Options.AssignedUserDetailCustom)
            {
                var keys = records.Where(r => r.AssignedUserCustomDetails != null).SelectMany(r => r.AssignedUserCustomDetails.Keys).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
                foreach (var key in keys.OrderBy(k => k, StringComparer.OrdinalIgnoreCase))
                {
                    metadata.Add(key, r => r.AssignedUserCustomDetails != null && r.AssignedUserCustomDetails.TryGetValue(key, out var value) ? value : null);
                }
            }

            return metadata;
        }
    }
}
