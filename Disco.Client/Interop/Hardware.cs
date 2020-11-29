using Disco.Models.ClientServices;
using Disco.Models.ClientServices.EnrolmentInformation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;

namespace Disco.Client.Interop
{
    public static class Hardware
    {
        private static DeviceHardware information;

        public static DeviceHardware Information
        {
            get
            {
                if (information == null)
                {
                    information = GatherInformation();
                }
                return information;
            }
        }

        private static DeviceHardware GatherInformation()
        {
            var audit = new DeviceHardware();

            audit.ApplyBIOSInformation();
            audit.ApplySystemInformation();
            audit.ApplyBaseBoardInformation();
            audit.ApplySystemProductInformation();

            if (string.IsNullOrWhiteSpace(audit.SerialNumber))
            {
                throw new Exception("This device has no serial number stored in BIOS or BaseBoard");
            }
            if (audit.SerialNumber.Length > 60)
            {
                throw new Exception($"The serial number reported by this device is over 60 characters long:\r\n{audit.SerialNumber}");
            }

            audit.ApplyProcessorInformation();
            audit.ApplyPhysicalMemoryInformation();
            audit.ApplyDiskDriveInformation();

            audit.NetworkAdapters = Network.GetNetworkAdapters();

            return audit;
        }

        private static void ApplyProcessorInformation(this DeviceHardware deviceHardware)
        {
            try
            {
                using (var mSearcher = new ManagementObjectSearcher("SELECT DeviceID, Manufacturer, Name, Description, Architecture, Family, MaxClockSpeed, NumberOfCores, NumberOfLogicalProcessors FROM Win32_Processor"))
                {
                    using (var mResults = mSearcher.Get())
                    {
                        if (mResults.Count > 0)
                        {
                            var processors = new List<Processor>(mResults.Count);
                            foreach (var mItem in mResults.Cast<ManagementObject>())
                            {
                                if (mItem != null)
                                {
                                    var processor = new Processor();

                                    processor.DeviceID = (string)mItem.GetPropertyValue("DeviceID");
                                    processor.Manufacturer = (string)mItem.GetPropertyValue("Manufacturer");
                                    processor.Name = (string)mItem.GetPropertyValue("Name");
                                    processor.Description = (string)mItem.GetPropertyValue("Description");
                                    processor.Family = (ushort)mItem.GetPropertyValue("Family");
                                    processor.MaxClockSpeed = (uint)mItem.GetPropertyValue("MaxClockSpeed");
                                    processor.NumberOfCores = (uint)mItem.GetPropertyValue("NumberOfCores");
                                    processor.NumberOfLogicalProcessors = (uint)mItem.GetPropertyValue("NumberOfLogicalProcessors");

                                    var architectureCode = (ushort)mItem.GetPropertyValue("Architecture");
                                    processor.Architecture = Enum.GetName(typeof(ProcessorArchitectures), architectureCode) ?? $"Unknown {architectureCode}";

                                    processors.Add(processor);
                                }
                            }
                            deviceHardware.Processors = processors;
                        }
                    }
                }
            }
            catch (Exception)
            {
                // ignore errors to ensure backwards compatibility
            }
        }

        private static void ApplyPhysicalMemoryInformation(this DeviceHardware deviceHardware)
        {
            try
            {
                using (var mSearcher = new ManagementObjectSearcher("SELECT Tag, SerialNumber, Manufacturer, PartNumber, Capacity, ConfiguredClockSpeed, Speed, DeviceLocator FROM Win32_PhysicalMemory"))
                {
                    using (var mResults = mSearcher.Get())
                    {
                        if (mResults.Count > 0)
                        {
                            var physicalMemories = new List<PhysicalMemory>(mResults.Count);
                            foreach (var mItem in mResults.Cast<ManagementObject>())
                            {
                                if (mItem != null)
                                {
                                    var physicalMemory = new PhysicalMemory();

                                    physicalMemory.Tag = (string)mItem.GetPropertyValue("Tag");
                                    physicalMemory.SerialNumber = (string)mItem.GetPropertyValue("SerialNumber");
                                    physicalMemory.Manufacturer = (string)mItem.GetPropertyValue("Manufacturer");
                                    physicalMemory.PartNumber = (string)mItem.GetPropertyValue("PartNumber");
                                    physicalMemory.Capacity = (ulong)mItem.GetPropertyValue("Capacity");
                                    physicalMemory.ConfiguredClockSpeed = (uint)mItem.GetPropertyValue("ConfiguredClockSpeed");
                                    physicalMemory.Speed = (uint)mItem.GetPropertyValue("Speed");
                                    physicalMemory.DeviceLocator = (string)mItem.GetPropertyValue("DeviceLocator");

                                    physicalMemories.Add(physicalMemory);
                                }
                            }
                            deviceHardware.PhysicalMemory = physicalMemories;
                        }
                    }
                }
            }
            catch (Exception)
            {
                // ignore errors to ensure backwards compatibility
            }
        }

        private static void ApplyDiskDriveInformation(this DeviceHardware deviceHardware)
        {
            try
            {
                using (var diskSearcher = new ManagementObjectSearcher("SELECT DeviceID, Manufacturer, Model, MediaType, InterfaceType, SerialNumber, FirmwareRevision, Size  FROM Win32_DiskDrive"))
                {
                    using (var diskResults = diskSearcher.Get())
                    {
                        if (diskResults.Count > 0)
                        {
                            var diskDrives = new List<DiskDrive>(diskResults.Count);
                            foreach (var diskResult in diskResults.Cast<ManagementObject>())
                            {
                                if (diskResult != null)
                                {
                                    var diskDrive = new DiskDrive();

                                    diskDrive.DeviceID = (string)diskResult.GetPropertyValue("DeviceID");
                                    diskDrive.Manufacturer = (string)diskResult.GetPropertyValue("Manufacturer");
                                    diskDrive.Model = (string)diskResult.GetPropertyValue("Model");
                                    diskDrive.MediaType = (string)diskResult.GetPropertyValue("MediaType");
                                    diskDrive.InterfaceType = (string)diskResult.GetPropertyValue("InterfaceType");
                                    diskDrive.SerialNumber = (string)diskResult.GetPropertyValue("SerialNumber");
                                    diskDrive.FirmwareRevision = (string)diskResult.GetPropertyValue("FirmwareRevision");
                                    diskDrive.Size = (ulong)diskResult.GetPropertyValue("Size");

                                    using (var partitionSearcher = new ManagementObjectSearcher($"ASSOCIATORS OF {{Win32_DiskDrive.DeviceID=\"{diskDrive.DeviceID.Replace(@"\", @"\\")}\"}} WHERE AssocClass = Win32_DiskDriveToDiskPartition"))
                                    {
                                        using (var partitionResults = partitionSearcher.Get())
                                        {
                                            if (partitionResults.Count > 0)
                                            {
                                                var partitions = new List<DiskDrivePartition>(partitionResults.Count);
                                                foreach (var partitionResult in partitionResults.Cast<ManagementObject>())
                                                {
                                                    if (partitionResult != null)
                                                    {
                                                        var partition = new DiskDrivePartition();

                                                        partition.DeviceID = (string)partitionResult.GetPropertyValue("DeviceID");
                                                        partition.Bootable = (bool)partitionResult.GetPropertyValue("Bootable");
                                                        partition.BootPartition = (bool)partitionResult.GetPropertyValue("BootPartition");
                                                        partition.PrimaryParition = (bool)partitionResult.GetPropertyValue("PrimaryPartition");
                                                        partition.Size = (ulong)partitionResult.GetPropertyValue("Size");
                                                        partition.StartingOffset = (ulong)partitionResult.GetPropertyValue("StartingOffset");
                                                        partition.Type = (string)partitionResult.GetPropertyValue("Type");

                                                        using (var logicalSearcher = new ManagementObjectSearcher($"ASSOCIATORS OF {{Win32_DiskPartition.DeviceID=\"{partition.DeviceID}\"}} WHERE AssocClass = Win32_LogicalDiskToPartition"))
                                                        {
                                                            using (var logicalResults = logicalSearcher.Get())
                                                            {
                                                                if (logicalResults.Count > 0)
                                                                {
                                                                    foreach (var logicalResult in logicalResults.Cast<ManagementObject>().Take(1))
                                                                    {
                                                                        if (logicalResult != null)
                                                                        {
                                                                            var logical = new DiskLogical();

                                                                            logical.DeviceID = (string)logicalResult.GetPropertyValue("DeviceID");
                                                                            logical.Description = (string)logicalResult.GetPropertyValue("Description");
                                                                            var driveType = (uint)logicalResult.GetPropertyValue("DriveType");
                                                                            logical.DriveType = Enum.GetName(typeof(DiskLogicalDriveTypes), driveType) ?? $"Unknown {driveType}";
                                                                            var mediaType = (uint)logicalResult.GetPropertyValue("MediaType");
                                                                            logical.MediaType = Enum.GetName(typeof(DiskLogicalMediaTypes), mediaType) ?? $"Unknown {mediaType}";
                                                                            logical.FileSystem = (string)logicalResult.GetPropertyValue("FileSystem");
                                                                            logical.Size = (ulong)logicalResult.GetPropertyValue("Size");
                                                                            logical.FreeSpace = (ulong)logicalResult.GetPropertyValue("FreeSpace");
                                                                            logical.VolumeName = (string)logicalResult.GetPropertyValue("VolumeName");
                                                                            logical.VolumeSerialNumber = (string)logicalResult.GetPropertyValue("VolumeSerialNumber");

                                                                            partition.LogicalDisk = logical;
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }

                                                        partitions.Add(partition);
                                                    }
                                                }
                                                diskDrive.Partitions = partitions;
                                            }
                                        }
                                    }

                                    diskDrives.Add(diskDrive);
                                }
                            }

                            deviceHardware.DiskDrives = diskDrives;
                        }
                    }
                }
            }
            catch (Exception)
            {
                // ignore errors to ensure backwards compatibility
            }
        }

        private static void ApplyBIOSInformation(this DeviceHardware deviceHardware)
        {
            try
            {
                using (var mSearcher = new ManagementObjectSearcher("SELECT SerialNumber, Manufacturer, SMBIOSBIOSVersion FROM Win32_BIOS WHERE PrimaryBios=true"))
                {
                    using (var mResults = mSearcher.Get())
                    {
                        using (var mItem = mResults.Cast<ManagementObject>().FirstOrDefault())
                        {
                            if (mItem != null)
                            {
                                var serialNumber = (string)mItem.GetPropertyValue("SerialNumber");
                                if (!string.IsNullOrWhiteSpace(serialNumber))
                                {
                                    deviceHardware.SerialNumber = serialNumber.Trim();
                                }

                                var manufacturer = (string)mItem.GetPropertyValue("Manufacturer");
                                if (deviceHardware.Manufacturer == null && !string.IsNullOrWhiteSpace(manufacturer))
                                {
                                    deviceHardware.Manufacturer = manufacturer.Trim();
                                }

                                ErrorReporting.DeviceIdentifier = deviceHardware.SerialNumber;
                            }
                            else
                            {
                                throw new Exception("No Win32_BIOS WHERE PrimaryBios=true was found");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Disco Client was unable to retrieve BIOS information from WMI", ex);
            }
        }

        private static void ApplySystemInformation(this DeviceHardware deviceHardware)
        {
            try
            {
                using (var mSearcher = new ManagementObjectSearcher("SELECT Manufacturer, Model, PCSystemType FROM Win32_ComputerSystem"))
                {
                    using (var mResults = mSearcher.Get())
                    {
                        using (var mItem = mResults.Cast<ManagementObject>().FirstOrDefault())
                        {
                            if (mItem != null)
                            {
                                var manufacturer = (string)mItem.GetPropertyValue("Manufacturer");
                                if (!string.IsNullOrWhiteSpace(manufacturer))
                                {
                                    deviceHardware.Manufacturer = manufacturer.Trim();
                                }

                                var model = (string)mItem.GetPropertyValue("Model");
                                if (!string.IsNullOrWhiteSpace(model))
                                {
                                    deviceHardware.Model = model.ToString();
                                }

                                deviceHardware.ModelType = ((PCSystemTypes)mItem.GetPropertyValue("PCSystemType")).Description();
                            }
                            else
                            {
                                throw new Exception("No Win32_ComputerSystem was found");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Disco Client was unable to retrieve ComputerSystem information from WMI", ex);
            }
        }

        public static void ApplySystemInformation(this Enrol enrol)
        {
            try
            {
                using (var mSearcher = new ManagementObjectSearcher("SELECT PartOfDomain, Domain FROM Win32_ComputerSystem"))
                {
                    using (var mResults = mSearcher.Get())
                    {
                        using (var mItem = mResults.Cast<ManagementObject>().FirstOrDefault())
                        {
                            if (mItem != null)
                            {
                                enrol.IsPartOfDomain = (bool)mItem.GetPropertyValue("PartOfDomain");

                                if (enrol.IsPartOfDomain)
                                {
                                    enrol.DNSDomainName = (string)mItem.GetPropertyValue("Domain");
                                }
                            }
                            else
                            {
                                throw new Exception("No Win32_ComputerSystem was found");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Disco Client was unable to retrieve ComputerSystem information from WMI", ex);
            }
        }

        private static void ApplyBaseBoardInformation(this DeviceHardware deviceHardware)
        {
            try
            {
                using (var mSearcher = new ManagementObjectSearcher("SELECT SerialNumber, Manufacturer, Model, Product FROM Win32_BaseBoard"))
                {
                    using (var mResults = mSearcher.Get())
                    {
                        using (var mItem = mResults.Cast<ManagementObject>().FirstOrDefault())
                        {
                            if (mItem != null)
                            {
                                // Apply Manufacturer/Model information only if not previously found in Win32_ComputerSystem
                                var manufacturer = (string)mItem.GetPropertyValue("Manufacturer");
                                if (deviceHardware.Manufacturer == null && !string.IsNullOrWhiteSpace(manufacturer))
                                {
                                    deviceHardware.Manufacturer = manufacturer.Trim();
                                }

                                var model = (string)mItem.GetPropertyValue("Model");
                                if (deviceHardware.Model == null && !string.IsNullOrWhiteSpace(model))
                                {
                                    deviceHardware.Model = model.ToString();
                                }

                                var product = (string)mItem.GetPropertyValue("Product");
                                if (deviceHardware.Model == null && !string.IsNullOrWhiteSpace(product))
                                {
                                    deviceHardware.Model = product.ToString();
                                }

                                // Added 2012-11-22 G# - Lenovo IdeaPad Serial SHIM
                                // http://www.discoict.com.au/forum/feature-requests/2012/11/serial-number-detection-on-ideapads.aspx
                                var serialNumber = (string)mItem.GetPropertyValue("SerialNumber");
                                if (!string.IsNullOrWhiteSpace(serialNumber) &&
                                    (deviceHardware.SerialNumber == null ||
                                    ((deviceHardware.Manufacturer?.Equals("LENOVO", StringComparison.OrdinalIgnoreCase) ?? false) &&
                                    ((deviceHardware.Model?.Equals("S10-3", StringComparison.OrdinalIgnoreCase) ?? false) // S10-3
                                    || (deviceHardware.Model?.Equals("2957", StringComparison.OrdinalIgnoreCase) ?? false)))))
                                {
                                    deviceHardware.SerialNumber = serialNumber.Trim();
                                    ErrorReporting.DeviceIdentifier = deviceHardware.SerialNumber;
                                }
                            }
                            else
                            {
                                throw new Exception("No Win32_BaseBoard was found");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Disco Client was unable to retrieve BaseBoard information from WMI", ex);
            }
        }

        private static void ApplySystemProductInformation(this DeviceHardware deviceHardware)
        {
            try
            {
                using (var mSearcher = new ManagementObjectSearcher("SELECT IdentifyingNumber, UUID FROM Win32_ComputerSystemProduct"))
                {
                    using (var mResults = mSearcher.Get())
                    {
                        using (var mItem = mResults.Cast<ManagementObject>().FirstOrDefault())
                        {
                            if (mItem != null)
                            {
                                if (deviceHardware.SerialNumber == null)
                                {
                                    var serialNumber = mItem.GetPropertyValue("IdentifyingNumber") as string;
                                    if (!string.IsNullOrWhiteSpace(serialNumber))
                                    {
                                        deviceHardware.SerialNumber = serialNumber.Trim();
                                        ErrorReporting.DeviceIdentifier = deviceHardware.SerialNumber;
                                    }
                                }

                                var uUID = (string)mItem.GetPropertyValue("UUID");
                                if (!string.IsNullOrWhiteSpace(uUID))
                                {
                                    deviceHardware.UUID = uUID.Trim();

                                    // if serial number is absent attempt using UUID if valid
                                    if (string.IsNullOrWhiteSpace(deviceHardware.SerialNumber))
                                    {
                                        Guid uuidGuid;
                                        if (Guid.TryParse(deviceHardware.UUID, out uuidGuid) && uuidGuid != Guid.Empty)
                                        {
                                            deviceHardware.SerialNumber = $"UUID{uuidGuid:N}";
                                        }
                                    }
                                }
                            }
                            else
                            {
                                throw new Exception("No Win32_ComputerSystemProduct was found");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Disco Client was unable to retrieve ComputerSystemProduct information from WMI", ex);
            }
        }

        private static string Description(this PCSystemTypes type)
        {
            switch (type)
            {
                case PCSystemTypes.Desktop:
                    return "Desktop";
                case PCSystemTypes.Mobile:
                    return "Mobile";
                case PCSystemTypes.Workstation:
                    return "Workstation";
                case PCSystemTypes.EnterpriseServer:
                    return "Enterprise Server";
                case PCSystemTypes.SmallOfficeAndHomeOfficeServer:
                    return "Small Office And Home Office Server";
                case PCSystemTypes.AppliancePC:
                    return "Appliance PC";
                case PCSystemTypes.PerformanceServer:
                    return "Performance Server";
                case PCSystemTypes.Maximum:
                    return "Maximum";
                case PCSystemTypes.Unknown:
                default:
                    return "Unknown";
            }
        }

        private enum PCSystemTypes : ushort
        {
            Unknown = 0,
            Desktop,
            Mobile,
            Workstation,
            EnterpriseServer,
            SmallOfficeAndHomeOfficeServer,
            AppliancePC,
            PerformanceServer,
            Maximum
        }

        private enum ProcessorArchitectures : ushort
        {
            x86 = 0,
            MIPS = 1,
            Alpha = 2,
            PowerPC = 3,
            ia64 = 6,
            x64 = 9,
        }

        private enum DiskLogicalDriveTypes : uint
        {
            Unknown = 0,
            NoRootDirectory,
            Removable,
            Fixed,
            Remote,
            CDRom,
            RAMDisk,
        }

        private enum DiskLogicalMediaTypes : uint
        {
            Unknown = 0,
            F5_1Pt2_512,
            F3_1Pt44_512,
            F3_2Pt88_512,
            F3_20Pt8_512,
            F3_720_512,
            F5_360_512,
            F5_320_512,
            F5_320_1024,
            F5_180_512,
            F5_160_512,
            RemovableMedia,
            FixedMedia,
            F3_120M_512,
            F3_640_512,
            F5_640_512,
            F5_720_512,
            F3_1Pt2_512,
            F3_1Pt23_1024,
            F5_1Pt23_1024,
            F3_128Mb_512,
            F3_230Mb_512,
            F8_256_128,
            F3_200Mb_512,
            F3_240M_512,
            F3_32M_512,
        }
    }
}
