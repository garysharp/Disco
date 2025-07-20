using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Disco.Models.Repository
{
    public class JobType
    {
        [StringLength(5), Key]
        public string Id { get; set; }
        [StringLength(100)]
        public string Description { get; set; }
        
        public virtual IList<JobSubType> JobSubTypes { get; set; }

        public override string ToString()
        {
            return Description;
        }

        public static class JobTypeIds
        {
            public const string HMisc = "HMisc";
            public const string HNWar = "HNWar";
            public const string HWar = "HWar";
            public const string SApp = "SApp";
            public const string SImg = "SImg";
            public const string SOS = "SOS";
            public const string UMgmt = "UMgmt";
        }
    }
}
