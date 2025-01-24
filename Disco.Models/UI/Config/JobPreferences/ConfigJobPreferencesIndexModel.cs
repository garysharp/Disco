using Disco.Models.Services.Jobs;
using System.Collections.Generic;

namespace Disco.Models.UI.Config.JobPreferences
{
    public interface ConfigJobPreferencesIndexModel : BaseUIModel
    {
        string InitialCommentsTemplate { get; set; }
        int LongRunningJobDaysThreshold { get; set; }
        int StaleJobMinutesThreshold { get; set; }
        bool LodgmentIncludeAllAttachmentsByDefault { get; set; }
        LocationModes LocationMode { get; set; }
        List<string> LocationList { get; set; }

        string OnCreateExpression { get; set; }
        string OnDeviceReadyForReturnExpression { get; set; }
        string OnCloseExpression { get; set; }

        List<KeyValuePair<int, string>> LongRunningJobDaysThresholdOptions();
        List<KeyValuePair<int, string>> StaleJobMinutesThresholdOptions();
        List<KeyValuePair<string, string>> LocationModeOptions();
    }
}
