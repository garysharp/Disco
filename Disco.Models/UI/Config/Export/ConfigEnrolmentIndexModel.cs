using System;
using System.Collections.Generic;

namespace Disco.Models.UI.Config.Export
{
    public interface ConfigExportCreateModel : BaseUIModel
    {
        string ExportTypeName { get; set; }
        Guid Id { get; set; }

        string Name { get; set; }
        string Description { get; set; }

        string FilePath { get; set; }
        bool TimestampSuffix { get; set; }

        bool ScheduleEnabled { get; set; }
        bool ScheduleMonday { get; set; }
        bool ScheduleTuesday { get; set; }
        bool ScheduleWednesday { get; set; }
        bool ScheduleThursday { get; set; }
        bool ScheduleFriday { get; set; }
        bool ScheduleSaturday { get; set; }
        bool ScheduleSunday { get; set; }

        byte ScheduleStartHour { get; set; }
        byte? ScheduleEndHour { get; set; }

        List<string> OnDemandPrincipals { get; set; }
    }
}
