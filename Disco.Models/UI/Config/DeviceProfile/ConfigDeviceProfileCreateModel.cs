namespace Disco.Models.UI.Config.DeviceProfile
{
    public interface ConfigDeviceProfileCreateModel : BaseUIModel
    {
        string Name { get; set; }
        string ShortName { get; set; }
        string Description { get; set; }
    }
}
