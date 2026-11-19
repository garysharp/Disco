using Disco.Models.Services.Devices.Importing;

namespace Disco.Models.UI.Device
{
    public interface DeviceImportIntuneModel : BaseUIModel
    {
        IDeviceImportContext Context { get; }
    }
}
