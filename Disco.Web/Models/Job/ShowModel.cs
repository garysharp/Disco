using Disco.Models.Services.Documents;
using Disco.Models.Services.Job;
using Disco.Models.Services.Jobs.JobLists;
using Disco.Models.UI.Job;
using Disco.Web.Models.Shared;
using System;
using System.Collections.Generic;

namespace Disco.Web.Models.Job
{
    public class ShowModel : JobShowModel
    {
        public Disco.Models.Repository.Job Job { get; set; }

        public TimeSpan? LongRunning { get; set; }

        public List<Disco.Models.Repository.DocumentTemplate> AvailableDocumentTemplates { get; set; }
        public List<DocumentTemplatePackage> AvailableDocumentTemplatePackages { get; set; }
        public GenerateDocumentControlModel GenerateDocumentControlModel => new GenerateDocumentControlModel()
        {
            Target = Job,
            Templates = AvailableDocumentTemplates,
            TemplatePackages = AvailableDocumentTemplatePackages,
        };

        public List<Disco.Models.Repository.JobSubType> UpdatableJobSubTypes { get; set; }
        public List<Disco.Models.Repository.JobQueue> AvailableQueues { get; set; }

        public LocationModes LocationMode { get; set; }
        public List<JobLocationReference> LocationOptions { get; set; }
    }
}