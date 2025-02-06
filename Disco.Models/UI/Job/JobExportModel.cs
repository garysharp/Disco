using Disco.Models.Repository;
using Disco.Models.Services.Exporting;
using Disco.Models.Services.Jobs;
using System;
using System.Collections.Generic;

namespace Disco.Models.UI.Job
{
    public interface JobExportModel : BaseUIModel
    {
        JobExportOptions Options { get; set; }

        Guid? ExportId { get; set; }
        ExportResult ExportResult { get; set; }

        List<JobQueue> JobQueues { get; set; }
        List<KeyValuePair<string, string>> JobStatuses { get; set; }
        List<JobType> JobTypes { get; set; }
    }
}
