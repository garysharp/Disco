namespace Disco.Models.UI.Config.DeviceBatch
{
    public interface ConfigDeviceBatchCreateModel : BaseUIModel
    {
        Repository.DeviceBatch DeviceBatch { get; set; }
    }
}
