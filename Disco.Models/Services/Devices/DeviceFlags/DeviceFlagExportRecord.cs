using Disco.Models.Exporting;
using Disco.Models.Repository;
using System.Collections.Generic;

namespace Disco.Models.Services.Devices.DeviceFlag
{
    public class DeviceFlagExportRecord : IExportRecord
    {
        public DeviceFlagAssignment Assignment { get; set; }
        public Dictionary<string, string> AssignedUserCustomDetails { get; set; }
    }
}
