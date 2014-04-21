using Disco.Models.Services.Jobs.JobQueues;
using Disco.Models.UI.Config.JobQueue;
using Disco.Web.Areas.API.Models.Shared;
using System.Collections.Generic;

namespace Disco.Web.Areas.Config.Models.JobQueue
{
    public class ShowModel : ConfigJobQueueShowModel
    {
        public IJobQueueToken Token { get; set; }

        public List<SubjectDescriptorModel> Subjects { get; set; }

        public int OpenJobCount { get; set; }
        public int TotalJobCount { get; set; }

        public List<Disco.Models.Repository.JobType> JobTypes { get; set; }

        public bool CanDelete { get; set; }
    }
}