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
