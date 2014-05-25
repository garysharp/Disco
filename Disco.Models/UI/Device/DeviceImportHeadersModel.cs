using Disco.Models.Services.Devices.Importing;

namespace Disco.Models.UI.Device
{
    public interface DeviceImportHeadersModel : BaseUIModel
    {
        IDeviceImportContext Context { get; set; }
    }
}