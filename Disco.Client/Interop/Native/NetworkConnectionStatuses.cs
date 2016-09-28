using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Disco.Client.Interop.Native
{
    public enum NetworkConnectionStatuses : ushort
    {
        Disconnected = 0,
        Connecting,
        Connected,
        Disconnecting,
        HardwareNotPresent,
        HardwareDisabled,
        HardwareMalfunction,
        MediaDisconnected,
        Authenticating,
        AuthenticationSucceeded,
        AuthenticationFailed,
        InvalidAddress,
        CredentialsRequired
    }
}
