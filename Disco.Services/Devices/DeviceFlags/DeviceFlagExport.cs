using Disco.Data.Repository;
using Disco.Models.Exporting;
using Disco.Models.Services.Devices.DeviceFlag;
using Disco.Models.Services.Exporting;
using Disco.Services.Plugins.Features.DetailsProvider;
using Disco.Services.Tasks;
using Disco.Services.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;

namespace Disco.Services.Devices.DeviceFlags
{
    using Metadata = ExportFieldMetadata<DeviceFlagExportRecord>;

    public class DeviceFlagExport
    {
        private readonly DiscoDataContext database;
        private readonly DeviceFlagExportOptions options;

        public DeviceFlagExport(DiscoDataContext database, DeviceFlagExportOptions options)
        {
            this.database = database;
            this.options = options;
        }

        public ExportResult Generate(IScheduledTaskStatus status)
        {
            var records = BuildRecords(status);

            var metadata = BuildMetadata(records, status);

            if (metadata.Count == 0)
                throw new ArgumentException("At least one export field must be specified", nameof(options));

            status.UpdateStatus(90, $"Formatting {records.Count} records for export");
            return ExportHelpers.WriteExport(options, status, metadata, records);
        }

        private List<DeviceFlagExportRecord> BuildRecords(IScheduledTaskStatus status)
        {
            var query = database.DeviceFlagAssignments
                .Include(a => a.DeviceFlag);

            if (options.HasDeviceOptions())
                query = query.Include(a => a.Device);
            if (options.HasDeviceModelOptions())
                query = query.Include(a => a.Device.DeviceModel);
            if (options.HasDeviceBatchOptions())
                query = query.Include(a => a.Device.DeviceBatch);
            if (options.HasDeviceProfileOptions())
                query = query.Include(a => a.Device.DeviceProfile);
            if (options.HasAssignedUserOptions())
                query = query.Include(a => a.Device.AssignedUser);
            if (options.AssignedUserDetailCustom)
                query = query.Include(a => a.Device.AssignedUser.UserDetails);

            query = query.Where(a => options.DeviceFlagIds.Contains(a.DeviceFlagId));

            if (options.CurrentOnly)
            {
                query = query.Where(a => !a.RemovedDate.HasValue);
            }

            // Update Users
            if (options.HasAssignedUserOptions())
            {
                status.UpdateStatus(5, "Refreshing user details from Active Directory");
                var userIds = query.Where(d => d.Device.AssignedUserId != null).Select(d => d.Device.AssignedUserId).Distinct().ToList();
                foreach (var userId in userIds)
                {
                    try
                    {
                        UserService.GetUser(userId, database);
                    }
                    catch (Exception) { } // Ignore Errors
                }
            }

            status.UpdateStatus(15, "Extracting records from the database");

            var records = query.Select(a => new DeviceFlagExportRecord()
            {
                Assignment = a
            }).ToList();

            if (options.AssignedUserDetailCustom)
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

        private List<Metadata> BuildMetadata(List<DeviceFlagExportRecord> records, IScheduledTaskStatus status)
        {
            status.UpdateStatus(80, "Building metadata");

            IEnumerable<string> userDetailCustomKeys = null;
            if (options.AssignedUserDetailCustom)
                userDetailCustomKeys = records.Where(r => r.AssignedUserCustomDetails != null).SelectMany(r => r.AssignedUserCustomDetails.Keys).Distinct(StringComparer.OrdinalIgnoreCase).ToList();

            var accessors = BuildAccessors(userDetailCustomKeys);

            return typeof(DeviceFlagExportOptions).GetProperties()
                .Where(p => p.PropertyType == typeof(bool))
                .Select(p => new
                {
                    property = p,
                    details = (DisplayAttribute)p.GetCustomAttributes(typeof(DisplayAttribute), false).FirstOrDefault()
                })
                .Where(p => p.details != null && p.property.Name != nameof(options.CurrentOnly) && (bool)p.property.GetValue(options))
                .SelectMany(p =>
                {
                    var fieldMetadata = accessors[p.property.Name];
                    fieldMetadata.ForEach(f =>
                    {
                        if (f.ColumnName == null)
                            f.ColumnName = (p.details.ShortName == "Device Flag") ? p.details.Name : $"{p.details.ShortName} {p.details.Name}";
                    });
                    return fieldMetadata;
                }).ToList();
        }

        private static Dictionary<string, List<Metadata>> BuildAccessors(IEnumerable<string> userDetailsCustomKeys)
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

            // Device Flag
            metadata.Add(nameof(DeviceFlagExportOptions.Id), new List<Metadata>() { new Metadata(nameof(DeviceFlagExportOptions.Id), typeof(string), r => r.Assignment.DeviceFlagId, csvToStringEncoded) });
            metadata.Add(nameof(DeviceFlagExportOptions.Name), new List<Metadata>() { new Metadata(nameof(DeviceFlagExportOptions.Name), typeof(string), r => r.Assignment.DeviceFlag.Name, csvStringEncoded) });
            metadata.Add(nameof(DeviceFlagExportOptions.Description), new List<Metadata>() { new Metadata(nameof(DeviceFlagExportOptions.Description), typeof(string), r => r.Assignment.DeviceFlag.Description, csvStringEncoded) });
            metadata.Add(nameof(DeviceFlagExportOptions.Icon), new List<Metadata>() { new Metadata(nameof(DeviceFlagExportOptions.Icon), typeof(string), r => r.Assignment.DeviceFlag.Icon, csvStringEncoded) });
            metadata.Add(nameof(DeviceFlagExportOptions.IconColour), new List<Metadata>() { new Metadata(nameof(DeviceFlagExportOptions.IconColour), typeof(string), r => r.Assignment.DeviceFlag.IconColour, csvStringEncoded) });
            metadata.Add(nameof(DeviceFlagExportOptions.AssignmentId), new List<Metadata>() { new Metadata(nameof(DeviceFlagExportOptions.AssignmentId), typeof(string), r => r.Assignment.Id, csvToStringEncoded) });
            metadata.Add(nameof(DeviceFlagExportOptions.AddedDate), new List<Metadata>() { new Metadata(nameof(DeviceFlagExportOptions.AddedDate), typeof(string), r => r.Assignment.AddedDate, csvDateTimeEncoded) });
            metadata.Add(nameof(DeviceFlagExportOptions.AddedUserId), new List<Metadata>() { new Metadata(nameof(DeviceFlagExportOptions.AddedUserId), typeof(string), r => r.Assignment.AddedUserId, csvStringEncoded) });
            metadata.Add(nameof(DeviceFlagExportOptions.RemovedUserId), new List<Metadata>() { new Metadata(nameof(DeviceFlagExportOptions.RemovedUserId), typeof(string), r => r.Assignment.RemovedUserId, csvStringEncoded) });
            metadata.Add(nameof(DeviceFlagExportOptions.RemovedDate), new List<Metadata>() { new Metadata(nameof(DeviceFlagExportOptions.RemovedDate), typeof(string), r => r.Assignment.RemovedDate, csvNullableDateTimeEncoded) });
            metadata.Add(nameof(DeviceFlagExportOptions.Comments), new List<Metadata>() { new Metadata(nameof(DeviceFlagExportOptions.Comments), typeof(string), r => r.Assignment.Comments, csvStringEncoded) });


            // Device
            metadata.Add(nameof(DeviceFlagExportOptions.DeviceSerialNumber), new List<Metadata>() { new Metadata(nameof(DeviceFlagExportOptions.DeviceSerialNumber), typeof(string), r => r.Assignment.Device.SerialNumber, csvStringEncoded) });
            metadata.Add(nameof(DeviceFlagExportOptions.DeviceAssetNumber), new List<Metadata>() { new Metadata(nameof(DeviceFlagExportOptions.DeviceAssetNumber), typeof(string), r => r.Assignment.Device.AssetNumber, csvStringEncoded) });
            metadata.Add(nameof(DeviceFlagExportOptions.DeviceLocation), new List<Metadata>() { new Metadata(nameof(DeviceFlagExportOptions.DeviceLocation), typeof(string), r => r.Assignment.Device.Location, csvStringEncoded) });
            metadata.Add(nameof(DeviceFlagExportOptions.DeviceComputerName), new List<Metadata>() { new Metadata(nameof(DeviceFlagExportOptions.DeviceComputerName), typeof(string), r => r.Assignment.Device.DeviceDomainId, csvStringEncoded) });
            metadata.Add(nameof(DeviceFlagExportOptions.DeviceLastNetworkLogon), new List<Metadata>() { new Metadata(nameof(DeviceFlagExportOptions.DeviceLastNetworkLogon), typeof(DateTime), r => r.Assignment.Device.LastNetworkLogonDate, csvNullableDateTimeEncoded) });
            metadata.Add(nameof(DeviceFlagExportOptions.DeviceCreatedDate), new List<Metadata>() { new Metadata(nameof(DeviceFlagExportOptions.DeviceCreatedDate), typeof(DateTime), r => r.Assignment.Device.CreatedDate, csvDateTimeEncoded) });
            metadata.Add(nameof(DeviceFlagExportOptions.DeviceFirstEnrolledDate), new List<Metadata>() { new Metadata(nameof(DeviceFlagExportOptions.DeviceFirstEnrolledDate), typeof(DateTime), r => r.Assignment.Device.EnrolledDate, csvNullableDateTimeEncoded) });
            metadata.Add(nameof(DeviceFlagExportOptions.DeviceLastEnrolledDate), new List<Metadata>() { new Metadata(nameof(DeviceFlagExportOptions.DeviceLastEnrolledDate), typeof(DateTime), r => r.Assignment.Device.LastEnrolDate, csvNullableDateTimeEncoded) });
            metadata.Add(nameof(DeviceFlagExportOptions.DeviceAllowUnauthenticatedEnrol), new List<Metadata>() { new Metadata(nameof(DeviceFlagExportOptions.DeviceAllowUnauthenticatedEnrol), typeof(bool), r => r.Assignment.Device.AllowUnauthenticatedEnrol, csvToStringEncoded) });
            metadata.Add(nameof(DeviceFlagExportOptions.DeviceDecommissionedDate), new List<Metadata>() { new Metadata(nameof(DeviceFlagExportOptions.DeviceDecommissionedDate), typeof(DateTime), r => r.Assignment.Device.DecommissionedDate, csvNullableDateTimeEncoded) });
            metadata.Add(nameof(DeviceFlagExportOptions.DeviceDecommissionedReason), new List<Metadata>() { new Metadata(nameof(DeviceFlagExportOptions.DeviceDecommissionedReason), typeof(string), r => r.Assignment.Device.DecommissionReason, csvToStringEncoded) });

            // Model
            metadata.Add(nameof(DeviceFlagExportOptions.ModelId), new List<Metadata>() { new Metadata(nameof(DeviceFlagExportOptions.ModelId), typeof(int), r => r.Assignment.Device.DeviceModel.Id, csvToStringEncoded) });
            metadata.Add(nameof(DeviceFlagExportOptions.ModelDescription), new List<Metadata>() { new Metadata(nameof(DeviceFlagExportOptions.ModelDescription), typeof(string), r => r.Assignment.Device.DeviceModel.Description, csvStringEncoded) });
            metadata.Add(nameof(DeviceFlagExportOptions.ModelManufacturer), new List<Metadata>() { new Metadata(nameof(DeviceFlagExportOptions.ModelManufacturer), typeof(string), r => r.Assignment.Device.DeviceModel.Manufacturer, csvStringEncoded) });
            metadata.Add(nameof(DeviceFlagExportOptions.ModelModel), new List<Metadata>() { new Metadata(nameof(DeviceFlagExportOptions.ModelModel), typeof(string), r => r.Assignment.Device.DeviceModel.Model, csvStringEncoded) });
            metadata.Add(nameof(DeviceFlagExportOptions.ModelType), new List<Metadata>() { new Metadata(nameof(DeviceFlagExportOptions.ModelType), typeof(string), r => r.Assignment.Device.DeviceModel.ModelType, csvStringEncoded) });

            // Batch
            metadata.Add(nameof(DeviceFlagExportOptions.BatchId), new List<Metadata>() { new Metadata(nameof(DeviceFlagExportOptions.BatchId), typeof(int), r => r.Assignment.Device.DeviceBatch?.Id, csvToStringEncoded) });
            metadata.Add(nameof(DeviceFlagExportOptions.BatchName), new List<Metadata>() { new Metadata(nameof(DeviceFlagExportOptions.BatchName), typeof(string), r => r.Assignment.Device.DeviceBatch?.Name, csvStringEncoded) });
            metadata.Add(nameof(DeviceFlagExportOptions.BatchPurchaseDate), new List<Metadata>() { new Metadata(nameof(DeviceFlagExportOptions.BatchPurchaseDate), typeof(DateTime), r => r.Assignment.Device.DeviceBatch?.PurchaseDate, csvNullableDateEncoded) });
            metadata.Add(nameof(DeviceFlagExportOptions.BatchSupplier), new List<Metadata>() { new Metadata(nameof(DeviceFlagExportOptions.BatchSupplier), typeof(string), r => r.Assignment.Device.DeviceBatch?.Supplier, csvStringEncoded) });
            metadata.Add(nameof(DeviceFlagExportOptions.BatchUnitCost), new List<Metadata>() { new Metadata(nameof(DeviceFlagExportOptions.BatchUnitCost), typeof(decimal), r => r.Assignment.Device.DeviceBatch?.UnitCost, csvCurrencyEncoded) });
            metadata.Add(nameof(DeviceFlagExportOptions.BatchWarrantyValidUntilDate), new List<Metadata>() { new Metadata(nameof(DeviceFlagExportOptions.BatchWarrantyValidUntilDate), typeof(DateTime), r => r.Assignment.Device.DeviceBatch?.WarrantyValidUntil, csvNullableDateEncoded) });
            metadata.Add(nameof(DeviceFlagExportOptions.BatchInsuredDate), new List<Metadata>() { new Metadata(nameof(DeviceFlagExportOptions.BatchInsuredDate), typeof(DateTime), r => r.Assignment.Device.DeviceBatch?.InsuredDate, csvNullableDateEncoded) });
            metadata.Add(nameof(DeviceFlagExportOptions.BatchInsuranceSupplier), new List<Metadata>() { new Metadata(nameof(DeviceFlagExportOptions.BatchInsuranceSupplier), typeof(string), r => r.Assignment.Device.DeviceBatch?.InsuranceSupplier, csvStringEncoded) });
            metadata.Add(nameof(DeviceFlagExportOptions.BatchInsuredUntilDate), new List<Metadata>() { new Metadata(nameof(DeviceFlagExportOptions.BatchInsuredUntilDate), typeof(DateTime), r => r.Assignment.Device.DeviceBatch?.InsuredUntil, csvNullableDateEncoded) });

            // Profile
            metadata.Add(nameof(DeviceFlagExportOptions.ProfileId), new List<Metadata>() { new Metadata(nameof(DeviceFlagExportOptions.ProfileId), typeof(int), r => r.Assignment.Device.DeviceProfile?.Id, csvToStringEncoded) });
            metadata.Add(nameof(DeviceFlagExportOptions.ProfileName), new List<Metadata>() { new Metadata(nameof(DeviceFlagExportOptions.ProfileName), typeof(string), r => r.Assignment.Device.DeviceProfile?.Name, csvStringEncoded) });
            metadata.Add(nameof(DeviceFlagExportOptions.ProfileShortName), new List<Metadata>() { new Metadata(nameof(DeviceFlagExportOptions.ProfileShortName), typeof(string), r => r.Assignment.Device.DeviceProfile?.ShortName, csvStringEncoded) });


            // User
            metadata.Add(nameof(DeviceFlagExportOptions.AssignedUserId), new List<Metadata>() { new Metadata(nameof(DeviceFlagExportOptions.AssignedUserId), typeof(string), r => r.Assignment.Device?.AssignedUser?.UserId, csvStringEncoded) });
            metadata.Add(nameof(DeviceFlagExportOptions.AssignedUserDisplayName), new List<Metadata>() { new Metadata(nameof(DeviceFlagExportOptions.AssignedUserDisplayName), typeof(string), r => r.Assignment.Device?.AssignedUser?.DisplayName, csvStringEncoded) });
            metadata.Add(nameof(DeviceFlagExportOptions.AssignedUserSurname), new List<Metadata>() { new Metadata(nameof(DeviceFlagExportOptions.AssignedUserSurname), typeof(string), r => r.Assignment.Device?.AssignedUser?.Surname, csvStringEncoded) });
            metadata.Add(nameof(DeviceFlagExportOptions.AssignedUserGivenName), new List<Metadata>() { new Metadata(nameof(DeviceFlagExportOptions.AssignedUserGivenName), typeof(string), r => r.Assignment.Device?.AssignedUser?.GivenName, csvStringEncoded) });
            metadata.Add(nameof(DeviceFlagExportOptions.AssignedUserPhoneNumber), new List<Metadata>() { new Metadata(nameof(DeviceFlagExportOptions.AssignedUserPhoneNumber), typeof(string), r => r.Assignment.Device?.AssignedUser?.PhoneNumber, csvStringEncoded) });
            metadata.Add(nameof(DeviceFlagExportOptions.AssignedUserEmailAddress), new List<Metadata>() { new Metadata(nameof(DeviceFlagExportOptions.AssignedUserEmailAddress), typeof(string), r => r.Assignment.Device?.AssignedUser?.EmailAddress, csvStringEncoded) });
            if (userDetailsCustomKeys != null)
            {
                var userDetailCustomFields = new List<Metadata>();
                foreach (var detailKey in userDetailsCustomKeys.OrderBy(k => k, StringComparer.OrdinalIgnoreCase))
                {
                    var key = detailKey;
                    userDetailCustomFields.Add(new Metadata(detailKey, detailKey, typeof(string), r => r.AssignedUserCustomDetails != null && r.AssignedUserCustomDetails.TryGetValue(key, out var value) ? value : null, csvStringEncoded));
                }
                metadata.Add(nameof(DeviceFlagExportOptions.AssignedUserDetailCustom), userDetailCustomFields);
            }

            return metadata;
        }

    }
}
