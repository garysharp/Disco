using System.Collections.Generic;

namespace Disco.Models.UI.Job
{
    public interface JobCreateModel : BaseUIModel
    {
        string DeviceSerialNumber { get; set; }
        string UserId { get; set; }

        string Type { get; set; }
        List<string> SubTypes { get; set; }

        string Comments { get; set; }
        bool RegenerateCommentsOnTypeChange { get; set; }

        bool? DeviceHeld { get; set; }

        string SourceUrl { get; set; }

        bool? QuickLog { get; set; }
        int? QuickLogTaskTimeMinutes { get; set; }
        int? QuickLogTaskTimeMinutesOther { get; set; }

        Repository.Device Device { get; set; }
        Repository.User User { get; set; }
        List<Repository.JobType> JobTypes { get; set; }
    }
}
