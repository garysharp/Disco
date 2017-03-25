namespace Disco.Services.Authorization.Roles.ClaimGroups.Job
{
    [ClaimDetails("Actions", "Permissions related to Job Actions")]
    public class JobActionsClaims : BaseRoleClaimGroup
    {
        [ClaimDetails("Create Jobs", "Can create jobs")]
        public bool Create { get; set; }
        [ClaimDetails("Close Jobs", "Can close jobs")]
        public bool Close { get; set; }
        [ClaimDetails("Force Close Jobs", "Can force close jobs")]
        public bool ForceClose { get; set; }
        [ClaimDetails("Reopen Jobs", "Can reopen jobs")]
        public bool Reopen { get; set; }
        [ClaimDetails("Delete Jobs", "Can delete jobs")]
        public bool Delete { get; set; }

        [ClaimDetails("Add to Own Queues", "Can add to own job queues")]
        public bool AddOwnQueues { get; set; }
        [ClaimDetails("Add to Any Queues", "Can add to any job queues")]
        public bool AddAnyQueues { get; set; }
        [ClaimDetails("Remove from Own Queues", "Can remove from own job queues")]
        public bool RemoveOwnQueues { get; set; }
        [ClaimDetails("Remove from Any Queues", "Can remove from any job queues")]
        public bool RemoveAnyQueues { get; set; }

        [ClaimDetails("Log Warranty", "Can log warranty for jobs")]
        public bool LogWarranty { get; set; }
        [ClaimDetails("Log Repair", "Can log repair for non-warranty jobs")]
        public bool LogRepair { get; set; }

        [ClaimDetails("Convert HWar Jobs To HNWar", "Can convert warranty jobs to non-warranty jobs")]
        public bool ConvertHWarToHNWar { get; set; }

        [ClaimDetails("Add Logs", "Can add job logs")]
        public bool AddLogs { get; set; }
        [ClaimDetails("Remove Any Logs", "Can remove any job logs")]
        public bool RemoveAnyLogs { get; set; }
        [ClaimDetails("Remove Own Logs", "Can remove own job logs")]
        public bool RemoveOwnLogs { get; set; }

        [ClaimDetails("Add Attachments", "Can add attachments to jobs")]
        public bool AddAttachments { get; set; }
        [ClaimDetails("Remove Any Attachments", "Can remove any attachments from jobs")]
        public bool RemoveAnyAttachments { get; set; }
        [ClaimDetails("Remove Own Attachments", "Can remove own attachments from jobs")]
        public bool RemoveOwnAttachments { get; set; }

        [ClaimDetails("Generate Documents", "Can generate documents for jobs")]
        public bool GenerateDocuments { get; set; }

        [ClaimDetails("Update Sub Types", "Can update sub types for jobs")]
        public bool UpdateSubTypes { get; set; }
    }
}
