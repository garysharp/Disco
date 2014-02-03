using Disco.Models.UI.Config.JobQueue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Disco.Web.Areas.Config.Models.JobQueue
{
    public class CreateModel : ConfigJobQueueCreateModel
    {
        public Disco.Models.Repository.JobQueue JobQueue { get; set; }
    }
}