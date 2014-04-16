using Disco.Models.Interop.ActiveDirectory;
using Disco.Models.Services.Jobs.JobQueues;
using Disco.Models.UI.Config.JobQueue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Disco.Web.Areas.Config.Models.JobQueue
{
    public class ShowModel : ConfigJobQueueShowModel
    {
        public IJobQueueToken Token { get; set; }

        public List<SubjectDescriptor> Subjects { get; set; }

        public class SubjectDescriptor
        {
            public bool IsGroup { get; set; }
            public string Name { get; set; }
            public string Id { get; set; }

            public static SubjectDescriptor FromActiveDirectoryObject(IActiveDirectoryObject ADObject)
            {
                var item = new SubjectDescriptor()
                {
                    Id = ADObject.NetBiosId,
                    Name = ADObject.DisplayName
                };

                if (ADObject is ActiveDirectoryGroup)
                    item.IsGroup = true;

                return item;
            }
        }

        public int OpenJobCount { get; set; }
        public int TotalJobCount { get; set; }

        public List<Disco.Models.Repository.JobType> JobTypes { get; set; }

        public bool CanDelete { get; set; }

    }
}