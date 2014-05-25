using Disco.Models.Services.Devices.Importing;

namespace Disco.Models.UI.Device
{
    public interface DeviceImportReviewModel : BaseUIModel
    {
        IDeviceImportContext Context { get; set; }

        int StatisticErrorRecords { get; set; }
        int StatisticNewRecords { get; set; }
        int StatisticModifiedRecords { get; set; }
        int StatisticUnmodifiedRecords { get; set; }
        int StatisticImportRecords { get; }
    }
}