using System;
using System.Management;
using System.Runtime.InteropServices;

namespace Disco.ClientBootstrapper.Interop
{

    class NetworkAdapter
    {

        public uint Index { get; set; }
        public string WmiPath { get; set; }
        public Guid Guid { get; set; }
        public string Name { get; set; }
        public string NetConnectionID { get; set; }
        public string MACAddress { get; set; }
        public UInt64 Speed { get; set; }
        public UInt16 LastConnectionStatus { get; set; }
        public bool IsWireless { get; set; }
        public string WirelessInterfaceDescription { get; set; }
        public int LastWirelessConnectionStatus { get; set; }

        public NetworkAdapter(ManagementObject wmiObject)
        {
            UpdateFromWmi(wmiObject);
        }

        private void UpdateFromWmi(ManagementObject wmiObject)
        {
            WmiPath = (string)wmiObject.GetPropertyValue("__PATH");
            Index = (UInt32)wmiObject.GetPropertyValue("Index");
            Guid = Guid.Parse((string)wmiObject.GetPropertyValue("GUID"));
            MACAddress = (string)wmiObject.GetPropertyValue("MACAddress");
            Name = (string)wmiObject.GetPropertyValue("Name");
            NetConnectionID = (string)wmiObject.GetPropertyValue("NetConnectionID");
            Speed = (UInt64)wmiObject.GetPropertyValue("Speed");
            var connectionStatus = ConnectionStatus;
            IsWireless = true;
            try
            {
                var wirelessConnectionStatus = WirelessConnectionStatus;
            }
            catch (Exception) {
                IsWireless = false;
            };
        }

        public int WirelessConnectionStatus
        {
            get {
                if (IsWireless)
                {
                    IntPtr handle = IntPtr.Zero;
                    uint negotiatedVersion;
                    try
                    {
                        if (NetworkInterop.WlanOpenHandle(1, IntPtr.Zero, out negotiatedVersion, ref handle) != 0)
                            throw new NotSupportedException("This network adapter does not support Wireless");

                        IntPtr ptr = new IntPtr();

                        uint dataSize;

                        var interfaceGuid = Guid;

                        if (NetworkInterop.WlanQueryInterface(handle, ref interfaceGuid, NetworkInterop.WLAN_INTF_OPCODE.wlan_intf_opcode_interface_state, IntPtr.Zero, out dataSize, ref ptr, IntPtr.Zero) != 0)
                            throw new NotSupportedException("This network adapter does not support Wireless");

                        LastWirelessConnectionStatus = Marshal.ReadInt32(ptr);


                        NetworkInterop.WlanFreeMemory(ptr);

                        return LastWirelessConnectionStatus;
                    }
                    finally
                    {
                        if (handle != IntPtr.Zero)
                            NetworkInterop.WlanCloseHandle(handle, IntPtr.Zero);
                    }

                }
                else
                {
                    throw new NotSupportedException("This network adapter does not support Wireless");
                }
            }
        }
        public string WirelessConnectionStatusMeaning(int status)
        {
            switch ((NetworkInterop.WLAN_INTERFACE_STATE)status)
            {
                case NetworkInterop.WLAN_INTERFACE_STATE.wlan_interface_state_ad_hoc_network_formed:
                    return "Ad Hoc Network Formed";
                case NetworkInterop.WLAN_INTERFACE_STATE.wlan_interface_state_associating:
                    return "Associating";
                case NetworkInterop.WLAN_INTERFACE_STATE.wlan_interface_state_authenticating:
                    return "Authenticating";
                case NetworkInterop.WLAN_INTERFACE_STATE.wlan_interface_state_connected:
                    return "Connected";
                case NetworkInterop.WLAN_INTERFACE_STATE.wlan_interface_state_disconnected:
                    return "Disconnected";
                case NetworkInterop.WLAN_INTERFACE_STATE.wlan_interface_state_disconnecting:
                    return "Disconnecting";
                case NetworkInterop.WLAN_INTERFACE_STATE.wlan_interface_state_discovering:
                    return "Discovering";
                case NetworkInterop.WLAN_INTERFACE_STATE.wlan_interface_state_not_ready:
                    return "Not Ready";
                default:
                    return "Unknown";
            }
        }

        public UInt16 ConnectionStatus
        {
            get
            {
                using (var wmiObject = new ManagementObject(WmiPath))
                {
                    LastConnectionStatus = (UInt16)wmiObject.GetPropertyValue("NetConnectionStatus");
                }
                return LastConnectionStatus;
            }
        }
        public string ConnectionStatusMeaning(UInt16 status)
        {
            switch (status)
            {
                case (UInt16)0:
                    return "Disconnected";
                case (UInt16)1:
                    return "Connecting";
                case (UInt16)2:
                    return "Connected";
                case (UInt16)3:
                    return "Disconnecting";
                case (UInt16)4:
                    return "Hardware not present";
                case (UInt16)5:
                    return "Hardware disabled";
                case (UInt16)6:
                    return "Hardware malfunction";
                case (UInt16)7:
                    return "Media disconnected";
                case (UInt16)8:
                    return "Authenticating";
                case (UInt16)9:
                    return "Authentication succeeded";
                case (UInt16)10:
                    return "Authentication failed";
                case (UInt16)11:
                    return "Invalid address";
                case (UInt16)12:
                    return "Credentials required";
                default:
                    return "Unknown";
            }
        }

    }
}
