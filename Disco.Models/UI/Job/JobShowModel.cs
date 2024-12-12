﻿using Disco.Models.Services.Documents;
using Disco.Models.Services.Jobs;
using Disco.Models.Services.Jobs.JobLists;
using System;
using System.Collections.Generic;

namespace Disco.Models.UI.Job
{
    public interface JobShowModel : BaseUIModel
    {
        Repository.Job Job { get; set; }
        TimeSpan? LongRunning { get; set; }
        List<Repository.DocumentTemplate> AvailableDocumentTemplates { get; set; }
        List<DocumentTemplatePackage> AvailableDocumentTemplatePackages { get; set; }
        List<Repository.JobSubType> UpdatableJobSubTypes { get; set; }
        List<Repository.JobQueue> AvailableQueues { get; set; }

        LocationModes LocationMode { get; set; }
        List<JobLocationReference> LocationOptions { get; set; }
        Dictionary<string, string> UserDetails { get; set; }
        bool HasUserPhoto { get; set; }
    }
}
