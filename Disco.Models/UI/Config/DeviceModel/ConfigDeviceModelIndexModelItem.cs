namespace Disco.Models.UI.Config.DeviceModel
{
    public interface ConfigDeviceModelIndexModelItem
    {
        int Id { get; set; }
        string Name { get; set; }
        string Manufacturer { get; set; }
        string Model { get; set; }
        string ModelType { get; set; }
        int DeviceCount { get; set; }
        int DeviceDecommissionedCount { get; set; }
    }
}
