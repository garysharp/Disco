using Disco.Client.Interop.Native;
using Disco.Models.ClientServices.EnrolmentInformation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Disco.Client.Interop
{
    public static class WirelessNetwork
    {

        public static void ApplyWlanInformation(this List<NetworkAdapter> Adapters)
        {
            try
            {
                IntPtr wlanHandle;
                uint wlanServiceVersion;

                if (WlanApi.WlanOpenHandle(WlanApi.WLAN_API_VERSION_2_0, IntPtr.Zero, out wlanServiceVersion, out wlanHandle) == WlanApi.ERROR_SUCCESS)
                {
                    try
                    {
                        IntPtr wlanInterfacesPtr;

                        if (WlanApi.WlanEnumInterfaces(wlanHandle, IntPtr.Zero, out wlanInterfacesPtr) == WlanApi.ERROR_SUCCESS)
                        {
                            try
                            {
                                var wlanInterfaces = new WLAN_INTERFACE_INFO_LIST(wlanInterfacesPtr);

                                foreach (var wlanInterface in wlanInterfaces.InterfaceInfo)
                                {
                                    var adapter = Adapters.FirstOrDefault(a => a.ConnectionIdentifier == wlanInterface.InterfaceGuid);
                                    if (adapter != null)
                                    {
                                        adapter.IsWlanAdapter = true;
                                        adapter.WlanStatus = wlanInterface.isState.Description();
                                    }
                                }
                            }
                            finally
                            {
                                WlanApi.WlanFreeMemory(wlanInterfacesPtr);
                            }
                        }
                    }
                    finally
                    {
                        WlanApi.WlanCloseHandle(wlanHandle, IntPtr.Zero);
                    }
                }
            }
            catch (DllNotFoundException)
            {
                // Ignore
                // Indicates 'Wlanapi.dll' isn't present (ie. Servers)
            }
            catch (Exception ex)
            {
                throw new Exception("Disco Client was unable to retrieve Wireless NetworkAdapter information from WlanApi", ex);
            }
        }

        public static List<WirelessProfile> GetWirelessProfiles()
        {
            try
            {
                IntPtr wlanHandle;
                uint wlanServiceVersion;
                uint interopResult;

                // Connect to wireless service
                interopResult = WlanApi.WlanOpenHandle(WlanApi.WLAN_API_VERSION_2_0, IntPtr.Zero, out wlanServiceVersion, out wlanHandle);
                if (interopResult == WlanApi.ERROR_SERVICE_NOT_ACTIVE)
                {
                    // Indicates the Wlan service has not been started on the client
                    //  typically as it is not needed (no wireless adapter) or if it
                    //  has been forcibly disabled.
                    return null;
                }
                if (interopResult != WlanApi.ERROR_SUCCESS)
                {
                    throw new Exception($"Unable to connect to local wireless service. WlanOpenHandle returned: {interopResult}");
                }
                try
                {
                    return GetWirelessProfiles(wlanHandle);
                }
                finally
                {
                    WlanApi.WlanCloseHandle(wlanHandle, IntPtr.Zero);
                }
            }
            catch (DllNotFoundException)
            {
                // Indicates 'Wlanapi.dll' isn't present (ie. Servers)
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception("Disco Client was unable to retrieve Wireless Profiles from WlanApi", ex);
            }
        }

        private static List<WirelessProfile> GetWirelessProfiles(IntPtr wlanHandle)
        {
            uint interopResult;
            IntPtr wlanInterfacesPtr;

            // Enumerate wireless interfaces
            interopResult = WlanApi.WlanEnumInterfaces(wlanHandle, IntPtr.Zero, out wlanInterfacesPtr);
            if (interopResult != WlanApi.ERROR_SUCCESS)
            {
                throw new Exception($"Unable to list interfaces with the local wireless service. WlanEnumInterfaces returned: {interopResult}");
            }
            try
            {
                var wlanInterfaces = new WLAN_INTERFACE_INFO_LIST(wlanInterfacesPtr);
                var profiles = new List<WirelessProfile>();

                foreach (var wlanInterface in wlanInterfaces.InterfaceInfo)
                {
                    IntPtr wlanProfilesPtr;
                    // Enumerate wireless profiles for interface
                    interopResult = WlanApi.WlanGetProfileList(wlanHandle, wlanInterface.InterfaceGuid, IntPtr.Zero, out wlanProfilesPtr);
                    if (interopResult != WlanApi.ERROR_SUCCESS)
                    {
                        throw new Exception($"Unable to list wireless profiles for the {wlanInterface.InterfaceGuid} interface with the local wireless service. WlanGetProfileList returned: {interopResult}");
                    }
                    try
                    {
                        var wlanProfiles = new WLAN_PROFILE_INFO_LIST(wlanProfilesPtr);

                        foreach (var wlanProfile in wlanProfiles.ProfileInfo)
                        {
                            profiles.Add(new WirelessProfile()
                            {
                                Name = wlanProfile.strInterfaceDescription,
                                InterfaceGuid = wlanInterface.InterfaceGuid,
                                IsGroupPolicy = wlanProfile.dwFlags.HasFlag(ProfileInfoFlags.WLAN_PROFILE_GROUP_POLICY)
                            });
                        }
                    }
                    finally
                    {
                        WlanApi.WlanFreeMemory(wlanProfilesPtr);
                    }
                }

                return profiles;
            }
            finally
            {
                WlanApi.WlanFreeMemory(wlanInterfacesPtr);
            }
        }

        public static void Apply(this WirelessProfileStore ProfileStore)
        {
            var adapters = Network.GetNetworkAdapters().Where(a => a.IsWlanAdapter).ToList();

            try
            {
                IntPtr wlanHandle;
                uint wlanServiceVersion;
                uint interopResult;

                // Connect to wireless service
                interopResult = WlanApi.WlanOpenHandle(WlanApi.WLAN_API_VERSION_2_0, IntPtr.Zero, out wlanServiceVersion, out wlanHandle);
                if (interopResult == WlanApi.ERROR_SERVICE_NOT_ACTIVE)
                {
                    // Indicates the Wlan service has not been started on the client
                    //  typically as it is not needed (no wireless adapter) or if it
                    //  has been forcibly disabled.
                    Presentation.UpdateStatus("Enrolling Device", $"Configuring Wireless Profiles\r\nSkipping, the WLAN service is not enabled", true, -1, 3000);
                    return;
                }
                if (interopResult != WlanApi.ERROR_SUCCESS)
                {
                    throw new Exception($"Unable to connect to local wireless service. WlanOpenHandle returned: {interopResult}");
                }
                try
                {
                    var existingProfiles = GetWirelessProfiles(wlanHandle);
                    var addedProfiles = new List<string>();

                    // Remove Profiles
                    if (ProfileStore.RemoveNames != null && ProfileStore.RemoveNames.Count > 0)
                    {
                        var profileRemoved = false;

                        foreach (var removeName in ProfileStore.RemoveNames)
                        {
                            var foundProfiles = existingProfiles.Where(p => p.Name == removeName);
                            foreach (var profile in foundProfiles)
                            {
                                var adapter = adapters.FirstOrDefault(a => a.ConnectionIdentifier == profile.InterfaceGuid);

                                if (profile.IsGroupPolicy == true)
                                {
                                    Presentation.UpdateStatus("Enrolling Device", $"Configuring Wireless Profiles\r\nUnable to remove Group Policy Wireless Profile '{removeName}' from '{adapter?.NetConnectionID ?? profile.InterfaceGuid.ToString()}'", true, -1, 3000);
                                }
                                else
                                {
                                    Presentation.UpdateStatus("Enrolling Device", $"Configuring Wireless Profiles\r\nRemoving Wireless Profile '{removeName}' from '{adapter?.NetConnectionID ?? profile.InterfaceGuid.ToString()}'", true, -1, 1000);

                                    interopResult = WlanApi.WlanDeleteProfile(wlanHandle, profile.InterfaceGuid.Value, profile.Name, IntPtr.Zero);

                                    if (interopResult != WlanApi.ERROR_SUCCESS)
                                    {
                                        Presentation.UpdateStatus("Enrolling Device", $"Configuring Wireless Profiles\r\nFailed to remove Wireless Profile '{removeName}' from '{adapter?.NetConnectionID ?? profile.InterfaceGuid.ToString()}'; WlanDeleteProfile returned: {interopResult}", true, -1, 3000);
                                    }
                                    profileRemoved = true;
                                }
                            }
                        }

                        if (profileRemoved)
                        {
                            existingProfiles = GetWirelessProfiles(wlanHandle);
                        }
                    }

                    // Add Profiles
                    if (ProfileStore.Profiles != null && ProfileStore.Profiles.Count > 0)
                    {
                        foreach (var addProfile in ProfileStore.Profiles)
                        {
                            foreach (var adapter in adapters)
                            {
                                var existingProfile = existingProfiles.FirstOrDefault(p => p.Name == addProfile.Name && p.InterfaceGuid == adapter.ConnectionIdentifier);

                                if (addProfile.ForceDeployment.Value ||
                                    existingProfile == null)
                                {
                                    if (existingProfile != null && existingProfile.IsGroupPolicy.Value)
                                    {
                                        Presentation.UpdateStatus("Enrolling Device", $"Configuring Wireless Profiles\r\nSkipped Wireless Profile '{addProfile.Name}' on '{adapter.NetConnectionID}' as this profile is managed by Group Policy", true, -1, 3000);
                                    }
                                    else
                                    {
                                        uint pdwReasonCode;
                                        Presentation.UpdateStatus("Enrolling Device", $"Configuring Wireless Profiles\r\nAdding Wireless Profile '{addProfile.Name}' on '{adapter.NetConnectionID}'", true, -1, 1000);
                                        interopResult = WlanApi.WlanSetProfile(wlanHandle, adapter.ConnectionIdentifier, 0, addProfile.ProfileXml, null, true, IntPtr.Zero, out pdwReasonCode);

                                        if (interopResult != WlanApi.ERROR_SUCCESS)
                                        {
                                            // Get Reason Code
                                            var reason = new StringBuilder(256);
                                            WlanApi.WlanReasonCodeToString(pdwReasonCode, (uint)reason.Capacity, ref reason, IntPtr.Zero);

                                            Presentation.UpdateStatus("Enrolling Device", $"Configuring Wireless Profiles\r\nFailed to add Wireless Profile '{addProfile.Name}' on '{adapter.NetConnectionID}'; WlanSetProfile returned: {interopResult}; {reason.ToString()}", true, -1, 3000);
                                        }
                                    }
                                    addedProfiles.Add(addProfile.Name);
                                }
                            }
                        }
                    }

                    // Transform Profiles
                    if (ProfileStore.Transformations != null && ProfileStore.Transformations.Count > 0)
                    {
                        foreach (var transformGroup in ProfileStore.Transformations.GroupBy(t => t.Name))
                        {
                            var profileName = transformGroup.Key;

                            // Don't transform if just added
                            if (!addedProfiles.Contains(transformGroup.Key))
                            {
                                foreach (var adapter in adapters)
                                {
                                    var existingProfile = existingProfiles.FirstOrDefault(p => p.Name == profileName && p.InterfaceGuid == adapter.ConnectionIdentifier);

                                    if (existingProfile != null)
                                    {
                                        if (existingProfile.IsGroupPolicy.Value)
                                        {
                                            Presentation.UpdateStatus("Enrolling Device", $"Configuring Wireless Profiles\r\nSkipped Wireless Profile '{profileName}' on '{adapter.NetConnectionID}' as this profile is managed by Group Policy", true, -1, 3000);
                                        }
                                        else
                                        {
                                            // Load profile
                                            IntPtr pstrProfileXml;
                                            uint pdwFlags;
                                            IntPtr pdwGrantAccess;

                                            interopResult = WlanApi.WlanGetProfile(wlanHandle, adapter.ConnectionIdentifier, profileName, IntPtr.Zero, out pstrProfileXml, out pdwFlags, out pdwGrantAccess);

                                            if (interopResult == WlanApi.ERROR_SUCCESS)
                                            {
                                                try
                                                {
                                                    var profileXml = Marshal.PtrToStringUni(pstrProfileXml);
                                                    var originalProfileXml = XElement.Parse(profileXml);
                                                    var transformProfileXml = originalProfileXml.ToString(SaveOptions.DisableFormatting);

                                                    // Apply Transforms
                                                    foreach (var transform in transformGroup)
                                                    {
                                                        var regex = new Regex(transform.RegularExpression, RegexOptions.Singleline);
                                                        transformProfileXml = regex.Replace(transformProfileXml, transform.RegularExpressionReplacement);
                                                    }

                                                    // Compare XML
                                                    var transformedProfileXml = XElement.Parse(transformProfileXml);

                                                    if (!XNode.DeepEquals(originalProfileXml, transformedProfileXml))
                                                    {
                                                        // Set Profile
                                                        uint pdwReasonCode;
                                                        Presentation.UpdateStatus("Enrolling Device", $"Configuring Wireless Profiles\r\nModifying Wireless Profile '{profileName}' on '{adapter.NetConnectionID}'", true, -1, 1000);
                                                        transformProfileXml = transformedProfileXml.ToString(SaveOptions.None);
                                                        interopResult = WlanApi.WlanSetProfile(wlanHandle, adapter.ConnectionIdentifier, 0, transformProfileXml, null, true, IntPtr.Zero, out pdwReasonCode);

                                                        if (interopResult != WlanApi.ERROR_SUCCESS)
                                                        {
                                                            // Get Reason Code
                                                            var reason = new StringBuilder(256);
                                                            WlanApi.WlanReasonCodeToString(pdwReasonCode, (uint)reason.Capacity, ref reason, IntPtr.Zero);

                                                            Presentation.UpdateStatus("Enrolling Device", $"Configuring Wireless Profiles\r\nFailed to modify Wireless Profile '{profileName}' to '{adapter.NetConnectionID}'; WlanSetProfile returned: {interopResult}; {reason.ToString()}", true, -1, 3000);
                                                        }
                                                    }
                                                }
                                                finally
                                                {
                                                    WlanApi.WlanFreeMemory(pstrProfileXml);
                                                }
                                            }
                                            else
                                            {
                                                Presentation.UpdateStatus("Enrolling Device", $"Configuring Wireless Profiles\r\nFailed to transform Wireless Profile '{profileName}' on '{adapter.NetConnectionID}'; WlanGetProfile returned: {interopResult}", true, -1, 3000);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                }
                finally
                {
                    WlanApi.WlanCloseHandle(wlanHandle, IntPtr.Zero);
                }
            }
            catch (DllNotFoundException)
            {
                // Indicates 'Wlanapi.dll' isn't present (ie. Servers)
                // Ignore policies
            }
            catch (Exception ex)
            {
                throw new Exception("Disco Client was unable to apply Wireless Profile Changes using WlanApi", ex);
            }
        }

        public static string Description(this WLAN_INTERFACE_STATE State)
        {
            switch (State)
            {
                case WLAN_INTERFACE_STATE.wlan_interface_state_not_ready:
                    return "Not Ready";
                case WLAN_INTERFACE_STATE.wlan_interface_state_connected:
                    return "Connected";
                case WLAN_INTERFACE_STATE.wlan_interface_state_ad_hoc_network_formed:
                    return "Ad Hoc Network";
                case WLAN_INTERFACE_STATE.wlan_interface_state_disconnecting:
                    return "Disconnecting";
                case WLAN_INTERFACE_STATE.wlan_interface_state_disconnected:
                    return "Disconnected";
                case WLAN_INTERFACE_STATE.wlan_interface_state_associating:
                    return "Associating";
                case WLAN_INTERFACE_STATE.wlan_interface_state_discovering:
                    return "Discovering";
                case WLAN_INTERFACE_STATE.wlan_interface_state_authenticating:
                    return "Authenticating";
                default:
                    return "Unknown";
            }
        }
    }
}
