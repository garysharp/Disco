using System.Collections.Generic;

namespace Disco.Models.UI.Config.DeviceBatch
{
    public interface ConfigDeviceBatchIndexModel : BaseUIModel
    {
        List<ConfigDeviceBatchIndexModelItem> DeviceBatches { get; set; }
    }
}
