using Disco.Models.Services.Documents;
using Disco.Models.Services.Job;
using Disco.Models.Services.Jobs.JobLists;
using Disco.Models.Services.Plugins.Details;
using Disco.Models.UI.Job;
using Disco.Services.Plugins;
using Disco.Services.Plugins.Features.DocumentHandlerProvider;
using Disco.Web.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;

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
            HandlersPresent = Plugins.GetPluginFeatures(typeof(DocumentHandlerProviderFeature)).Any(),
        };

        public List<Disco.Models.Repository.JobSubType> UpdatableJobSubTypes { get; set; }
        public List<Disco.Models.Repository.JobQueue> AvailableQueues { get; set; }

        public LocationModes LocationMode { get; set; }
        public List<JobLocationReference> LocationOptions { get; set; }

        public DetailsResult UserDetails { get; set; }
        public bool HasUserPhoto { get; set; }
    }
}