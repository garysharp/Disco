namespace Disco.Services.Authorization.Roles.ClaimGroups.Configuration.JobQueue
{
    [ClaimDetails("Job Queues", "Permissions related to Job Queues")]
    public class JobQueueClaims : BaseRoleClaimGroup
    {
        [ClaimDetails("Configure Job Queues", "Can configure job queues")]
        public bool Configure { get; set; }

        [ClaimDetails("Create Job Queues", "Can create job queues")]
        public bool Create { get; set; }

        [ClaimDetails("Delete Job Queues", "Can delete job queues")]
        public bool Delete { get; set; }

        [ClaimDetails("Show Job Queues", "Can show job queues")]
        public bool Show { get; set; }
    }
}
