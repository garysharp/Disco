using Disco.Client.Interop.Native;
using Disco.Models.ClientServices.EnrolmentInformation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;

namespace Disco.Client.Interop
{
    public static class Network
    {
        public static List<NetworkAdapter> GetNetworkAdapters()
        {
            var adapters = GetWmiNetworkAdapters();

            if (adapters != null && adapters.Count > 0)
            {
                // Apply Wlan Information
                adapters.ApplyWlanInformation();
            }

            return adapters;
        }
        
        private static List<NetworkAdapter> GetWmiNetworkAdapters()
        {
            try
            {
                // Load Physical Adapters
                using (var wmiSearcher = new ManagementObjectSearcher("SELECT DeviceID, GUID, Manufacturer, ProductName, AdapterType, MACAddress, Speed, NetConnectionID, NetConnectionStatus, NetEnabled FROM Win32_NetworkAdapter WHERE PhysicalAdapter=true AND MACAddress IS NOT NULL AND NetConnectionID IS NOT NULL AND Speed IS NOT NULL"))
                {
                    using (var wmiResults = wmiSearcher.Get())
                    {
                        return wmiResults
                            .Cast<ManagementObject>()
                            .Select(wmiResult =>
                            {
                                var adapter = new NetworkAdapter()
                                {
                                    DeviceID = (string)wmiResult.GetPropertyValue("DeviceID"),
                                    ConnectionIdentifier = Guid.Parse((string)wmiResult.GetPropertyValue("GUID")),
                                    Manufacturer = (string)wmiResult.GetPropertyValue("Manufacturer"),
                                    ProductName = (string)wmiResult.GetPropertyValue("ProductName"),
                                    AdapterType = (string)wmiResult.GetPropertyValue("AdapterType"),
                                    MACAddress = (string)wmiResult.GetPropertyValue("MACAddress"),
                                    Speed = (ulong)wmiResult.GetPropertyValue("Speed"),
                                    NetConnectionID = (string)wmiResult.GetPropertyValue("NetConnectionID"),
                                    NetConnectionStatus = ((NetworkConnectionStatuses)wmiResult.GetPropertyValue("NetConnectionStatus")).Description(),
                                    NetEnabled = (bool)wmiResult.GetPropertyValue("NetEnabled")
                                };

                                using (var wmiRelatedResults = wmiResult.GetRelated("Win32_NetworkAdapterConfiguration", "Win32_NetworkAdapterSetting", null, null, null, null, false, null))
                                {
                                    var wmiConfiguration = wmiRelatedResults.Cast<ManagementObject>().First();

                                    adapter.IPEnabled = (bool)wmiConfiguration.GetPropertyValue("IPEnabled");
                                    if (adapter.IPEnabled)
                                    {
                                        adapter.IPAddresses = ((string[])wmiConfiguration.GetPropertyValue("IPAddress")).ToList();
                                    }
                                }

                                return adapter;
                            })
                            .ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Disco Client was unable to retrieve NetworkAdapter information from WMI", ex);
            }
        }

        public static string Description(this NetworkConnectionStatuses Status)
        {
            switch (Status)
            {
                case NetworkConnectionStatuses.Disconnected:
                    return "Disconnected";
                case NetworkConnectionStatuses.Connecting:
                    return "Connecting";
                case NetworkConnectionStatuses.Connected:
                    return "Connected";
                case NetworkConnectionStatuses.Disconnecting:
                    return "Disconnecting";
                case NetworkConnectionStatuses.HardwareNotPresent:
                    return "Hardware Not Present";
                case NetworkConnectionStatuses.HardwareDisabled:
                    return "Hardware Disabled";
                case NetworkConnectionStatuses.HardwareMalfunction:
                    return "Hardware Malfunction";
                case NetworkConnectionStatuses.MediaDisconnected:
                    return "Media Disconnected";
                case NetworkConnectionStatuses.Authenticating:
                    return "Authenticating";
                case NetworkConnectionStatuses.AuthenticationSucceeded:
                    return "Authentication Succeeded";
                case NetworkConnectionStatuses.AuthenticationFailed:
                    return "Authentication Failed";
                case NetworkConnectionStatuses.InvalidAddress:
                    return "Invalid Address";
                case NetworkConnectionStatuses.CredentialsRequired:
                    return "Credentials Required";
                default:
                    return "Unknown";
            }
        }

    }
}
