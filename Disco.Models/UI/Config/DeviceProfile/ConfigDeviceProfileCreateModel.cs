using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Models.UI.Config.DeviceProfile
{
    public interface ConfigDeviceProfileCreateModel : BaseUIModel
    {
        Models.Repository.DeviceProfile DeviceProfile { get; set; }
    }
}
