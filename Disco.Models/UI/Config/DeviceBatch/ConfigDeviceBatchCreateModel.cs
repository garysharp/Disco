namespace Disco.Models.UI.Config.DeviceBatch
{
    public interface ConfigDeviceBatchCreateModel : BaseUIModel
    {
        Models.Repository.DeviceBatch DeviceBatch { get; set; }
    }
}
