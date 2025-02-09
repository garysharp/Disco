using Disco.Data.Repository;
using Disco.Models.Exporting;
using Disco.Models.Repository;
using Disco.Models.Services.Devices;
using Disco.Models.Services.Devices.DeviceFlag;
using Disco.Models.Services.Exporting;
using Disco.Services.Devices.DeviceFlags;
using Disco.Services.Exporting;
using Disco.Services.Plugins.Features.DetailsProvider;
using Disco.Services.Tasks;
using Disco.Services.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Text.Json.Serialization;

namespace Disco.Services.Devices
{
    public class DeviceExport : IExport<DeviceExportOptions, DeviceExportRecord>
    {
        public Guid Id { get; set; }
        public string Name { get; } = "Device Export";
        public DeviceExportOptions Options { get; set; }

        public string FilenamePrefix { get; } = "DeviceExport";
        public string ExcelWorksheetName { get; } = "DeviceExport";
        public string ExcelTableName { get; } = "Devices";

        public DeviceExport(DeviceExportOptions options)
        {
            Id = Guid.NewGuid();
            Options = options;
        }

        [JsonConstructor]
        public DeviceExport()
            : this(DeviceExportOptions.DefaultOptions())
        {
        }

        public ExportResult Export(DiscoDataContext database, IScheduledTaskStatus taskStatus)
            => Exporter.Export(this, database, taskStatus);

        private IQueryable<Device> BuildFilteredRecords(DiscoDataContext database)
        {
            var query = database.Devices
                .Include(d => d.AssignedUser.UserDetails)
                .Include(d => d.DeviceDetails);

            switch (Options.ExportType)
            {
                case DeviceExportTypes.All:
                    break;
                case DeviceExportTypes.Batch:
                    if (Options.ExportTypeTargetId.HasValue && Options.ExportTypeTargetId.Value > 0)
                        query = query.Where(d => d.DeviceBatchId != Options.ExportTypeTargetId);
                    else
                        query = query.Where(d => d.DeviceBatchId != null);
                    break;
                case DeviceExportTypes.Model:
                    query = query.Where(d => d.DeviceModelId == Options.ExportTypeTargetId);
                    break;
                case DeviceExportTypes.Profile:
                    query = query.Where(d => d.DeviceProfileId == Options.ExportTypeTargetId);
                    break;
                default:
                    throw new ArgumentException($"Unknown Device Export Type '{Options.ExportType}'", nameof(Options.ExportType));
            }

            return query;
        }

        public List<DeviceExportRecord> BuildRecords(DiscoDataContext database, IScheduledTaskStatus taskStatus)
        {
            var query = BuildFilteredRecords(database);

            // Update Users
            if (Options.AssignedUserDisplayName ||
                Options.AssignedUserSurname ||
                Options.AssignedUserGivenName ||
                Options.AssignedUserPhoneNumber ||
                Options.AssignedUserEmailAddress)
            {
                taskStatus.UpdateStatus(5, "Refreshing user details from Active Directory");
                var userIds = query.Where(d => d.AssignedUserId != null).Select(d => d.AssignedUserId).Distinct().ToList();
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
                taskStatus.UpdateStatus(15, "Refreshing device last network logon dates from Active Directory");
                try
                {
                    Interop.ActiveDirectory.ADNetworkLogonDatesUpdateTask.UpdateLastNetworkLogonDates(database, ScheduledTaskMockStatus.Create("UpdateLastNetworkLogonDates"));
                    database.SaveChanges();
                }
                catch (Exception) { } // Ignore Errors
            }

            taskStatus.UpdateStatus(25, "Gathering database records");

            var records = query.Select(d => new DeviceExportRecord()
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
            }).ToList();

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
                    var detailsService = new DetailsProviderService(database);
                    r.AssignedUserCustomDetails = detailsService.GetDetails(r.AssignedUser);
                }
            });

            return records;
        }

        public ExportMetadata<DeviceExportRecord> BuildMetadata(DiscoDataContext database, List<DeviceExportRecord> records, IScheduledTaskStatus taskStatus)
        {
            var metadata = new ExportMetadata<DeviceExportRecord>();
            metadata.IgnoreShortNames.Add("Device");
            metadata.IgnoreShortNames.Add("Details");

            // Device
            metadata.Add(Options, o => o.DeviceSerialNumber, r => r.Device.SerialNumber);
            metadata.Add(Options, o => o.DeviceAssetNumber, r => r.Device.AssetNumber);
            metadata.Add(Options, o => o.DeviceLocation, r => r.Device.Location);
            metadata.Add(Options, o => o.DeviceComputerName, r => r.Device.DeviceDomainId);
            metadata.Add(Options, o => o.DeviceLastNetworkLogon, r => r.Device.LastNetworkLogonDate);
            metadata.Add(Options, o => o.DeviceCreatedDate, r => r.Device.CreatedDate);
            metadata.Add(Options, o => o.DeviceFirstEnrolledDate, r => r.Device.EnrolledDate);
            metadata.Add(Options, o => o.DeviceLastEnrolledDate, r => r.Device.LastEnrolDate);
            metadata.Add(Options, o => o.DeviceAllowUnauthenticatedEnrol, r => r.Device.AllowUnauthenticatedEnrol);
            metadata.Add(Options, o => o.DeviceDecommissionedDate, r => r.Device.DecommissionedDate);
            metadata.Add(Options, o => o.DeviceDecommissionedReason, r => r.Device.DecommissionReason?.ToString());

            // Model
            metadata.Add(Options, o => o.ModelId, r => r.ModelId);
            metadata.Add(Options, o => o.ModelDescription, r => r.ModelDescription);
            metadata.Add(Options, o => o.ModelManufacturer, r => r.ModelManufacturer);
            metadata.Add(Options, o => o.ModelModel, r => r.ModelModel);
            metadata.Add(Options, o => o.ModelType, r => r.ModelType);

            // Batch
            metadata.Add(Options, o => o.BatchId, r => r.BatchId);
            metadata.Add(Options, o => o.BatchName, r => r.BatchName);
            metadata.Add(Options, o => o.BatchPurchaseDate, r => r.BatchPurchaseDate);
            metadata.Add(Options, o => o.BatchSupplier, r => r.BatchSupplier);
            metadata.Add(Options, o => o.BatchUnitCost, r => r.BatchUnitCost, Exporter.CsvEncoders.NullableCurrencyEncoder);
            metadata.Add(Options, o => o.BatchWarrantyValidUntilDate, r => r.BatchWarrantyValidUntilDate);
            metadata.Add(Options, o => o.BatchInsuredDate, r => r.BatchInsuredDate);
            metadata.Add(Options, o => o.BatchInsuranceSupplier, r => r.BatchInsuranceSupplier);
            metadata.Add(Options, o => o.BatchInsuredUntilDate, r => r.BatchInsuredUntilDate);

            // Profile
            metadata.Add(Options, o => o.ProfileId, r => r.ProfileId);
            metadata.Add(Options, o => o.ProfileName, r => r.ProfileName);
            metadata.Add(Options, o => o.ProfileShortName, r => r.ProfileShortName);

            // User
            metadata.Add(Options, o => o.AssignedUserId, r => r.AssignedUser?.UserId);
            metadata.Add(Options, o => o.AssignedUserDate, r => r.DeviceUserAssignment?.AssignedDate);
            metadata.Add(Options, o => o.AssignedUserDisplayName, r => r.AssignedUser?.DisplayName);
            metadata.Add(Options, o => o.AssignedUserSurname, r => r.AssignedUser?.Surname);
            metadata.Add(Options, o => o.AssignedUserGivenName, r => r.AssignedUser?.GivenName);
            metadata.Add(Options, o => o.AssignedUserPhoneNumber, r => r.AssignedUser?.PhoneNumber);
            metadata.Add(Options, o => o.AssignedUserEmailAddress, r => r.AssignedUser?.EmailAddress);

            // User Custom Details
            if (Options.AssignedUserDetailCustom)
            {
                var keys = records.Where(r => r.AssignedUserCustomDetails != null).SelectMany(r => r.AssignedUserCustomDetails.Keys).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
                foreach (var key in keys.OrderBy(k => k, StringComparer.OrdinalIgnoreCase))
                {
                    metadata.Add(key, r => r.AssignedUserCustomDetails != null && r.AssignedUserCustomDetails.TryGetValue(key, out var value) ? value : null);
                }
            }

            // Jobs
            metadata.Add(Options, o => o.JobsTotalCount, r => r.JobsTotalCount);
            metadata.Add(Options, o => o.JobsOpenCount, r => r.JobsOpenCount);

            // Attachments
            metadata.Add(Options, o => o.AttachmentsCount, r => r.AttachmentsCount);

            // Certificates
            if (Options.Certificates)
            {
                var certificateMaxCount = Math.Max(1, records.Max(r => r.DeviceCertificates?.Count() ?? 0));
                for (int i = 0; i < certificateMaxCount; i++)
                {
                    var v = i;
                    var index = i + 1;
                    metadata.Add($"Certificate{index}Name", r => r.DeviceCertificates.Skip(v).FirstOrDefault()?.Name);
                    metadata.Add($"Certificate{index}AllocationDate", r => r.DeviceCertificates.Skip(v).FirstOrDefault()?.AllocatedDate);
                    metadata.Add($"Certificate{index}ExpirationDate", r => r.DeviceCertificates.Skip(v).FirstOrDefault()?.ExpirationDate);
                    metadata.Add($"Certificate{index}ProviderId", r => r.DeviceCertificates.Skip(v).FirstOrDefault()?.ProviderId);
                }
            }

            // Details

            // BIOS
            if (Options.DetailBios)
            {
                metadata.Add("BIOS Manufacturer", r => r.DeviceDetailBios?.FirstOrDefault()?.Manufacturer);
                metadata.Add("BIOS Serial Number", r => r.DeviceDetailBios?.FirstOrDefault()?.SerialNumber);
                metadata.Add("BIOS Version", r =>
                {
                    var bios = r.DeviceDetailBios?.FirstOrDefault();
                    if (bios?.SMBIOSBIOSVersion != null)
                        return $"{bios.SMBIOSBIOSVersion} {bios.SMBIOSMajorVersion}.{bios.SMBIOSMinorVersion}";
                    else
                        return null;
                });
                metadata.Add("BIOS System Version", r =>
                {
                    var bios = r.DeviceDetailBios?.FirstOrDefault();
                    if (bios?.SystemBiosMajorVersion.HasValue ?? false)
                        return $"{bios.SystemBiosMajorVersion}.{bios.SystemBiosMinorVersion}";
                    else
                        return null;
                });
                metadata.Add("BIOS Release Date", r => r.DeviceDetailBios?.FirstOrDefault()?.ReleaseDate);
            }

            // Base Board
            if (Options.DetailBaseBoard)
            {
                metadata.Add("Base Board Manufacturer", r => r.DeviceDetailBaseBoard?.FirstOrDefault()?.Manufacturer);
                metadata.Add("Base Board Model", r => r.DeviceDetailBaseBoard?.FirstOrDefault()?.Model);
                metadata.Add("Base Board Product", r => r.DeviceDetailBaseBoard?.FirstOrDefault()?.Product);
                metadata.Add("Base Board Part Number", r => r.DeviceDetailBaseBoard?.FirstOrDefault()?.PartNumber);
                metadata.Add("Base Board SKU", r => r.DeviceDetailBaseBoard?.FirstOrDefault()?.SKU);
                metadata.Add("Base Board Serial Number", r => r.DeviceDetailBaseBoard?.FirstOrDefault()?.SerialNumber);
                metadata.Add("Base Board Config Options", r =>
                {
                    var baseBoard = r.DeviceDetailBaseBoard?.FirstOrDefault();
                    if (baseBoard?.ConfigOptions != null)
                        return string.Join("; ", baseBoard.ConfigOptions);
                    else
                        return null;
                });
                metadata.Add("Base Board Version", r => r.DeviceDetailBaseBoard?.FirstOrDefault()?.Version);
            }

            // Computer System
            if (Options.DetailComputerSystem)
            {
                metadata.Add("System Description", r => r.DeviceDetailComputerSystem?.FirstOrDefault()?.Description);
                metadata.Add("System Form Factor", r => r.DeviceDetailComputerSystem?.FirstOrDefault()?.PCSystemType);
                metadata.Add("System Type", r => r.DeviceDetailComputerSystem?.FirstOrDefault()?.SystemType);
                metadata.Add("System Primary Owner Name", r => r.DeviceDetailComputerSystem?.FirstOrDefault()?.PrimaryOwnerName);
                metadata.Add("System Primary Owner Contact", r => r.DeviceDetailComputerSystem?.FirstOrDefault()?.PrimaryOwnerContact);
                metadata.Add("System Chassis SKU", r => r.DeviceDetailComputerSystem?.FirstOrDefault()?.ChassisSKUNumber);
                metadata.Add("System SKU", r => r.DeviceDetailComputerSystem?.FirstOrDefault()?.SystemSKUNumber);
                metadata.Add("System OEM Reference", r =>
                {
                    var computerSystem = r.DeviceDetailComputerSystem?.FirstOrDefault();
                    if (computerSystem?.OEMStringArray != null)
                        return string.Join("; ", computerSystem.OEMStringArray);
                    else
                        return null;
                });
                metadata.Add("System Time Zone", r =>
                {
                    var computerSystem = r.DeviceDetailComputerSystem?.FirstOrDefault();
                    if (computerSystem?.CurrentTimeZone.HasValue ?? false)
                        return $"{computerSystem.CurrentTimeZone.Value / 60:00}:{Math.Abs(computerSystem.CurrentTimeZone.Value % 60):00}";
                    else
                        return null;
                });
                metadata.Add("System Roles", r =>
                {
                    var computerSystem = r.DeviceDetailComputerSystem?.FirstOrDefault();
                    if (computerSystem?.Roles != null)
                        return string.Join("; ", computerSystem.Roles);
                    else
                        return null;
                });
            }

            // Processors
            if (Options.DetailProcessors)
            {
                var processorMaxCount = Math.Max(1, records.Max(r => r.DeviceDetailProcessors?.Count ?? 0));
                for (int i = 0; i < processorMaxCount; i++)
                {
                    var v = i;
                    var index = i + 1;
                    metadata.Add($"Processor{index}Name", r => r.DeviceDetailProcessors?.Skip(v).FirstOrDefault()?.Name);
                    metadata.Add($"Processor{index}Description", r => r.DeviceDetailProcessors?.Skip(v).FirstOrDefault()?.Description);
                    metadata.Add($"Processor{index}Architecture", r => r.DeviceDetailProcessors?.Skip(v).FirstOrDefault()?.Architecture);
                    metadata.Add($"Processor{index}ClockSpeed", r => r.DeviceDetailProcessors?.Skip(v).FirstOrDefault()?.MaxClockSpeedFriendly());
                    metadata.Add($"Processor{index}Cores", r => r.DeviceDetailProcessors?.Skip(v).FirstOrDefault()?.NumberOfCores);
                    metadata.Add($"Processor{index}LogicalProcessors", r => r.DeviceDetailProcessors?.Skip(v).FirstOrDefault()?.NumberOfLogicalProcessors);
                }
            }

            // Memory
            if (Options.DetailMemory)
            {
                metadata.Add("Memory Total Capacity", r => MeasurementUnitExtensions.ByteSizeToFriendly((ulong)(r.DeviceDetailPhysicalMemory?.Sum(m => (long)m.Capacity) ?? 0L)));
                var memoryMaxCount = Math.Max(1, records.Max(r => r.DeviceDetailPhysicalMemory?.Count ?? 0));
                for (int i = 0; i < memoryMaxCount; i++)
                {
                    var v = i;
                    var index = i + 1;
                    metadata.Add($"Memory{index}Location", r => r.DeviceDetailPhysicalMemory?.Skip(v).FirstOrDefault()?.DeviceLocator);
                    metadata.Add($"Memory{index}Manufacturer", r => r.DeviceDetailPhysicalMemory?.Skip(v).FirstOrDefault()?.Manufacturer);
                    metadata.Add($"Memory{index}PartNumber", r => r.DeviceDetailPhysicalMemory?.Skip(v).FirstOrDefault()?.PartNumber);
                    metadata.Add($"Memory{index}SerialNumber", r => r.DeviceDetailPhysicalMemory?.Skip(v).FirstOrDefault()?.SerialNumber);
                    metadata.Add($"Memory{index}Capacity", r => r.DeviceDetailPhysicalMemory?.Skip(v).FirstOrDefault()?.CapacityFriendly());
                    metadata.Add($"Memory{index}ConfiguredClockSpeed", r => r.DeviceDetailPhysicalMemory?.Skip(v).FirstOrDefault()?.ConfiguredClockSpeed);
                }
            }

            // Disk Drives
            if (Options.DetailDiskDrives)
            {
                var diskDriveMaxCount = Math.Max(1, records.Max(r => r.DeviceDetailDiskDrives?.Count ?? 0));
                for (int i = 0; i < diskDriveMaxCount; i++)
                {
                    var v = i;
                    var index = i + 1;
                    metadata.Add($"Disk {index} Manufacturer", r => r.DeviceDetailDiskDrives?.Skip(v).FirstOrDefault()?.Manufacturer);
                    metadata.Add($"Disk {index} Model", r => r.DeviceDetailDiskDrives?.Skip(v).FirstOrDefault()?.Model);
                    metadata.Add($"Disk {index} Serial Number", r => r.DeviceDetailDiskDrives?.Skip(v).FirstOrDefault()?.SerialNumber);
                    metadata.Add($"Disk {index} Firmware", r => r.DeviceDetailDiskDrives?.Skip(v).FirstOrDefault()?.FirmwareRevision);
                    metadata.Add($"Disk {index} Size", r => r.DeviceDetailDiskDrives?.Skip(v).FirstOrDefault()?.SizeFriendly());
                    metadata.Add($"Disk {index} Total Free Space", r => MeasurementUnitExtensions.ByteSizeToFriendly((ulong)(r.DeviceDetailDiskDrives?.Skip(v).FirstOrDefault()?.Partitions?.Sum(p => (long)(p.LogicalDisk?.FreeSpace ?? 0L)) ?? 0L)));
                }
            }

            // Local Network Adapters
            if (Options.DetailLanAdapters)
            {
                var lanAdapterMaxCount = Math.Max(1, records.Max(r => r.DeviceDetailNetworkAdapters?.Count(a => !a.IsWlanAdapter) ?? r.DeviceDetailLanMacAddresses?.Count ?? 0));
                for (int i = 0; i < lanAdapterMaxCount; i++)
                {
                    var v = i;
                    var index = i + 1;
                    metadata.Add($"Lan Adapter {index} Connection", r => r.DeviceDetailNetworkAdapters?.Where(a => !a.IsWlanAdapter).Skip(v).FirstOrDefault()?.NetConnectionID);
                    metadata.Add($"Lan Adapter {index} Manufacturer", r => r.DeviceDetailNetworkAdapters?.Where(a => !a.IsWlanAdapter).Skip(v).FirstOrDefault()?.Manufacturer);
                    metadata.Add($"Lan Adapter {index} Product Name", r => r.DeviceDetailNetworkAdapters?.Where(a => !a.IsWlanAdapter).Skip(v).FirstOrDefault()?.ProductName);
                    metadata.Add($"Lan Adapter {index} Speed", r => r.DeviceDetailNetworkAdapters?.Where(a => !a.IsWlanAdapter).Skip(v).FirstOrDefault()?.SpeedFriendly());
                    metadata.Add($"Lan Adapter {index} Mac Address", r => r.DeviceDetailNetworkAdapters?.Where(a => !a.IsWlanAdapter).Skip(v).FirstOrDefault()?.MACAddress ?? r.DeviceDetailLanMacAddresses?.Skip(v).FirstOrDefault());
                }
            }

            // Wireless Network Adapters
            if (Options.DetailWLanAdapters)
            {
                var wlanAdapterMaxCount = Math.Max(1, records.Max(r => r.DeviceDetailNetworkAdapters?.Count(a => a.IsWlanAdapter) ?? r.DeviceDetailWlanMacAddresses?.Count ?? 0));
                for (int i = 0; i < wlanAdapterMaxCount; i++)
                {
                    var v = i;
                    var index = i + 1;
                    metadata.Add($"Wlan Adapter {index} Connection", r => r.DeviceDetailNetworkAdapters?.Where(a => a.IsWlanAdapter).Skip(v).FirstOrDefault()?.NetConnectionID);
                    metadata.Add($"Wlan Adapter {index} Manufacturer", r => r.DeviceDetailNetworkAdapters?.Where(a => a.IsWlanAdapter).Skip(v).FirstOrDefault()?.Manufacturer);
                    metadata.Add($"Wlan Adapter {index} Product Name", r => r.DeviceDetailNetworkAdapters?.Where(a => a.IsWlanAdapter).Skip(v).FirstOrDefault()?.ProductName);
                    metadata.Add($"Wlan Adapter {index} Speed", r => r.DeviceDetailNetworkAdapters?.Where(a => a.IsWlanAdapter).Skip(v).FirstOrDefault()?.SpeedFriendly());
                    metadata.Add($"Wlan Adapter {index} Mac Address", r => r.DeviceDetailNetworkAdapters?.Where(a => a.IsWlanAdapter).Skip(v).FirstOrDefault()?.MACAddress ?? r.DeviceDetailWlanMacAddresses?.Skip(v).FirstOrDefault());
                }
            }

            metadata.Add(Options, o => o.DetailACAdapter, r => r.DeviceDetails.Where(dd => dd.Key == DeviceDetail.HardwareKeyACAdapter).Select(dd => dd.Value).FirstOrDefault());

            // Batteries
            metadata.Add(Options, o => o.DetailBattery, r => r.DeviceDetails.Where(dd => dd.Key == DeviceDetail.HardwareKeyBattery).Select(dd => dd.Value).FirstOrDefault());
            if (Options.DetailBatteries)
            {
                var batteriesMaxCount = Math.Max(1, records.Max(r => r.DeviceDetailBatteries?.Count ?? 0));
                for (int i = 0; i < batteriesMaxCount; i++)
                {
                    var v = i;
                    var index = i + 1;
                    metadata.Add($"Battery {index} Name", r => r.DeviceDetailBatteries?.Skip(v).FirstOrDefault()?.Name);
                    metadata.Add($"Battery {index} Description", r => r.DeviceDetailBatteries?.Skip(v).FirstOrDefault()?.Description);
                    metadata.Add($"Battery {index} Availability", r => r.DeviceDetailBatteries?.Skip(v).FirstOrDefault()?.Availability);
                    metadata.Add($"Battery {index} Chemistry", r => r.DeviceDetailBatteries?.Skip(v).FirstOrDefault()?.Chemistry);
                    metadata.Add($"Battery {index} Design Voltage", r => r.DeviceDetailBatteries?.Skip(v).FirstOrDefault()?.DesignVoltage);
                    metadata.Add($"Battery {index} Design Capacity", r => r.DeviceDetailBatteries?.Skip(v).FirstOrDefault()?.DesignCapacity);
                    metadata.Add($"Battery {index} Capacity", r => r.DeviceDetailBatteries?.Skip(v).FirstOrDefault()?.FullChargeCapacity);
                }
            }

            metadata.Add(Options, o => o.DetailKeyboard, r => r.DeviceDetails.Where(dd => dd.Key == DeviceDetail.HardwareKeyKeyboard).Select(dd => dd.Value).FirstOrDefault());
            metadata.Add(Options, o => o.DetailMdmHardwareData, r => r.DeviceDetails.MdmHardwareData());

            return metadata;
        }
    }
}
