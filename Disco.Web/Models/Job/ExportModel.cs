using Disco.Models.Repository;
using Disco.Models.Services.Exporting;
using Disco.Models.Services.Jobs.Exporting;
using Disco.Models.UI.Job;
using System.Collections.Generic;

namespace Disco.Web.Models.Job
{
    public class ExportModel : JobExportModel
    {
        public JobExportOptions Options { get; set; }

        public string ExportSessionId { get; set; }
        public ExportResult ExportSessionResult { get; set; }

        public List<JobQueue> JobQueues { get; set; }
        public List<KeyValuePair<string, string>> JobStatuses { get; set; }
        public List<JobType> JobTypes { get; set; }

    }
}
