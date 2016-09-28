using System;

namespace Disco.Client.Interop.Native
{
    [Flags]
    public enum ProfileInfoFlags : uint
    {
        WLAN_PROFILE_GROUP_POLICY = 1,
        WLAN_PROFILE_USER = 2
    }
}
