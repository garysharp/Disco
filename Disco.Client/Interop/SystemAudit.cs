using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Client.Interop
{
    public static class SystemAudit
    {

        public static string DeviceSerialNumber { get; private set; }
        public static string DeviceSMBIOSVersion { get; private set; }
        public static string DeviceManufacturer { get; private set; }
        public static string DeviceModel { get; private set; }
        public static string DeviceType { get; private set; }
        public static string DeviceUUID { get; private set; }
        public static bool DeviceIsPartOfDomain { get; private set; }

        static SystemAudit()
        {
            // Get BIOS Information
            try
            {
                using (ManagementObjectSearcher mSearcher = new ManagementObjectSearcher("SELECT SerialNumber, SMBIOSBIOSVersion FROM Win32_BIOS WHERE PrimaryBios=true"))
                {
                    using (ManagementObjectCollection mResults = mSearcher.Get())
                    {
                        using (var mItem = mResults.Cast<ManagementObject>().FirstOrDefault())
                        {
                            if (mItem != null)
                            {
                                DeviceSerialNumber = mItem.GetPropertyValue("SerialNumber").ToString().Trim();
                                ErrorReporting.DeviceIdentifier = DeviceSerialNumber;
                                DeviceSMBIOSVersion = mItem.GetPropertyValue("SMBIOSBIOSVersion").ToString().Trim();
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

            // Get System Information
            try
            {
                using (ManagementObjectSearcher mSearcher = new ManagementObjectSearcher("SELECT Manufacturer, Model, PartOfDomain, PCSystemType FROM Win32_ComputerSystem"))
                {
                    using (ManagementObjectCollection mResults = mSearcher.Get())
                    {
                        using (var mItem = mResults.Cast<ManagementObject>().FirstOrDefault())
                        {
                            if (mItem != null)
                            {
                                DeviceManufacturer = mItem.GetPropertyValue("Manufacturer").ToString().Trim();
                                DeviceModel = mItem.GetPropertyValue("Model").ToString().Trim();
                                DeviceIsPartOfDomain = bool.Parse(mItem.GetPropertyValue("PartOfDomain").ToString());
                                DeviceType = PCSystemTypeToString((UInt16)mItem.GetPropertyValue("PCSystemType"));
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

            // Get System Product Information
            string ComputerSystemProductSerialNumber;
            try
            {
                using (ManagementObjectSearcher mSearcher = new ManagementObjectSearcher("SELECT IdentifyingNumber, UUID FROM Win32_ComputerSystemProduct"))
                {
                    using (ManagementObjectCollection mResults = mSearcher.Get())
                    {
                        using (var mItem = mResults.Cast<ManagementObject>().FirstOrDefault())
                        {
                            if (mItem != null)
                            {
                                ComputerSystemProductSerialNumber = mItem.GetPropertyValue("IdentifyingNumber").ToString().Trim();
                                DeviceUUID = mItem.GetPropertyValue("UUID").ToString().Trim();
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

            // Added 2012-11-22 G# - Lenovo IdeaPad Serial SHIM
            // http://www.discoict.com.au/forum/feature-requests/2012/11/serial-number-detection-on-ideapads.aspx
            if (string.IsNullOrWhiteSpace(DeviceSerialNumber) ||
                (DeviceManufacturer.Equals("LENOVO", StringComparison.InvariantCultureIgnoreCase) &&
                (DeviceModel.Equals("S10-3", StringComparison.InvariantCultureIgnoreCase) // S10-3
                || DeviceModel.Equals("2957", StringComparison.InvariantCultureIgnoreCase)))) // S10-2
            {
                try
                {
                    using (ManagementObjectSearcher mSearcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BaseBoard"))
                    {
                        using (ManagementObjectCollection mResults = mSearcher.Get())
                        {
                            using (var mItem = mResults.Cast<ManagementObject>().FirstOrDefault())
                            {
                                if (mItem != null)
                                {
                                    DeviceSerialNumber = mItem.GetPropertyValue("SerialNumber").ToString().Trim();
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
                if (string.IsNullOrWhiteSpace(DeviceSerialNumber))
                    DeviceSerialNumber = ComputerSystemProductSerialNumber;
            }
            // End Added 2012-11-22 G#

            ErrorReporting.DeviceIdentifier = DeviceSerialNumber;

            // Validate Device 'State'
            if (string.IsNullOrWhiteSpace(DeviceSerialNumber))
                throw new Exception("This device has no serial number stored in BIOS or BaseBoard");
            if (DeviceSerialNumber.Length > 60)
                throw new Exception(string.Format("The serial number reported by this device is over 60 characters long:{0}{1}", Environment.NewLine, DeviceSerialNumber));
        }

        private static string PCSystemTypeToString(UInt16 PCSystemType)
        {
            switch (PCSystemType)
            {
                case 0:
                    return "Unknown";
                case 1:
                    return "Desktop";
                case 2:
                    return "Mobile";
                case 3:
                    return "Workstation";
                case 4:
                    return "EnterpriseServer";
                case 5:
                    return "SmallOfficeAndHomeOfficeServer";
                case 6:
                    return "AppliancePC";
                case 7:
                    return "PerformanceServer";
                case 8:
                    return "Maximum";
                default:
                    return "Unknown";
            }
        }
    }
}
