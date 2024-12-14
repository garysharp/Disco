namespace Disco.Models.UI.Config.DeviceBatch
{
    public interface ConfigDeviceBatchShowModelMembership : BaseUIModel
    {
        Disco.Models.Repository.DeviceModel DeviceModel { get; set; }
        int DeviceCount { get; set; }
        int DeviceDecommissionedCount { get; set; }
    }
}
