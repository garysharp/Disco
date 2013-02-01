using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;

namespace Disco.Client.Interop
{
    public static class Network
    {
        private static List<NetworkAdapterInfo> NetworkAdapters { get; set; }
        private static NetworkAdapterInfo PrimaryLanNetworkAdapter { get; set; }
        private static NetworkAdapterInfo PrimaryWlanNetworkAdapter { get; set; }

        static Network()
        {
            // Get All Adapters
            RetrieveLanAdapters();

            if (NetworkAdapters.Count > 0)
            {
                // Only Retrieve Wlan Adapters if at least one adapter was found by WMI
                RetrieveWlanAdapters();

                // Determine Primary Adapters

                // Lan
                PrimaryLanNetworkAdapter = NetworkAdapters.Where(n => !n.IsWLanAdapter && n.NetConnectionId.StartsWith("Local Area Connection", StringComparison.InvariantCultureIgnoreCase)).OrderByDescending(n => n.Speed).FirstOrDefault();
                // Might be too restrictive - remove name restriction just in case.
                if (PrimaryLanNetworkAdapter == null)
                    PrimaryLanNetworkAdapter = NetworkAdapters.Where(n => !n.IsWLanAdapter).OrderByDescending(n => n.Speed).FirstOrDefault();

                // Wan
                PrimaryWlanNetworkAdapter = NetworkAdapters.Where(n => n.IsWLanAdapter).OrderByDescending(n => n.Speed).FirstOrDefault();
            }
        }

        private static void RetrieveLanAdapters()
        {
            // Get NetworkAdapter Information
            try
            {
                using (ManagementObjectSearcher mSearcher = new ManagementObjectSearcher("SELECT Index, GUID, MACAddress, Name, NetConnectionID, Speed FROM Win32_NetworkAdapter WHERE PhysicalAdapter=true AND MACAddress IS NOT NULL AND Name IS NOT NULL AND NetConnectionID IS NOT NULL AND Speed IS NOT NULL"))
                {
                    using (ManagementObjectCollection mResults = mSearcher.Get())
                    {
                        NetworkAdapters = new List<NetworkAdapterInfo>();
                        foreach (var mResult in mResults.Cast<ManagementObject>())
                        {
                            NetworkAdapterInfo nic = new NetworkAdapterInfo()
                            {
                                Index = (UInt32)mResult.GetPropertyValue("Index"),
                                Guid = Guid.Parse((string)mResult.GetPropertyValue("GUID")),
                                MacAddress = mResult.GetPropertyValue("MACAddress").ToString(),
                                Name = mResult.GetPropertyValue("Name").ToString(),
                                NetConnectionId = mResult.GetPropertyValue("NetConnectionID").ToString(),
                                Speed = Convert.ToUInt64(mResult.GetPropertyValue("Speed")),
                                IsWLanAdapter = false
                            };
                            NetworkAdapters.Add(nic);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Disco Client was unable to retrieve NetworkAdapter information from WMI", ex);
            }
        }

        private static void RetrieveWlanAdapters()
        {
            WLAN_INTERFACE_INFO_LIST wlanApiInterfaceList;

            IntPtr wlanApiHandle = IntPtr.Zero;
            uint wlanApiServiceVersion = 0;

            if (WlanOpenHandle(WLAN_API_VERSION_2_0, IntPtr.Zero, out wlanApiServiceVersion, ref wlanApiHandle) == ERROR_SUCCESS)
            {
                IntPtr wlanApiInterfaceListPointer = IntPtr.Zero;

                if (WlanEnumInterfaces(wlanApiHandle, IntPtr.Zero, ref wlanApiInterfaceListPointer) == ERROR_SUCCESS)
                {
                    wlanApiInterfaceList = new WLAN_INTERFACE_INFO_LIST(wlanApiInterfaceListPointer);
                    WlanFreeMemory(wlanApiInterfaceListPointer);
                }
                else
                {
                    // Error - No Wlan Adapters Reported
                    WlanCloseHandle(wlanApiHandle, IntPtr.Zero);
                    return;
                }

                WlanCloseHandle(wlanApiHandle, IntPtr.Zero);
            }
            else
            {
                // Error - No Wlan Adapters Reported
                return;
            }

            if (wlanApiInterfaceList.InterfaceInfo != null)
            {
                foreach (var wlanApiAdapter in wlanApiInterfaceList.InterfaceInfo)
                {
                    var wlanApiAdapterInfo = wlanApiAdapter;
                    var wmiAdapterInfo = NetworkAdapters.FirstOrDefault(n => n.Guid == wlanApiAdapterInfo.InterfaceGuid);
                    if (wmiAdapterInfo != null)
                    {
                        wmiAdapterInfo.IsWLanAdapter = true;
                        wmiAdapterInfo.WlanState = wlanApiAdapterInfo.isState;
                    }
                }
            }
        }

        public static string PrimaryLanMacAddress
        {
            get
            {
                // Return null if no Primary LAN Network Adapter found on this Device

                return (PrimaryLanNetworkAdapter == null) ? null : PrimaryLanNetworkAdapter.MacAddress;
            }
        }
        public static string PrimaryWlanMacAddress
        {
            get
            {
                // Return null if no Primary WLAN Network Adapter found on this Device

                return (PrimaryWlanNetworkAdapter == null) ? null : PrimaryWlanNetworkAdapter.MacAddress;
            }
        }

        private class NetworkAdapterInfo
        {
            public UInt32 Index { get; set; }
            public Guid Guid { get; set; }
            public string Name { get; set; }
            public string NetConnectionId { get; set; }
            public string MacAddress { get; set; }
            public UInt64 Speed { get; set; }

            public bool IsWLanAdapter { get; set; }
            public WLAN_INTERFACE_STATE WlanState { get; set; }

            public string WlanStateDescription
            {
                get
                {
                    switch (WlanState)
                    {
                        case WLAN_INTERFACE_STATE.wlan_interface_state_not_ready:
                            return "Not Ready";
                        case WLAN_INTERFACE_STATE.wlan_interface_state_connected:
                            return "Connected";
                        case WLAN_INTERFACE_STATE.wlan_interface_state_ad_hoc_network_formed:
                            return "Ad Hoc Network Formed";
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

        #region Wlan Win32 Interop

        private const uint WLAN_API_VERSION_2_0 = 2; // Windows Vista WiFi API Version
        private const int ERROR_SUCCESS = 0;

        /// <summary >
        /// Opens a connection to the server
        /// </summary >
        [DllImport("Wlanapi.dll")]
        private static extern int WlanOpenHandle(
            uint dwClientVersion,
            IntPtr pReserved, //not in MSDN but required
            [Out] out uint pdwNegotiatedVersion,
            ref IntPtr ClientHandle);

        /// <summary >
        /// Closes a connection to the server
        /// </summary >
        [DllImport("Wlanapi", EntryPoint = "WlanCloseHandle")]
        private static extern uint WlanCloseHandle(
        [In] IntPtr hClientHandle,
        IntPtr pReserved);

        /// <summary >
        /// Enumerates all wireless interfaces in the laptop
        /// </summary >
        [DllImport("Wlanapi", EntryPoint = "WlanEnumInterfaces")]
        private static extern uint WlanEnumInterfaces(
        [In] IntPtr hClientHandle,
        IntPtr pReserved,
        ref IntPtr ppInterfaceList);

        /// <summary >
        /// Frees memory returned by native WiFi functions
        /// </summary >
        [DllImport("Wlanapi", EntryPoint = "WlanFreeMemory")]
        private static extern void WlanFreeMemory([In] IntPtr pMemory);

        /// <summary>
        /// Defines the state of the interface. e.g. connected, disconnected.
        /// </summary>
        private enum WLAN_INTERFACE_STATE
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


        /// <summary >
        /// Stores interface info
        /// </summary >
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct WLAN_INTERFACE_INFO
        {
            /// GUID->_GUID
            public Guid InterfaceGuid;

            /// WCHAR[256]
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string strInterfaceDescription;

            /// WLAN_INTERFACE_STATE->_WLAN_INTERFACE_STATE
            public WLAN_INTERFACE_STATE isState;
        }

        /// <summary>
        /// Contains an array of NIC information
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct WLAN_INTERFACE_INFO_LIST
        {
            /// <summary>
            /// Length of <see cref="InterfaceInfo"/> array
            /// </summary>
            public Int32 dwNumberOfItems;
            /// <summary>
            /// This member is not used by the wireless service. Applications can use this member when processing individual interfaces.
            /// </summary>
            public Int32 dwIndex;
            /// <summary>
            /// Array of WLAN interfaces.
            /// </summary>
            public WLAN_INTERFACE_INFO[] InterfaceInfo;

            /// <summary>
            /// Constructor for WLAN_INTERFACE_INFO_LIST.
            /// Constructor is needed because the InterfaceInfo member varies based on how many adapters are in the system.
            /// </summary>
            /// <param name="pList">the unmanaged pointer containing the list.</param>
            public WLAN_INTERFACE_INFO_LIST(IntPtr pList)
            {
                // The first 4 bytes are the number of WLAN_INTERFACE_INFO structures.
                dwNumberOfItems = Marshal.ReadInt32(pList, 0);

                // The next 4 bytes are the index of the current item in the unmanaged API.
                dwIndex = Marshal.ReadInt32(pList, 4);

                // Construct the array of WLAN_INTERFACE_INFO structures.
                InterfaceInfo = new WLAN_INTERFACE_INFO[dwNumberOfItems];

                for (int i = 0; i <= dwNumberOfItems - 1; i++)
                {
                    // The offset of the array of structures is 8 bytes past the beginning.
                    // Then, take the index and multiply it by the number of bytes in the
                    // structure.
                    // The length of the WLAN_INTERFACE_INFO structure is 532 bytes - this
                    // was determined by doing a Marshall.SizeOf(WLAN_INTERFACE_INFO) 
                    IntPtr pItemList = new IntPtr(pList.ToInt64() + (i * 532) + 8);

                    // Construct the WLAN_INTERFACE_INFO structure, marshal the unmanaged
                    // structure into it, then copy it to the array of structures.
                    InterfaceInfo[i] = (WLAN_INTERFACE_INFO)Marshal.PtrToStructure(pItemList, typeof(WLAN_INTERFACE_INFO));
                }
            }
        }

        #endregion

    }
}
