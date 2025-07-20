using Disco.Models.Repository;
using Disco.Models.UI.Config.DeviceFlag;
using Disco.Services.Devices.DeviceFlags;
using System.Collections.Generic;

namespace Disco.Web.Areas.Config.Models.DeviceFlag
{
    public class ShowModel : ConfigDeviceFlagShowModel
    {
        public Disco.Models.Repository.DeviceFlag DeviceFlag { get; set; }

        public int CurrentAssignmentCount { get; set; }
        public int TotalAssignmentCount { get; set; }

        public DeviceFlagDevicesManagedGroup DevicesLinkedGroup { get; set; }
        public DeviceFlagDeviceAssignedUsersManagedGroup AssignedUserLinkedGroup { get; set; }

        public IEnumerable<KeyValuePair<string, string>> Icons { get; set; }
        public IEnumerable<KeyValuePair<string, string>> ThemeColours { get; set; }

        public FlagPermission Permission { get; set; }
    }
}
