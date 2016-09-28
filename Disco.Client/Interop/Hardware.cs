using Disco.Models.ClientServices;
using Disco.Models.ClientServices.EnrolmentInformation;
using System;
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

#warning TODO: Processors, PhysicalMemory, DiskDrives, etc

            audit.NetworkAdapters = Network.GetNetworkAdapters();

            return audit;
        }

        private static void ApplyBIOSInformation(this DeviceHardware DeviceHardware)
        {
            try
            {
                using (var mSearcher = new ManagementObjectSearcher("SELECT SerialNumber, SMBIOSBIOSVersion FROM Win32_BIOS WHERE PrimaryBios=true"))
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
                                    DeviceHardware.SerialNumber = serialNumber.Trim();
                                }

                                ErrorReporting.DeviceIdentifier = DeviceHardware.SerialNumber;
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

        private static void ApplySystemInformation(this DeviceHardware DeviceHardware)
        {
            try
            {
                using (var mSearcher = new ManagementObjectSearcher("SELECT Manufacturer, Model, PartOfDomain, PCSystemType, Domain FROM Win32_ComputerSystem"))
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
                                    DeviceHardware.Manufacturer = manufacturer.Trim();
                                }

                                var model = (string)mItem.GetPropertyValue("Model");
                                if (!string.IsNullOrWhiteSpace(model))
                                {
                                    DeviceHardware.Model = model.ToString();
                                }

                                DeviceHardware.ModelType = ((PCSystemTypes)mItem.GetPropertyValue("PCSystemType")).Description();
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

        public static void ApplySystemInformation(this Enrol Enrol)
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
                                Enrol.IsPartOfDomain = (bool)mItem.GetPropertyValue("PartOfDomain");
                                
                                if (Enrol.IsPartOfDomain)
                                {
                                    Enrol.DNSDomainName = (string)mItem.GetPropertyValue("Domain");
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

        private static void ApplyBaseBoardInformation(this DeviceHardware DeviceHardware)
        {
            // Added 2012-11-22 G# - Lenovo IdeaPad Serial SHIM
            // http://www.discoict.com.au/forum/feature-requests/2012/11/serial-number-detection-on-ideapads.aspx
            if (string.IsNullOrWhiteSpace(DeviceHardware.SerialNumber) ||
                (DeviceHardware.Manufacturer.Equals("LENOVO", StringComparison.OrdinalIgnoreCase) &&
                (DeviceHardware.Model.Equals("S10-3", StringComparison.OrdinalIgnoreCase) // S10-3
                || DeviceHardware.Model.Equals("2957", StringComparison.OrdinalIgnoreCase)))) // S10-2
            {
                try
                {
                    using (var mSearcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BaseBoard"))
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
                                        DeviceHardware.SerialNumber = serialNumber.Trim();
                                        ErrorReporting.DeviceIdentifier = DeviceHardware.SerialNumber;
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
        }

        private static void ApplySystemProductInformation(this DeviceHardware DeviceHardware)
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
                                if (DeviceHardware.SerialNumber == null)
                                {
                                    var serialNumber = mItem.GetPropertyValue("IdentifyingNumber") as string;
                                    if (!string.IsNullOrWhiteSpace(serialNumber))
                                    {
                                        DeviceHardware.SerialNumber = serialNumber.Trim();
                                        ErrorReporting.DeviceIdentifier = DeviceHardware.SerialNumber;
                                    }
                                }

                                var uUID = (string)mItem.GetPropertyValue("UUID");
                                if (!string.IsNullOrWhiteSpace(uUID))
                                    DeviceHardware.UUID = uUID.Trim();
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
    }
}
