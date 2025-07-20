using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Disco.Models.Repository
{
    public class JobSubType
    {
        [Key, StringLength(20), Column(Order = 0)]
        public string Id { get; set; }
        [Key, Required, Column(Order = 1)]
        public string JobTypeId { get; set; }
        [Required, StringLength(100)]
        public string Description { get; set; }

        public virtual IList<DocumentTemplate> AttachmentTypes { get; set; }
        public virtual IList<DeviceComponent> DeviceComponents { get; set; }
        public virtual IList<JobQueue> JobQueues { get; set; }

        [ForeignKey("JobTypeId")]
        public virtual JobType JobType { get; set; }
        public virtual IList<Job> Jobs { get; set; }

        public static class UserManagementJobSubTypes
        {
            public const string Infringement = "Infringement";
            public const string Contact = "Contact";
            public const string BYOD = "BYOD";
        }

        public override string ToString()
        {
            return Description;
        }
    }
}
