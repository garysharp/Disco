using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Management;
using System.Runtime.InteropServices;
using System.Xml;

namespace Disco.ClientBootstrapper.Interop
{
    static class NetworkInterop
    {

        #region PInvoke

        [DllImport("Wlanapi", EntryPoint = "WlanOpenHandle")]
        public static extern uint WlanOpenHandle(uint dwClientVersion, IntPtr pReserved, [Out] out uint pdwNegotiatedVersion, ref IntPtr ClientHandle);
        [DllImport("Wlanapi", EntryPoint = "WlanCloseHandle")]
        public static extern uint WlanCloseHandle([In] IntPtr hClientHandle, IntPtr pReserved);
        [DllImport("Wlanapi", EntryPoint = "WlanFreeMemory")]
        public static extern void WlanFreeMemory([In] IntPtr pMemory);

        [DllImport("Wlanapi.dll", SetLastError = true)]
        public static extern uint WlanGetProfileList(IntPtr hClientHandle, ref Guid pInterfaceGuid, IntPtr pReserved, ref IntPtr ppProfileList);
        [DllImport("Wlanapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern uint WlanSetProfile(IntPtr hClientHandle, [In] ref Guid pInterfaceGuid, uint dwFlags, string strProfileXml, string strAllUserProfileSecurity, bool bOverwrite, IntPtr pReserved, out uint pdwReasonCode);


        [DllImport("Wlanapi", EntryPoint = "WlanQueryInterface")]
        public static extern uint WlanQueryInterface([In] IntPtr hClientHandle,
            [In] ref Guid pInterfaceGuid,
            WLAN_INTF_OPCODE OpCode,
            IntPtr pReserved,
            [Out] out uint pdwDataSize,
            ref IntPtr ppData,
            IntPtr pWlanOpcodeValueType);

        public enum WLAN_INTF_OPCODE
        {
            /// wlan_intf_opcode_autoconf_start -> 0x000000000
            wlan_intf_opcode_autoconf_start = 0,
            wlan_intf_opcode_autoconf_enabled,
            wlan_intf_opcode_background_scan_enabled,
            wlan_intf_opcode_media_streaming_mode,
            wlan_intf_opcode_radio_state,
            wlan_intf_opcode_bss_type,
            wlan_intf_opcode_interface_state,
            wlan_intf_opcode_current_connection,
            wlan_intf_opcode_channel_number,
            wlan_intf_opcode_supported_infrastructure_auth_cipher_pairs,
            wlan_intf_opcode_supported_adhoc_auth_cipher_pairs,
            wlan_intf_opcode_supported_country_or_region_string_list,
            wlan_intf_opcode_current_operation_mode,
            wlan_intf_opcode_supported_safe_mode,
            wlan_intf_opcode_certified_safe_mode,
            /// wlan_intf_opcode_autoconf_end -> 0x0fffffff
            wlan_intf_opcode_autoconf_end = 268435455,
            /// wlan_intf_opcode_msm_start -> 0x10000100
            wlan_intf_opcode_msm_start = 268435712,
            wlan_intf_opcode_statistics,
            wlan_intf_opcode_rssi,
            /// wlan_intf_opcode_msm_end -> 0x1fffffff
            wlan_intf_opcode_msm_end = 536870911,
            /// wlan_intf_opcode_security_start -> 0x20010000
            wlan_intf_opcode_security_start = 536936448,
            /// wlan_intf_opcode_security_end -> 0x2fffffff
            wlan_intf_opcode_security_end = 805306367,
            /// wlan_intf_opcode_ihv_start -> 0x30000000
            wlan_intf_opcode_ihv_start = 805306368,
            /// wlan_intf_opcode_ihv_end -> 0x3fffffff
            wlan_intf_opcode_ihv_end = 1073741823,
        }

        /// <summary>
        /// Defines the state of the interface. e.g. connected, disconnected.
        /// </summary>
        public enum WLAN_INTERFACE_STATE
        {
            /// <summary>
            /// wlan_interface_state_not_ready -> 0
            /// </summary>
            wlan_interface_state_not_ready = 0,
            /// <summary>
            /// wlan_interface_state_connected -> 1
            /// </summary>
            wlan_interface_state_connected = 1,
            /// <summary>
            /// wlan_interface_state_ad_hoc_network_formed -> 2
            /// </summary>
            wlan_interface_state_ad_hoc_network_formed = 2,
            /// <summary>
            /// wlan_interface_state_disconnecting -> 3
            /// </summary>
            wlan_interface_state_disconnecting = 3,
            /// <summary>
            /// wlan_interface_state_disconnected -> 4
            /// </summary>
            wlan_interface_state_disconnected = 4,
            /// <summary>
            /// wlan_interface_state_associating -> 5
            /// </summary>
            wlan_interface_state_associating = 5,
            /// <summary>
            /// wlan_interface_state_discovering -> 6
            /// </summary>
            wlan_interface_state_discovering = 6,
            /// <summary>
            /// wlan_interface_state_authenticating -> 7
            /// </summary>
            wlan_interface_state_authenticating = 7,
        }

        public struct WLAN_PROFILE_INFO_LIST
        {
            public uint dwNumberOfItems;
            public uint dwIndex;
            public WLAN_PROFILE_INFO[] ProfileInfo;

            public WLAN_PROFILE_INFO_LIST(IntPtr ppProfileList)
            {
                dwNumberOfItems = (uint)Marshal.ReadInt32(ppProfileList);
                dwIndex = (uint)Marshal.ReadInt32(ppProfileList, 4);
                ProfileInfo = new WLAN_PROFILE_INFO[dwNumberOfItems];
                IntPtr ppProfileListTemp = new IntPtr(ppProfileList.ToInt32() + 8);

                for (int i = 0; i < dwNumberOfItems; i++)
                {
                    ppProfileList = new IntPtr(ppProfileListTemp.ToInt32() + i * Marshal.SizeOf(typeof(WLAN_PROFILE_INFO)));
                    ProfileInfo[i] = (WLAN_PROFILE_INFO)Marshal.PtrToStructure(ppProfileList, typeof(WLAN_PROFILE_INFO));
                }
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct WLAN_PROFILE_INFO
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string strProfileName;
            public uint dwFlags;
        }

        #endregion

        private static List<NetworkAdapter> _networkAdapters;
        public static List<NetworkAdapter> NetworkAdapters
        {
            get
            {
                if (_networkAdapters == null)
                {
                    using (var mSearcher = new ManagementObjectSearcher("SELECT __PATH, Index, GUID, MACAddress, Name, NetConnectionID, Speed FROM Win32_NetworkAdapter WHERE PhysicalAdapter=true AND MACAddress IS NOT NULL AND Name IS NOT NULL AND NetConnectionID IS NOT NULL AND Speed IS NOT NULL"))
                    {

                        var mResults = mSearcher.Get();
                        _networkAdapters = new List<NetworkAdapter>(mResults.Count);

                        foreach (ManagementObject mResult in mResults)
                        {
                            _networkAdapters.Add(new NetworkAdapter(mResult));
                        }
                    }
                }
                return _networkAdapters;
            }
        }

        public static bool PingDiscoIct(string ServerName)
        {
            using (Ping p = new Ping())
            {
                try
                {
                    PingReply pr = p.Send(ServerName, 2000);
                    if (pr.Status == IPStatus.Success)
                        return true;
                    else
                        return false;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public static void ConfigureWireless()
        {
            // Add Certificates
            Program.Status.UpdateStatus(null, null, "Configuring Wireless Certificates");
            CertificateInterop.AddTempCerts();

            // Add Wireless Profiles
            Program.Status.UpdateStatus(null, null, "Configuring Wireless Profiles");
            var wirelessInlineProfiles = GetInlineWirelessProfiles();
            if (wirelessInlineProfiles.Count > 0)
            {

                IntPtr wlanHandle = IntPtr.Zero;
                uint negotiatedVersion;

                try
                {
                    if (WlanOpenHandle(1, IntPtr.Zero, out negotiatedVersion, ref wlanHandle) != 0)
                        throw new NotSupportedException("This device does not support Wireless");

                    // Add Profile to Each Wireless Adapter
                    var wirelessAdapters = NetworkAdapters.Where(na => na.IsWireless).ToList();
                    foreach (var na in wirelessAdapters)
                    {
                        foreach (var inlineWirelessProfile in wirelessInlineProfiles)
                        {
                            if (inlineWirelessProfile.AddProfile(wlanHandle, na.Guid))
                            {
                                Program.Status.UpdateStatus(null, null, $"Added Wireless Profile: {inlineWirelessProfile.ProfileName}");
                                Program.SleepThread(500, false);
                            }
                            else
                            {
                                Program.Status.UpdateStatus(null, null, $"Unable to add Wireless Profile: {inlineWirelessProfile.ProfileName}");
                                Program.SleepThread(5000, false);
                            }
                        }
                    }
                }
                finally
                {
                    if (wlanHandle != IntPtr.Zero)
                        NetworkInterop.WlanCloseHandle(wlanHandle, IntPtr.Zero);
                }
            }

        }
        private class WirelessProfile
        {
            public string Filename { get; set; }
            public string ProfileXml { get; set; }
            public string ProfileName { get; set; }

            public bool AddProfile(IntPtr WlanHandle, Guid interfaceGuid)
            {
                var pInterfaceGuid = interfaceGuid;
                var pProfileXml = ProfileXml;
                uint pFlag = 0;
                uint failReason;
                return (WlanSetProfile(WlanHandle, ref pInterfaceGuid, pFlag, pProfileXml, null, true, IntPtr.Zero, out failReason) == 0);
            }
        }

        private static List<WirelessProfile> GetInlineWirelessProfiles()
        {
            var inlineProfileFiles = System.IO.Directory.EnumerateFiles(Program.InlinePath.Value, "WLAN_Profile_*.xml").ToList();
            var inlineProfiles = new List<WirelessProfile>(inlineProfileFiles.Count);
            foreach (var filename in inlineProfileFiles)
            {
                var profile = new WirelessProfile()
                {
                    Filename = filename,
                    ProfileXml = System.IO.File.ReadAllText(filename)
                };
                var profileXml = new XmlDocument();
                profileXml.LoadXml(profile.ProfileXml);
                var profileXmlNS = new XmlNamespaceManager(profileXml.NameTable);
                profileXmlNS.AddNamespace("p", "http://www.microsoft.com/networking/WLAN/profile/v1");
                var profileXmlNameNode = profileXml.SelectSingleNode("/p:WLANProfile/p:name", profileXmlNS);
                if (profileXmlNameNode != null)
                {
                    profile.ProfileName = profileXmlNameNode.InnerText;
                    inlineProfiles.Add(profile);
                }
            }
            return inlineProfiles;
        }

        private static WLAN_PROFILE_INFO_LIST GetWirelessProfiles(IntPtr WlanHandle, Guid interfaceGuid)
        {
            Guid pInterfaceGuid = interfaceGuid;

            IntPtr ppProfileList = new IntPtr();
            WlanGetProfileList(WlanHandle, ref pInterfaceGuid, new IntPtr(), ref ppProfileList);
            WLAN_PROFILE_INFO_LIST wlanProfileInfoList = new WLAN_PROFILE_INFO_LIST(ppProfileList);

            NetworkInterop.WlanFreeMemory(ppProfileList);

            return wlanProfileInfoList;
        }

    }
}
