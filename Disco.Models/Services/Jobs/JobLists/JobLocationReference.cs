using System.Collections.Generic;

namespace Disco.Models.Services.Jobs.JobLists
{
    public class JobLocationReference
    {
        public string Location { get; set; }
        public List<JobTableStatusItemModel> References { get; set; }
    }
}
