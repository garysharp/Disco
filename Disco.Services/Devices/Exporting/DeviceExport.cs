﻿using Disco.Data.Repository;
using Disco.Models.Exporting;
using Disco.Models.Repository;
using Disco.Models.Services.Devices.Exporting;
using Disco.Models.Services.Exporting;
using Disco.Services.Plugins.Features.DetailsProvider;
using Disco.Services.Tasks;
using Disco.Services.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Entity;
using System.Linq;

namespace Disco.Services.Devices.Exporting
{
    using Metadata = ExportFieldMetadata<DeviceExportRecord>;

    public static class DeviceExport
    {

        public static ExportResult GenerateExport(DiscoDataContext Database, Func<IQueryable<Device>, IQueryable<Device>> Filter, DeviceExportOptions Options, IScheduledTaskStatus TaskStatus)
        {
            var deviceQuery = Database.Devices
                .Include(d => d.AssignedUser.UserDetails)
                .Include(d => d.DeviceDetails);
            if (Filter != null)
                deviceQuery = Filter(deviceQuery);

            // Update Users
            if (Options.AssignedUserDisplayName ||
                Options.AssignedUserSurname ||
                Options.AssignedUserGivenName ||
                Options.AssignedUserPhoneNumber ||
                Options.AssignedUserEmailAddress)
            {
                TaskStatus.UpdateStatus(5, "Refreshing user details from Active Directory");
                var userIds = deviceQuery.Where(d => d.AssignedUserId != null).Select(d => d.AssignedUserId).Distinct().ToList();
                foreach (var userId in userIds)
                {
                    try
                    {
                        UserService.GetUser(userId, Database);
                    }
                    catch (Exception) { } // Ignore Errors
                }
            }

            // Update Last Network Logon Date
            if (Options.DeviceLastNetworkLogon)
            {
                TaskStatus.UpdateStatus(15, "Refreshing device last network logon dates from Active Directory");
                try
                {
                    Interop.ActiveDirectory.ADNetworkLogonDatesUpdateTask.UpdateLastNetworkLogonDates(Database, ScheduledTaskMockStatus.Create("UpdateLastNetworkLogonDates"));
                    Database.SaveChanges();
                }
                catch (Exception) { } // Ignore Errors
            }

            TaskStatus.UpdateStatus(25, "Extracting records from the database");

            var records = BuildRecords(deviceQuery).ToList();

            // materialize device details
            records.ForEach(r =>
            {
                if (Options.DetailBios)
                    r.DeviceDetailBios = r.DeviceDetails.Bios();
                if (Options.DetailBaseBoard)
                    r.DeviceDetailBaseBoard = r.DeviceDetails.BaseBoard();
                if (Options.DetailComputerSystem)
                    r.DeviceDetailComputerSystem = r.DeviceDetails.ComputerSystem();
                if (Options.DetailProcessors)
                    r.DeviceDetailProcessors = r.DeviceDetails.Processors();
                if (Options.DetailMemory)
                    r.DeviceDetailPhysicalMemory = r.DeviceDetails.PhysicalMemory();
                if (Options.DetailDiskDrives)
                    r.DeviceDetailDiskDrives = r.DeviceDetails.DiskDrives();
                if (Options.DetailLanAdapters || Options.DetailWLanAdapters)
                {
                    r.DeviceDetailNetworkAdapters = r.DeviceDetails.NetworkAdapters();
                    if (r.DeviceDetailNetworkAdapters == null)
                    {
                        r.DeviceDetailLanMacAddresses = r.DeviceDetails.LanMacAddress()?.Split(';').Select(a => a.Trim()).ToList();
                        r.DeviceDetailWlanMacAddresses = r.DeviceDetails.WLanMacAddress()?.Split(';').Select(a => a.Trim()).ToList();
                    }
                }
                if (Options.DetailBatteries)
                    r.DeviceDetailBatteries = r.DeviceDetails.Batteries();

                if (Options.AssignedUserDetailCustom && r.AssignedUser != null)
                {
                    var detailsService = new DetailsProviderService(Database);
                    r.AssignedUserCustomDetails = detailsService.GetDetails(r.AssignedUser);
                }
            });

            TaskStatus.UpdateStatus(70, "Building metadata");
            var metadata = Options.BuildMetadata(records);

            if (metadata.Count == 0)
                throw new ArgumentException("At least one export field must be specified", "Options");

            TaskStatus.UpdateStatus(80, $"Formatting {records.Count} records for export");

            return ExportHelpers.WriteExport(Options, TaskStatus, metadata, records);
        }

        public static ExportResult GenerateExport(DiscoDataContext Database, DeviceExportOptions Options, IScheduledTaskStatus TaskStatus)
        {
            switch (Options.ExportType)
            {
                case DeviceExportTypes.All:
                    return GenerateExport(Database, null, Options, TaskStatus);
                case DeviceExportTypes.Batch:
                    if (Options.ExportTypeTargetId.HasValue && Options.ExportTypeTargetId.Value > 0)
                        return GenerateExport(Database, devices => devices.Where(d => d.DeviceBatchId == Options.ExportTypeTargetId), Options, TaskStatus);
                    else
                        return GenerateExport(Database, devices => devices.Where(d => d.DeviceBatchId == null), Options, TaskStatus);
                case DeviceExportTypes.Model:
                    return GenerateExport(Database, devices => devices.Where(d => d.DeviceModelId == Options.ExportTypeTargetId), Options, TaskStatus);
                case DeviceExportTypes.Profile:
                    return GenerateExport(Database, devices => devices.Where(d => d.DeviceProfileId == Options.ExportTypeTargetId), Options, TaskStatus);
                default:
                    throw new ArgumentException(string.Format("Unknown Device Export Type", Options.ExportType.ToString()), "Options");
            }
        }
        public static ExportResult GenerateExport(DiscoDataContext Database, DeviceExportOptions Options)
        {
            return GenerateExport(Database, Options, ScheduledTaskMockStatus.Create("Device Export"));
        }

        private static IEnumerable<DeviceExportRecord> BuildRecords(IQueryable<Device> Devices)
        {
            return Devices.Select(d => new DeviceExportRecord()
            {
                Device = d,

                DeviceDetails = d.DeviceDetails,

                ModelId = d.DeviceModelId,
                ModelDescription = d.DeviceModel.Description,
                ModelManufacturer = d.DeviceModel.Manufacturer,
                ModelModel = d.DeviceModel.Model,
                ModelType = d.DeviceModel.ModelType,

                BatchId = d.DeviceBatchId,
                BatchName = d.DeviceBatch.Name,
                BatchPurchaseDate = d.DeviceBatch.PurchaseDate,
                BatchSupplier = d.DeviceBatch.Supplier,
                BatchUnitCost = d.DeviceBatch.UnitCost,
                BatchWarrantyValidUntilDate = d.DeviceBatch.WarrantyValidUntil,
                BatchInsuredDate = d.DeviceBatch.InsuredDate,
                BatchInsuranceSupplier = d.DeviceBatch.InsuranceSupplier,
                BatchInsuredUntilDate = d.DeviceBatch.InsuredUntil,

                ProfileId = d.DeviceProfileId,
                ProfileName = d.DeviceProfile.Name,
                ProfileShortName = d.DeviceProfile.ShortName,

                DeviceUserAssignment = d.DeviceUserAssignments.Where(dua => dua.UnassignedDate == null).FirstOrDefault(),
                AssignedUser = d.AssignedUser,
                AssignedUserDetails = d.AssignedUser.UserDetails,

                JobsTotalCount = d.Jobs.Count(),
                JobsOpenCount = d.Jobs.Count(j => j.ClosedDate == null),

                AttachmentsCount = d.DeviceAttachments.Count(),

                DeviceCertificates = d.DeviceCertificates.Where(dc => dc.Enabled).OrderByDescending(dc => dc.AllocatedDate)
            });
        }

        private static List<Metadata> BuildMetadata(this DeviceExportOptions options, List<DeviceExportRecord> records)
        {
            var processorMaxCount = Math.Max(1, records.Max(r => r.DeviceDetailProcessors?.Count ?? 0));
            var memoryMaxCount = Math.Max(1, records.Max(r => r.DeviceDetailPhysicalMemory?.Count ?? 0));
            var diskDriveMaxCount = Math.Max(1, records.Max(r => r.DeviceDetailDiskDrives?.Count ?? 0));
            var lanAdapterMaxCount = Math.Max(1, records.Max(r => r.DeviceDetailNetworkAdapters?.Count(a => !a.IsWlanAdapter) ?? r.DeviceDetailLanMacAddresses?.Count ?? 0));
            var wlanAdapterMaxCount = Math.Max(1, records.Max(r => r.DeviceDetailNetworkAdapters?.Count(a => a.IsWlanAdapter) ?? r.DeviceDetailWlanMacAddresses?.Count ?? 0));
            var certificateMaxCount = Math.Max(1, records.Max(r => r.DeviceCertificates?.Count() ?? 0));
            var batteriesMaxCount = Math.Max(1, records.Max(r => r.DeviceDetailBatteries?.Count ?? 0));

            IEnumerable<string> assignedUserDetailCustomKeys = null;
            if (options.AssignedUserDetailCustom)
                assignedUserDetailCustomKeys = records.Where(r => r.AssignedUserCustomDetails != null).SelectMany(r => r.AssignedUserCustomDetails.Keys).Distinct(StringComparer.OrdinalIgnoreCase).ToList();

            var allAccessors = BuildRecordAccessors(processorMaxCount, memoryMaxCount, diskDriveMaxCount, lanAdapterMaxCount, wlanAdapterMaxCount, certificateMaxCount, batteriesMaxCount, assignedUserDetailCustomKeys);

            return typeof(DeviceExportOptions).GetProperties()
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
                            f.ColumnName = (p.details.ShortName == "Device" || p.details.ShortName == "Details") ? p.details.Name : $"{p.details.ShortName} {p.details.Name}";
                    });
                    return fieldMetadata;
                }).ToList();
        }

        private static Dictionary<string, List<Metadata>> BuildRecordAccessors(int processorMaxCount, int memoryMaxCount, int diskDriveMaxCount, int lanAdapterMaxCount, int wlanAdapterMaxCount, int certificateMaxCount, int batteriesMaxCount, IEnumerable<string> assignedUserDetailCustomKeys)
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

            // Device
            metadata.Add(nameof(DeviceExportOptions.DeviceSerialNumber), new List<Metadata>() { new Metadata(nameof(DeviceExportOptions.DeviceSerialNumber), typeof(string), r => r.Device.SerialNumber, csvStringEncoded) });
            metadata.Add(nameof(DeviceExportOptions.DeviceAssetNumber), new List<Metadata>() { new Metadata(nameof(DeviceExportOptions.DeviceAssetNumber), typeof(string), r => r.Device.AssetNumber, csvStringEncoded) });
            metadata.Add(nameof(DeviceExportOptions.DeviceLocation), new List<Metadata>() { new Metadata(nameof(DeviceExportOptions.DeviceLocation), typeof(string), r => r.Device.Location, csvStringEncoded) });
            metadata.Add(nameof(DeviceExportOptions.DeviceComputerName), new List<Metadata>() { new Metadata(nameof(DeviceExportOptions.DeviceComputerName), typeof(string), r => r.Device.DeviceDomainId, csvStringEncoded) });
            metadata.Add(nameof(DeviceExportOptions.DeviceLastNetworkLogon), new List<Metadata>() { new Metadata(nameof(DeviceExportOptions.DeviceLastNetworkLogon), typeof(DateTime), r => r.Device.LastNetworkLogonDate, csvNullableDateTimeEncoded) });
            metadata.Add(nameof(DeviceExportOptions.DeviceCreatedDate), new List<Metadata>() { new Metadata(nameof(DeviceExportOptions.DeviceCreatedDate), typeof(DateTime), r => r.Device.CreatedDate, csvDateTimeEncoded) });
            metadata.Add(nameof(DeviceExportOptions.DeviceFirstEnrolledDate), new List<Metadata>() { new Metadata(nameof(DeviceExportOptions.DeviceFirstEnrolledDate), typeof(DateTime), r => r.Device.EnrolledDate, csvNullableDateTimeEncoded) });
            metadata.Add(nameof(DeviceExportOptions.DeviceLastEnrolledDate), new List<Metadata>() { new Metadata(nameof(DeviceExportOptions.DeviceLastEnrolledDate), typeof(DateTime), r => r.Device.LastEnrolDate, csvNullableDateTimeEncoded) });
            metadata.Add(nameof(DeviceExportOptions.DeviceAllowUnauthenticatedEnrol), new List<Metadata>() { new Metadata(nameof(DeviceExportOptions.DeviceAllowUnauthenticatedEnrol), typeof(bool), r => r.Device.AllowUnauthenticatedEnrol, csvToStringEncoded) });
            metadata.Add(nameof(DeviceExportOptions.DeviceDecommissionedDate), new List<Metadata>() { new Metadata(nameof(DeviceExportOptions.DeviceDecommissionedDate), typeof(DateTime), r => r.Device.DecommissionedDate, csvNullableDateTimeEncoded) });
            metadata.Add(nameof(DeviceExportOptions.DeviceDecommissionedReason), new List<Metadata>() { new Metadata(nameof(DeviceExportOptions.DeviceDecommissionedReason), typeof(string), r => r.Device.DecommissionReason, csvToStringEncoded) });

            // Model
            metadata.Add(nameof(DeviceExportOptions.ModelId), new List<Metadata>() { new Metadata(nameof(DeviceExportOptions.ModelId), typeof(int), r => r.ModelId, csvToStringEncoded) });
            metadata.Add(nameof(DeviceExportOptions.ModelDescription), new List<Metadata>() { new Metadata(nameof(DeviceExportOptions.ModelDescription), typeof(string), r => r.ModelDescription, csvStringEncoded) });
            metadata.Add(nameof(DeviceExportOptions.ModelManufacturer), new List<Metadata>() { new Metadata(nameof(DeviceExportOptions.ModelManufacturer), typeof(string), r => r.ModelManufacturer, csvStringEncoded) });
            metadata.Add(nameof(DeviceExportOptions.ModelModel), new List<Metadata>() { new Metadata(nameof(DeviceExportOptions.ModelModel), typeof(string), r => r.ModelModel, csvStringEncoded) });
            metadata.Add(nameof(DeviceExportOptions.ModelType), new List<Metadata>() { new Metadata(nameof(DeviceExportOptions.ModelType), typeof(string), r => r.ModelType, csvStringEncoded) });

            // Batch
            metadata.Add(nameof(DeviceExportOptions.BatchId), new List<Metadata>() { new Metadata(nameof(DeviceExportOptions.BatchId), typeof(int), r => r.BatchId, csvToStringEncoded) });
            metadata.Add(nameof(DeviceExportOptions.BatchName), new List<Metadata>() { new Metadata(nameof(DeviceExportOptions.BatchName), typeof(string), r => r.BatchName, csvStringEncoded) });
            metadata.Add(nameof(DeviceExportOptions.BatchPurchaseDate), new List<Metadata>() { new Metadata(nameof(DeviceExportOptions.BatchPurchaseDate), typeof(DateTime), r => r.BatchPurchaseDate, csvNullableDateEncoded) });
            metadata.Add(nameof(DeviceExportOptions.BatchSupplier), new List<Metadata>() { new Metadata(nameof(DeviceExportOptions.BatchSupplier), typeof(string), r => r.BatchSupplier, csvStringEncoded) });
            metadata.Add(nameof(DeviceExportOptions.BatchUnitCost), new List<Metadata>() { new Metadata(nameof(DeviceExportOptions.BatchUnitCost), typeof(decimal), r => r.BatchUnitCost, csvCurrencyEncoded) });
            metadata.Add(nameof(DeviceExportOptions.BatchWarrantyValidUntilDate), new List<Metadata>() { new Metadata(nameof(DeviceExportOptions.BatchWarrantyValidUntilDate), typeof(DateTime), r => r.BatchWarrantyValidUntilDate, csvNullableDateEncoded) });
            metadata.Add(nameof(DeviceExportOptions.BatchInsuredDate), new List<Metadata>() { new Metadata(nameof(DeviceExportOptions.BatchInsuredDate), typeof(DateTime), r => r.BatchInsuredDate, csvNullableDateEncoded) });
            metadata.Add(nameof(DeviceExportOptions.BatchInsuranceSupplier), new List<Metadata>() { new Metadata(nameof(DeviceExportOptions.BatchInsuranceSupplier), typeof(string), r => r.BatchInsuranceSupplier, csvStringEncoded) });
            metadata.Add(nameof(DeviceExportOptions.BatchInsuredUntilDate), new List<Metadata>() { new Metadata(nameof(DeviceExportOptions.BatchInsuredUntilDate), typeof(DateTime), r => r.BatchInsuredUntilDate, csvNullableDateEncoded) });

            // Profile
            metadata.Add(nameof(DeviceExportOptions.ProfileId), new List<Metadata>() { new Metadata(nameof(DeviceExportOptions.ProfileId), typeof(int), r => r.ProfileId, csvToStringEncoded) });
            metadata.Add(nameof(DeviceExportOptions.ProfileName), new List<Metadata>() { new Metadata(nameof(DeviceExportOptions.ProfileName), typeof(string), r => r.ProfileName, csvStringEncoded) });
            metadata.Add(nameof(DeviceExportOptions.ProfileShortName), new List<Metadata>() { new Metadata(nameof(DeviceExportOptions.ProfileShortName), typeof(string), r => r.ProfileShortName, csvStringEncoded) });

            // User
            metadata.Add(nameof(DeviceExportOptions.AssignedUserId), new List<Metadata>() { new Metadata(nameof(DeviceExportOptions.AssignedUserId), typeof(string), r => r.AssignedUser?.UserId, csvStringEncoded) });
            metadata.Add(nameof(DeviceExportOptions.AssignedUserDate), new List<Metadata>() { new Metadata(nameof(DeviceExportOptions.AssignedUserDate), typeof(DateTime), r => r.DeviceUserAssignment?.AssignedDate, csvNullableDateTimeEncoded) });
            metadata.Add(nameof(DeviceExportOptions.AssignedUserDisplayName), new List<Metadata>() { new Metadata(nameof(DeviceExportOptions.AssignedUserDisplayName), typeof(string), r => r.AssignedUser?.DisplayName, csvStringEncoded) });
            metadata.Add(nameof(DeviceExportOptions.AssignedUserSurname), new List<Metadata>() { new Metadata(nameof(DeviceExportOptions.AssignedUserSurname), typeof(string), r => r.AssignedUser?.Surname, csvStringEncoded) });
            metadata.Add(nameof(DeviceExportOptions.AssignedUserGivenName), new List<Metadata>() { new Metadata(nameof(DeviceExportOptions.AssignedUserGivenName), typeof(string), r => r.AssignedUser?.GivenName, csvStringEncoded) });
            metadata.Add(nameof(DeviceExportOptions.AssignedUserPhoneNumber), new List<Metadata>() { new Metadata(nameof(DeviceExportOptions.AssignedUserPhoneNumber), typeof(string), r => r.AssignedUser?.PhoneNumber, csvStringEncoded) });
            metadata.Add(nameof(DeviceExportOptions.AssignedUserEmailAddress), new List<Metadata>() { new Metadata(nameof(DeviceExportOptions.AssignedUserEmailAddress), typeof(string), r => r.AssignedUser?.EmailAddress, csvStringEncoded) });
            if (assignedUserDetailCustomKeys != null)
            {
                var assignedUserDetailCustomFields = new List<Metadata>();
                foreach (var detailKey in assignedUserDetailCustomKeys.OrderBy(k => k, StringComparer.OrdinalIgnoreCase))
                {
                    var key = detailKey;
                    assignedUserDetailCustomFields.Add(new Metadata(detailKey, detailKey, typeof(string), r => r.AssignedUserCustomDetails != null && r.AssignedUserCustomDetails.TryGetValue(key, out var value) ? value : null, csvStringEncoded));
                }
                metadata.Add(nameof(DeviceExportOptions.AssignedUserDetailCustom), assignedUserDetailCustomFields);
            }

            // Jobs
            metadata.Add(nameof(DeviceExportOptions.JobsTotalCount), new List<Metadata>() { new Metadata(nameof(DeviceExportOptions.JobsTotalCount), typeof(int), r => r.JobsTotalCount, csvToStringEncoded) });
            metadata.Add(nameof(DeviceExportOptions.JobsOpenCount), new List<Metadata>() { new Metadata(nameof(DeviceExportOptions.JobsOpenCount), typeof(int), r => r.JobsOpenCount, csvToStringEncoded) });

            // Attachments
            metadata.Add(nameof(DeviceExportOptions.AttachmentsCount), new List<Metadata>() { new Metadata(nameof(DeviceExportOptions.AttachmentsCount), typeof(int), r => r.AttachmentsCount, csvToStringEncoded) });

            // Certificates
            var certificateFields = new List<Metadata>(certificateMaxCount * 4);
            for (int i = 0; i < certificateMaxCount; i++)
            {
                var v = i;
                var index = i + 1;
                certificateFields.Add(new Metadata($"Certificate{index}Name", $"Certificate {index} Name", typeof(string), r => r.DeviceCertificates.Skip(v).FirstOrDefault()?.Name, csvStringEncoded));
                certificateFields.Add(new Metadata($"Certificate{index}AllocationDate", $"Certificate {index} Allocation Date", typeof(DateTime), r => r.DeviceCertificates.Skip(v).FirstOrDefault()?.AllocatedDate, csvNullableDateTimeEncoded));
                certificateFields.Add(new Metadata($"Certificate{index}ExpirationDate", $"Certificate {index} Expiration Date", typeof(DateTime), r => r.DeviceCertificates.Skip(v).FirstOrDefault()?.ExpirationDate, csvNullableDateTimeEncoded));
                certificateFields.Add(new Metadata($"Certificate{index}ProviderId", $"Certificate {index} Provider Id", typeof(string), r => r.DeviceCertificates.Skip(v).FirstOrDefault()?.ProviderId, csvStringEncoded));
            }
            metadata.Add(nameof(DeviceExportOptions.Certificates), certificateFields);

            // Details
            var biosFields = new List<Metadata>()
            {
                new Metadata("BIOSManufacturer", "BIOS Manufacturer", typeof(string), r => r.DeviceDetailBios?.FirstOrDefault()?.Manufacturer, csvStringEncoded),
                new Metadata("BIOSSerialNumber", "BIOS Serial Number", typeof(string), r => r.DeviceDetailBios?.FirstOrDefault()?.SerialNumber, csvStringEncoded),
                new Metadata("BIOSVersion", "BIOS Version", typeof(string), r => {
                    var bios = r.DeviceDetailBios?.FirstOrDefault();
                    if (bios?.SMBIOSBIOSVersion != null)
                        return $"{bios.SMBIOSBIOSVersion} {bios.SMBIOSMajorVersion}.{bios.SMBIOSMinorVersion}";
                    else
                        return null;
                    }, csvStringEncoded),
                new Metadata("BIOSSystemVersion", "BIOS System Version", typeof(string), r => {
                    var bios = r.DeviceDetailBios?.FirstOrDefault();
                    if (bios?.SystemBiosMajorVersion.HasValue ?? false)
                        return $"{bios.SystemBiosMajorVersion}.{bios.SystemBiosMinorVersion}";
                    else
                        return null;
                    }, csvStringEncoded),
                new Metadata("BIOSReleaseDate", "BIOS Release Date", typeof(DateTime), r => r.DeviceDetailBios?.FirstOrDefault()?.ReleaseDate, csvNullableDateTimeEncoded),
            };
            metadata.Add(nameof(DeviceExportOptions.DetailBios), biosFields);

            var baseBoardFields = new List<Metadata>()
            {
                new Metadata("BaseBoardManufacturer", "Base Board Manufacturer", typeof(string), r => r.DeviceDetailBaseBoard?.FirstOrDefault()?.Manufacturer, csvStringEncoded),
                new Metadata("BaseBoardModel", "Base Board Model", typeof(string), r => r.DeviceDetailBaseBoard?.FirstOrDefault()?.Model, csvStringEncoded),
                new Metadata("BaseBoardProduct", "Base Board Product", typeof(string), r => r.DeviceDetailBaseBoard?.FirstOrDefault()?.Product, csvStringEncoded),
                new Metadata("BaseBoardPartNumber", "Base Board Part Number", typeof(string), r => r.DeviceDetailBaseBoard?.FirstOrDefault()?.PartNumber, csvStringEncoded),
                new Metadata("BaseBoardSKU", "Base Board SKU", typeof(string), r => r.DeviceDetailBaseBoard?.FirstOrDefault()?.SKU, csvStringEncoded),
                new Metadata("BaseBoardSerialNumber", "Base Board Serial Number", typeof(string), r => r.DeviceDetailBaseBoard?.FirstOrDefault()?.SerialNumber, csvStringEncoded),
                new Metadata("BaseBoardConfigOptions", "Base Board Config Options", typeof(string), r => {
                    var baseBoard = r.DeviceDetailBaseBoard?.FirstOrDefault();
                    if (baseBoard?.ConfigOptions != null)
                        return string.Join("; ", baseBoard.ConfigOptions);
                    else
                        return null;
                    }, csvStringEncoded),
                new Metadata("BaseBoardVersion", "Base Board Version", typeof(string), r => r.DeviceDetailBaseBoard?.FirstOrDefault()?.Version, csvStringEncoded),
            };
            metadata.Add(nameof(DeviceExportOptions.DetailBaseBoard), baseBoardFields);

            var computerSystemFields = new List<Metadata>()
            {
                new Metadata("ComputerSystemDescription", "System Description", typeof(string), r => r.DeviceDetailComputerSystem?.FirstOrDefault()?.Description, csvStringEncoded),
                new Metadata("ComputerSystemPCSystemType", "System Form Factor", typeof(string), r => r.DeviceDetailComputerSystem?.FirstOrDefault()?.PCSystemType, csvStringEncoded),
                new Metadata("ComputerSystemSystemType", "System Type", typeof(string), r => r.DeviceDetailComputerSystem?.FirstOrDefault()?.SystemType, csvStringEncoded),
                new Metadata("ComputerSystemPrimaryOwnerName", "System Primary Owner Name", typeof(string), r => r.DeviceDetailComputerSystem?.FirstOrDefault()?.PrimaryOwnerName, csvStringEncoded),
                new Metadata("ComputerSystemPrimaryOwnerContact", "System Primary Owner Contact", typeof(string), r => r.DeviceDetailComputerSystem?.FirstOrDefault()?.PrimaryOwnerContact, csvStringEncoded),
                new Metadata("ComputerSystemChassisSKU", "System Chassis SKU", typeof(string), r => r.DeviceDetailComputerSystem?.FirstOrDefault()?.ChassisSKUNumber, csvStringEncoded),
                new Metadata("ComputerSystemSystemSKU", "System SKU", typeof(string), r => r.DeviceDetailComputerSystem?.FirstOrDefault()?.SystemSKUNumber, csvStringEncoded),
                new Metadata("ComputerSystemOEMReference", "System OEM Reference", typeof(string), r => {
                    var computerSystem = r.DeviceDetailComputerSystem?.FirstOrDefault();
                    if (computerSystem?.OEMStringArray != null)
                        return string.Join("; ", computerSystem.OEMStringArray);
                    else
                        return null;
                    }, csvStringEncoded),
                new Metadata("ComputerSystemCurrentTimeZone", "System Time Zone", typeof(string), r => {
                    var computerSystem = r.DeviceDetailComputerSystem?.FirstOrDefault();
                    if (computerSystem?.CurrentTimeZone.HasValue ?? false)
                        return $"{computerSystem.CurrentTimeZone.Value / 60:00}:{Math.Abs(computerSystem.CurrentTimeZone.Value % 60):00}";
                    else
                        return null;
                    }, csvStringEncoded),
                new Metadata("ComputerSystemRoles", "System Roles", typeof(string), r => {
                    var computerSystem = r.DeviceDetailComputerSystem?.FirstOrDefault();
                    if (computerSystem?.Roles != null)
                        return string.Join("; ", computerSystem.Roles);
                    else
                        return null;
                    }, csvStringEncoded),
            };
            metadata.Add(nameof(DeviceExportOptions.DetailComputerSystem), computerSystemFields);

            var processorFields = new List<Metadata>(processorMaxCount * 6);
            for (int i = 0; i < processorMaxCount; i++)
            {
                var v = i;
                var index = i + 1;
                processorFields.Add(new Metadata($"Processor{index}Name", $"Processor {index} Name", typeof(string), r => r.DeviceDetailProcessors?.Skip(v).FirstOrDefault()?.Name, csvStringEncoded));
                processorFields.Add(new Metadata($"Processor{index}Description", $"Processor {index} Description", typeof(string), r => r.DeviceDetailProcessors?.Skip(v).FirstOrDefault()?.Description, csvStringEncoded));
                processorFields.Add(new Metadata($"Processor{index}Architecture", $"Processor {index} Architecture", typeof(string), r => r.DeviceDetailProcessors?.Skip(v).FirstOrDefault()?.Architecture, csvStringEncoded));
                processorFields.Add(new Metadata($"Processor{index}ClockSpeed", $"Processor {index} Clock Speed", typeof(string), r => r.DeviceDetailProcessors?.Skip(v).FirstOrDefault()?.MaxClockSpeedFriendly(), csvStringEncoded));
                processorFields.Add(new Metadata($"Processor{index}Cores", $"Processor {index} Cores", typeof(int), r => r.DeviceDetailProcessors?.Skip(v).FirstOrDefault()?.NumberOfCores, csvToStringEncoded));
                processorFields.Add(new Metadata($"Processor{index}LogicalProcessors", $"Processor {index} Logical Processors", typeof(int), r => r.DeviceDetailProcessors?.Skip(v).FirstOrDefault()?.NumberOfLogicalProcessors, csvToStringEncoded));
            }
            metadata.Add(nameof(DeviceExportOptions.DetailProcessors), processorFields);
            var memoryFields = new List<Metadata>((memoryMaxCount * 6) + 1);
            memoryFields.Add(new Metadata($"MemoryTotalCapacity", $"Memory Total Capacity", typeof(string), r => MeasurementUnitExtensions.ByteSizeToFriendly((ulong)(r.DeviceDetailPhysicalMemory?.Sum(m => (long)m.Capacity) ?? 0L)), csvStringEncoded));
            for (int i = 0; i < memoryMaxCount; i++)
            {
                var v = i;
                var index = i + 1;
                memoryFields.Add(new Metadata($"Memory{index}Location", $"Memory {index} Location", typeof(string), r => r.DeviceDetailPhysicalMemory?.Skip(v).FirstOrDefault()?.DeviceLocator, csvStringEncoded));
                memoryFields.Add(new Metadata($"Memory{index}Manufacturer", $"Memory {index} Manufacturer", typeof(string), r => r.DeviceDetailPhysicalMemory?.Skip(v).FirstOrDefault()?.Manufacturer, csvStringEncoded));
                memoryFields.Add(new Metadata($"Memory{index}PartNumber", $"Memory {index} Part Number", typeof(string), r => r.DeviceDetailPhysicalMemory?.Skip(v).FirstOrDefault()?.PartNumber, csvStringEncoded));
                memoryFields.Add(new Metadata($"Memory{index}SerialNumber", $"Memory {index} Serial Number", typeof(string), r => r.DeviceDetailPhysicalMemory?.Skip(v).FirstOrDefault()?.SerialNumber, csvStringEncoded));
                memoryFields.Add(new Metadata($"Memory{index}Capacity", $"Memory {index} Capacity", typeof(string), r => r.DeviceDetailPhysicalMemory?.Skip(v).FirstOrDefault()?.CapacityFriendly(), csvStringEncoded));
                memoryFields.Add(new Metadata($"Memory{index}ConfiguredClockSpeed", $"Memory {index} Clock Speed", typeof(int), r => r.DeviceDetailPhysicalMemory?.Skip(v).FirstOrDefault()?.ConfiguredClockSpeed, csvToStringEncoded));
            }
            metadata.Add(nameof(DeviceExportOptions.DetailMemory), memoryFields);
            var diskFields = new List<Metadata>(diskDriveMaxCount * 6);
            for (int i = 0; i < diskDriveMaxCount; i++)
            {
                var v = i;
                var index = i + 1;
                diskFields.Add(new Metadata($"Disk{index}Manufacturer", $"Disk {index} Manufacturer", typeof(string), r => r.DeviceDetailDiskDrives?.Skip(v).FirstOrDefault()?.Manufacturer, csvStringEncoded));
                diskFields.Add(new Metadata($"Disk{index}Model", $"Disk {index} Model", typeof(string), r => r.DeviceDetailDiskDrives?.Skip(v).FirstOrDefault()?.Model, csvStringEncoded));
                diskFields.Add(new Metadata($"Disk{index}SerialNumber", $"Disk {index} Serial Number", typeof(string), r => r.DeviceDetailDiskDrives?.Skip(v).FirstOrDefault()?.SerialNumber, csvStringEncoded));
                diskFields.Add(new Metadata($"Disk{index}Firmware", $"Disk {index} Firmware", typeof(string), r => r.DeviceDetailDiskDrives?.Skip(v).FirstOrDefault()?.FirmwareRevision, csvStringEncoded));
                diskFields.Add(new Metadata($"Disk{index}Capacity", $"Disk {index} Size", typeof(string), r => r.DeviceDetailDiskDrives?.Skip(v).FirstOrDefault()?.SizeFriendly(), csvStringEncoded));
                diskFields.Add(new Metadata($"Disk{index}Capacity", $"Disk {index} Total Free Space", typeof(string), r => MeasurementUnitExtensions.ByteSizeToFriendly((ulong)(r.DeviceDetailDiskDrives?.Skip(v).FirstOrDefault()?.Partitions?.Sum(p => (long)(p.LogicalDisk?.FreeSpace ?? 0L)) ?? 0L)), csvStringEncoded));
            }
            metadata.Add(nameof(DeviceExportOptions.DetailDiskDrives), diskFields);
            var lanAdapterFields = new List<Metadata>(lanAdapterMaxCount * 5);
            for (int i = 0; i < lanAdapterMaxCount; i++)
            {
                var v = i;
                var index = i + 1;
                lanAdapterFields.Add(new Metadata($"LanAdapter{index}Connection", $"Lan Adapter {index} Connection", typeof(string), r => r.DeviceDetailNetworkAdapters?.Where(a => !a.IsWlanAdapter).Skip(v).FirstOrDefault()?.NetConnectionID, csvStringEncoded));
                lanAdapterFields.Add(new Metadata($"LanAdapter{index}Manufacturer", $"Lan Adapter {index} Manufacturer", typeof(string), r => r.DeviceDetailNetworkAdapters?.Where(a => !a.IsWlanAdapter).Skip(v).FirstOrDefault()?.Manufacturer, csvStringEncoded));
                lanAdapterFields.Add(new Metadata($"LanAdapter{index}ProductName", $"Lan Adapter {index} Product Name", typeof(string), r => r.DeviceDetailNetworkAdapters?.Where(a => !a.IsWlanAdapter).Skip(v).FirstOrDefault()?.ProductName, csvStringEncoded));
                lanAdapterFields.Add(new Metadata($"LanAdapter{index}Speed", $"Lan Adapter {index} Speed", typeof(string), r => r.DeviceDetailNetworkAdapters?.Where(a => !a.IsWlanAdapter).Skip(v).FirstOrDefault()?.SpeedFriendly(), csvStringEncoded));
                lanAdapterFields.Add(new Metadata($"LanAdapter{index}MacAddress", $"Lan Adapter {index} Mac Address", typeof(string), r => r.DeviceDetailNetworkAdapters?.Where(a => !a.IsWlanAdapter).Skip(v).FirstOrDefault()?.MACAddress ?? r.DeviceDetailLanMacAddresses?.Skip(v).FirstOrDefault(), csvStringEncoded));
            }
            metadata.Add(nameof(DeviceExportOptions.DetailLanAdapters), lanAdapterFields);
            var fields = new List<Metadata>(wlanAdapterMaxCount * 5);
            for (int i = 0; i < wlanAdapterMaxCount; i++)
            {
                var v = i;
                var wlanAdapterFields = i + 1;
                fields.Add(new Metadata($"WlanAdapter{wlanAdapterFields}Connection", $"Wlan Adapter {wlanAdapterFields} Connection", typeof(string), r => r.DeviceDetailNetworkAdapters?.Where(a => a.IsWlanAdapter).Skip(v).FirstOrDefault()?.NetConnectionID, csvStringEncoded));
                fields.Add(new Metadata($"WlanAdapter{wlanAdapterFields}Manufacturer", $"Wlan Adapter {wlanAdapterFields} Manufacturer", typeof(string), r => r.DeviceDetailNetworkAdapters?.Where(a => a.IsWlanAdapter).Skip(v).FirstOrDefault()?.Manufacturer, csvStringEncoded));
                fields.Add(new Metadata($"WlanAdapter{wlanAdapterFields}ProductName", $"Wlan Adapter {wlanAdapterFields} Product Name", typeof(string), r => r.DeviceDetailNetworkAdapters?.Where(a => a.IsWlanAdapter).Skip(v).FirstOrDefault()?.ProductName, csvStringEncoded));
                fields.Add(new Metadata($"WlanAdapter{wlanAdapterFields}Speed", $"Wlan Adapter {wlanAdapterFields} Speed", typeof(string), r => r.DeviceDetailNetworkAdapters?.Where(a => a.IsWlanAdapter).Skip(v).FirstOrDefault()?.SpeedFriendly(), csvStringEncoded));
                fields.Add(new Metadata($"WlanAdapter{wlanAdapterFields}MacAddress", $"Wlan Adapter {wlanAdapterFields} Mac Address", typeof(string), r => r.DeviceDetailNetworkAdapters?.Where(a => a.IsWlanAdapter).Skip(v).FirstOrDefault()?.MACAddress ?? r.DeviceDetailWlanMacAddresses?.Skip(v).FirstOrDefault(), csvStringEncoded));
            }
            metadata.Add(nameof(DeviceExportOptions.DetailWLanAdapters), fields);

            metadata.Add(nameof(DeviceExportOptions.DetailACAdapter), new List<Metadata>() { new Metadata(nameof(DeviceExportOptions.DetailACAdapter), typeof(string), r => r.DeviceDetails.Where(dd => dd.Key == DeviceDetail.HardwareKeyACAdapter).Select(dd => dd.Value).FirstOrDefault(), csvStringEncoded) });
            metadata.Add(nameof(DeviceExportOptions.DetailBattery), new List<Metadata>() { new Metadata(nameof(DeviceExportOptions.DetailBattery), typeof(string), r => r.DeviceDetails.Where(dd => dd.Key == DeviceDetail.HardwareKeyBattery).Select(dd => dd.Value).FirstOrDefault(), csvStringEncoded) });
            var batteriesFields = new List<Metadata>(processorMaxCount * 6);
            for (int i = 0; i < batteriesMaxCount; i++)
            {
                var v = i;
                var index = i + 1;
                batteriesFields.Add(new Metadata($"Batteries{index}Name", $"Battery {index} Name", typeof(string), r => r.DeviceDetailBatteries?.Skip(v).FirstOrDefault()?.Name, csvStringEncoded));
                batteriesFields.Add(new Metadata($"Batteries{index}Description", $"Battery {index} Description", typeof(string), r => r.DeviceDetailBatteries?.Skip(v).FirstOrDefault()?.Description, csvStringEncoded));
                batteriesFields.Add(new Metadata($"Batteries{index}Availability", $"Battery {index} Availability", typeof(string), r => r.DeviceDetailBatteries?.Skip(v).FirstOrDefault()?.Availability, csvStringEncoded));
                batteriesFields.Add(new Metadata($"Batteries{index}Chemistry", $"Battery {index} Chemistry", typeof(string), r => r.DeviceDetailBatteries?.Skip(v).FirstOrDefault()?.Chemistry, csvStringEncoded));
                batteriesFields.Add(new Metadata($"Batteries{index}DesignVoltage", $"Battery {index} Design Voltage", typeof(long), r => r.DeviceDetailBatteries?.Skip(v).FirstOrDefault()?.DesignVoltage, csvToStringEncoded));
                batteriesFields.Add(new Metadata($"Batteries{index}DesignCapacity", $"Battery {index} Design Capacity", typeof(int), r => r.DeviceDetailBatteries?.Skip(v).FirstOrDefault()?.DesignCapacity, csvToStringEncoded));
                batteriesFields.Add(new Metadata($"Batteries{index}FullChargeCapacity", $"Battery {index} Capacity", typeof(int), r => r.DeviceDetailBatteries?.Skip(v).FirstOrDefault()?.FullChargeCapacity, csvToStringEncoded));
            }
            metadata.Add(nameof(DeviceExportOptions.DetailBatteries), batteriesFields);
            metadata.Add(nameof(DeviceExportOptions.DetailKeyboard), new List<Metadata>() { new Metadata(nameof(DeviceExportOptions.DetailKeyboard), typeof(string), r => r.DeviceDetails.Where(dd => dd.Key == DeviceDetail.HardwareKeyKeyboard).Select(dd => dd.Value).FirstOrDefault(), csvStringEncoded) });
            metadata.Add(nameof(DeviceExportOptions.DetailMdmHardwareData), new List<Metadata>() { new Metadata(nameof(DeviceExportOptions.DetailMdmHardwareData), typeof(string), r => r.DeviceDetails.MdmHardwareData(), csvStringEncoded) });

            return metadata;
        }

    }
}
