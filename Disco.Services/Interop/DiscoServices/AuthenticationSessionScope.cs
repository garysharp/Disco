using System;

namespace Disco.Services.Interop.DiscoServices
{
    [Flags]
    public enum AuthenticationSessionScope
    {
        Ping = 1,
        Host = 2,
    }
}
