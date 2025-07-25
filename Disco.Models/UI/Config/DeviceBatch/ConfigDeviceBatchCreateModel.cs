using System;

namespace Disco.Models.UI.Config.DeviceBatch
{
    public interface ConfigDeviceBatchCreateModel : BaseUIModel
    {
        string Name { get; set; }
        DateTime PurchaseDate { get; set; }
    }
}
