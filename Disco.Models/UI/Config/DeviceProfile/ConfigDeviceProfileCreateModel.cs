namespace Disco.Models.UI.Config.DeviceProfile
{
    public interface ConfigDeviceProfileCreateModel : BaseUIModel
    {
        Repository.DeviceProfile DeviceProfile { get; set; }
    }
}
