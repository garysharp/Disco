namespace Disco.Models.Services.Interop.DiscoServices
{
    public interface IConnectNotification
    {
        int Version { get; }
        int Type { get; }
        string Content { get; }
    }
}
