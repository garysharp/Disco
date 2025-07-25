namespace Disco.Models.UI.Config.DeviceFlag
{
    public interface ConfigDeviceFlagCreateModel : BaseUIModel
    {
        string Name { get; set; }
        string Description { get; set; }
    }
}
