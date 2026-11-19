using Disco.Models.Services.Devices.Importing;

namespace Disco.Models.UI.Device
{
    public interface DeviceImportGoogleModel : BaseUIModel
    {
        IDeviceImportContext Context { get; }
    }
}
