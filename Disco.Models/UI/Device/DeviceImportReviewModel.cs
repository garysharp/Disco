using Disco.Models.BI.Device;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Models.UI.Device
{
    public interface DeviceImportReviewModel : BaseUIModel
    {
        string ImportParseTaskId { get; set; }
        string ImportFilename { get; set; }
        List<ImportDevice> ImportDevices { get; set; }
    }
}
