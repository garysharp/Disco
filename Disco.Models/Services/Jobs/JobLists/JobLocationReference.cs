using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Models.Services.Jobs.JobLists
{
    public class JobLocationReference
    {
        public string Location { get; set; }
        public List<JobTableStatusItemModel> References { get; set; }
    }
}
