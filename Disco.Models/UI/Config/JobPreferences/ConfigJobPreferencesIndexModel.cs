using Disco.Models.BI.Job;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Models.UI.Config.JobPreferences
{
    public interface ConfigJobPreferencesIndexModel : BaseUIModel
    {
        int LongRunningJobDaysThreshold { get; set; }
        int StaleJobMinutesThreshold { get; set; }
        LocationModes LocationMode { get; set; }
        List<string> LocationList { get; set; }

        List<KeyValuePair<int, string>> LongRunningJobDaysThresholdOptions();
        List<KeyValuePair<int, string>> StaleJobMinutesThresholdOptions();
        List<KeyValuePair<string, string>> LocationModeOptions();
    }
}
