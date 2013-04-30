using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Models.UI.Config.DeviceBatch
{
    public interface ConfigDeviceBatchCreateModel : BaseUIModel
    {
        Models.Repository.DeviceBatch DeviceBatch { get; set; }
    }
}
