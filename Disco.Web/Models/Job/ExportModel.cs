using Disco.Models.Repository;
using Disco.Models.Services.Exporting;
using Disco.Models.Services.Jobs;
using Disco.Models.UI.Job;
using Disco.Models.UI.Shared;
using System;
using System.Collections.Generic;

namespace Disco.Web.Models.Job
{
    public class ExportModel : JobExportModel
    {
        public JobExportOptions Options { get; set; }

        public Guid? ExportId { get; set; }
        public ExportResult ExportResult { get; set; }

        public List<JobQueue> JobQueues { get; set; }
        public List<KeyValuePair<string, string>> JobStatuses { get; set; }
        public List<JobType> JobTypes { get; set; }

        public SharedExportFieldsModel<JobExportOptions> Fields { get; set; }
    }
}
