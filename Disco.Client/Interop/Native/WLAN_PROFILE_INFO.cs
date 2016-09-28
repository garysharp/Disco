using System.Runtime.InteropServices;

namespace Disco.Client.Interop.Native
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct WLAN_PROFILE_INFO
    {
        /// <summary>
        /// The name of the profile. This value may be the name of a domain if the profile is for provisioning. Profile names are case-sensitive.
        ///   This string must be NULL-terminated.
        /// Windows XP with SP3 and Wireless LAN API for Windows XP with SP2:  The name of the profile is derived automatically from
        ///   the SSID of the wireless network. For infrastructure network profiles, the name of the profile is the SSID of the network.
        ///   For ad hoc network profiles, the name of the profile is the SSID of the ad hoc network followed by -adhoc.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string strInterfaceDescription;

        /// <summary>
        /// A set of flags specifying settings for wireless profile. These values are defined in the Wlanapi.h header file.
        /// Windows XP with SP3 and Wireless LAN API for Windows XP with SP2:  dwFlags must be 0. Per-user profiles are not supported.
        /// </summary>
        public ProfileInfoFlags dwFlags;
    }
}
