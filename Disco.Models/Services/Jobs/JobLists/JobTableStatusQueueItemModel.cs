using Disco.Models.Repository;
using System;

namespace Disco.Models.Services.Jobs.JobLists
{
    public class JobTableStatusQueueItemModel
    {
        public int Id { get; set; }
        public int QueueId { get; set; }
        public DateTime AddedDate { get; set; }
        public DateTime? SLAExpiresDate { get; set; }
        public JobQueuePriority Priority { get; set; }
    }
}
