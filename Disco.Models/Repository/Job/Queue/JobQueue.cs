using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Disco.Models.Repository
{
    public class JobQueue
    {
        [Key]
        public int Id { get; set; }
        [Required, StringLength(100)]
        public string Name { get; set; }
        [StringLength(500), DataType(DataType.MultilineText)]
        public string Description { get; set; }
        [Required, StringLength(25)]
        public string Icon { get; set; }
        [Required, StringLength(10)]
        public string IconColour { get; set; }

        public int? DefaultSLAExpiry { get; set; }
        [Required]
        public JobQueuePriority Priority { get; set; }

        public string SubjectIds { get; set; }

        public virtual IList<JobSubType> JobSubTypes { get; set; }

        public virtual IList<JobQueueJob> QueueJobs { get; set; }

        public override string ToString()
        {
            return this.Name;
        }
    }
}
