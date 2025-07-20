namespace Disco.Models.UI.Config.DeviceBatch
{
    public interface ConfigDeviceBatchShowModelMembership : BaseUIModel
    {
        Repository.DeviceModel DeviceModel { get; set; }
        int DeviceCount { get; set; }
        int DeviceDecommissionedCount { get; set; }
    }
}
