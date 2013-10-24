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

        List<KeyValuePair<int, string>> LongRunningJobDaysThresholdOptions();
    }
}
