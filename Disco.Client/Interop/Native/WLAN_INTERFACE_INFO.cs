using System;
using System.Runtime.InteropServices;

namespace Disco.Client.Interop.Native
{
    /// <summary >
    /// The WLAN_INTERFACE_INFO structure contains information about a wireless LAN interface.
    /// </summary >
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct WLAN_INTERFACE_INFO
    {
        /// <summary>
        /// Contains the GUID of the interface.
        /// </summary>
        public Guid InterfaceGuid;

        /// <summary>
        /// Contains the description of the interface.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string strInterfaceDescription;

        /// <summary>
        /// Contains a WLAN_INTERFACE_STATE value that indicates the current state of the interface.
        /// Windows XP with SP3 and Wireless LAN API for Windows XP with SP2:  Only the wlan_interface_state_connected,
        ///   wlan_interface_state_disconnected, and wlan_interface_state_authenticating values are supported.
        /// </summary>
        public WLAN_INTERFACE_STATE isState;
    }
}
