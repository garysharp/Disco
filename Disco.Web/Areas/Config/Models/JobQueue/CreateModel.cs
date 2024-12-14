using Disco.Models.UI.Config.JobQueue;

namespace Disco.Web.Areas.Config.Models.JobQueue
{
    public class CreateModel : ConfigJobQueueCreateModel
    {
        public Disco.Models.Repository.JobQueue JobQueue { get; set; }
    }
}