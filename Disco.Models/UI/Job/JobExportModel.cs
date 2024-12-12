using Disco.Models.Repository;
using Disco.Models.Services.Exporting;
using Disco.Models.Services.Jobs.Exporting;
using System.Collections.Generic;

namespace Disco.Models.UI.Job
{
    public interface JobExportModel : BaseUIModel
    {
        JobExportOptions Options { get; set; }

        string ExportSessionId { get; set; }
        ExportResult ExportSessionResult { get; set; }

        List<JobQueue> JobQueues { get; set; }
        List<KeyValuePair<string, string>> JobStatuses { get; set; }
        List<JobType> JobTypes { get; set; }
    }
}
