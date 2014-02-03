using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Models.Repository
{
    public class JobQueueJob
    {
        public int Id { get; set; }

        [Required]
        public int JobQueueId { get; set; }
        [Required]
        public int JobId { get; set; }
        
        [Required]
        public DateTime AddedDate { get; set; }
        [Required]
        public string AddedUserId { get; set; }
        public string AddedComment { get; set; }
        
        public DateTime? RemovedDate { get; set; }
        public string RemovedUserId { get; set; }
        public string RemovedComment { get; set; }

        public DateTime? SLAExpiresDate { get; set; }
        public JobQueuePriority Priority { get; set; }

        [ForeignKey("JobQueueId")]
        public virtual JobQueue JobQueue { get; set; }
        [ForeignKey("JobId")]
        public virtual Job Job { get; set; }
        [ForeignKey("AddedUserId")]
        public virtual User AddedUser { get; set; }
        [ForeignKey("RemovedUserId")]
        public virtual User RemovedUser { get; set; }
    }
}
