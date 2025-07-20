using Disco.Models.Repository;
using System.Collections.Generic;

namespace Disco.Models.UI.Config.DeviceFlag
{
    public interface ConfigDeviceFlagShowModel : BaseUIModel
    {
        Repository.DeviceFlag DeviceFlag { get; set; }

        int CurrentAssignmentCount { get; set; }
        int TotalAssignmentCount { get; set; }

        IEnumerable<KeyValuePair<string, string>> Icons { get; set; }
        IEnumerable<KeyValuePair<string, string>> ThemeColours { get; set; }

        FlagPermission Permission { get; set; }
    }
}
