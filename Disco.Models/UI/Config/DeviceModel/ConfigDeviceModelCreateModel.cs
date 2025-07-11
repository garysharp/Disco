namespace Disco.Models.UI.Config.DeviceModel
{
    public interface ConfigDeviceModelCreateModel : BaseUIModel
    {
        string Description { get; set; }
        string Manufacturer { get; set; }
        string ManufacturerModel { get; set; }
    }
}
