namespace Disco.Models.UI.Config.DeviceFlag
{
    public interface ConfigDeviceFlagCreateModel : BaseUIModel
    {
        Repository.DeviceFlag DeviceFlag { get; set; }
    }
}
