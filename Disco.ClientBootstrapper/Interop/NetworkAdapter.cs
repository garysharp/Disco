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
        public ulong Speed { get; set; }
        public ushort LastConnectionStatus { get; set; }
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
            Index = (uint)wmiObject.GetPropertyValue("Index");
            Guid = Guid.Parse((string)wmiObject.GetPropertyValue("GUID"));
            MACAddress = (string)wmiObject.GetPropertyValue("MACAddress");
            Name = (string)wmiObject.GetPropertyValue("Name");
            NetConnectionID = (string)wmiObject.GetPropertyValue("NetConnectionID");
            Speed = (ulong)wmiObject.GetPropertyValue("Speed");
            _ = ConnectionStatus;
            IsWireless = true;
            try
            {
                var wirelessConnectionStatus = WirelessConnectionStatus;
            }
            catch (Exception)
            {
                IsWireless = false;
            }
        }

        public int WirelessConnectionStatus
        {
            get
            {
                if (IsWireless)
                {
                    IntPtr handle = IntPtr.Zero;
                    try
                    {
                        if (NetworkInterop.WlanOpenHandle(1, IntPtr.Zero, out var negotiatedVersion, ref handle) != 0)
                            throw new NotSupportedException("This network adapter does not support Wireless");

                        IntPtr ptr = new IntPtr();


                        var interfaceGuid = Guid;

                        if (NetworkInterop.WlanQueryInterface(handle, ref interfaceGuid, NetworkInterop.WLAN_INTF_OPCODE.wlan_intf_opcode_interface_state, IntPtr.Zero, out var dataSize, ref ptr, IntPtr.Zero) != 0)
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

        public ushort ConnectionStatus
        {
            get
            {
                using (var wmiObject = new ManagementObject(WmiPath))
                {
                    LastConnectionStatus = (ushort)wmiObject.GetPropertyValue("NetConnectionStatus");
                }
                return LastConnectionStatus;
            }
        }
        public string ConnectionStatusMeaning(ushort status)
        {
            switch (status)
            {
                case (ushort)0:
                    return "Disconnected";
                case (ushort)1:
                    return "Connecting";
                case (ushort)2:
                    return "Connected";
                case (ushort)3:
                    return "Disconnecting";
                case (ushort)4:
                    return "Hardware not present";
                case (ushort)5:
                    return "Hardware disabled";
                case (ushort)6:
                    return "Hardware malfunction";
                case (ushort)7:
                    return "Media disconnected";
                case (ushort)8:
                    return "Authenticating";
                case (ushort)9:
                    return "Authentication succeeded";
                case (ushort)10:
                    return "Authentication failed";
                case (ushort)11:
                    return "Invalid address";
                case (ushort)12:
                    return "Credentials required";
                default:
                    return "Unknown";
            }
        }

    }
}
