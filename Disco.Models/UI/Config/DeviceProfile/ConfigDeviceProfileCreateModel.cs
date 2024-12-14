namespace Disco.Models.UI.Config.DeviceProfile
{
    public interface ConfigDeviceProfileCreateModel : BaseUIModel
    {
        Models.Repository.DeviceProfile DeviceProfile { get; set; }
    }
}
