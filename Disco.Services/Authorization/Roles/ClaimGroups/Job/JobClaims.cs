namespace Disco.Services.Authorization.Roles.ClaimGroups.Job
{
    [ClaimDetails("Job", "Permissions related to Jobs")]
    public class JobClaims : BaseRoleClaimGroup
    {
        public JobClaims()
        {
            Lists = new JobListsClaims();
            Actions = new JobActionsClaims();
            Properties = new JobPropertiesClaims();
            Types = new JobTypesClaims();
        }

        [ClaimDetails("Search Jobs", "Can search jobs")]
        public bool Search { get; set; }

        [ClaimDetails("Show Jobs", "Can show jobs")]
        public bool Show { get; set; }

        [ClaimDetails("Show Daily Opened & Closed", "Can show daily opened & closed chart")]
        public bool ShowDailyChart { get; set; }

        [ClaimDetails("Show Logs", "Can show job logs")]
        public bool ShowLogs { get; set; }
        [ClaimDetails("Show Attachments", "Can show job attachments")]
        public bool ShowAttachments { get; set; }

        [ClaimDetails("Show Jobs Queues", "Can show jobs queues")]
        public bool ShowJobsQueues { get; set; }

        [ClaimDetails("Show Non-Warranty Components", "Can show non-warranty job components")]
        public bool ShowNonWarrantyComponents { get; set; }
        [ClaimDetails("Show Non-Warranty Finance", "Can show non-warranty job finance")]
        public bool ShowNonWarrantyFinance { get; set; }
        [ClaimDetails("Show Non-Warranty Repairs", "Can show non-warranty job repairs")]
        public bool ShowNonWarrantyRepairs { get; set; }
        [ClaimDetails("Show Non-Warranty Insurance", "Can show non-warranty job insurance")]
        public bool ShowNonWarrantyInsurance { get; set; }

        [ClaimDetails("Show Warranty", "Can show job warranty")]
        public bool ShowWarranty { get; set; }

        [ClaimDetails("Show Flags", "Can show job flags")]
        public bool ShowFlags { get; set; }

        public JobListsClaims Lists { get; set; }

        public JobActionsClaims Actions { get; set; }

        public JobPropertiesClaims Properties { get; set; }

        public JobTypesClaims Types { get; set; }
    }
}
