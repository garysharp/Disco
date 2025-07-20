namespace Disco.Services.Authorization.Roles.ClaimGroups.Job
{
    [ClaimDetails("Types", "Permissions related to Job Types")]
    public class JobTypesClaims : BaseRoleClaimGroup
    {
        // Show Jobs
        [ClaimDetails("Show Hardware - Miscellaneous Jobs", "Can show jobs of this type")]
        public bool ShowHMisc { get; set; }
        [ClaimDetails("Show Hardware - Non-Warranty Jobs", "Can show jobs of this type")]
        public bool ShowHNWar { get; set; }
        [ClaimDetails("Show Hardware - Warranty Jobs", "Can show jobs of this type")]
        public bool ShowHWar { get; set; }

        [ClaimDetails("Show Software - Application Jobs", "Can show jobs of this type")]
        public bool ShowSApp { get; set; }
        [ClaimDetails("Show Software - Reimage Jobs", "Can show jobs of this type")]
        public bool ShowSImg { get; set; }
        [ClaimDetails("Show Software - Operating System Jobs", "Can show jobs of this type")]
        public bool ShowSOS { get; set; }

        [ClaimDetails("Show User Management Jobs", "Can show jobs of this type")]
        public bool ShowUMgmt { get; set; }

        // Create Jobs
        [ClaimDetails("Create Hardware - Miscellaneous Jobs", "Can create jobs of this type (Requires: Create Jobs)")]
        public bool CreateHMisc { get; set; }
        [ClaimDetails("Create Hardware - Non-Warranty Jobs", "Can create jobs of this type (Requires: Create Jobs)")]
        public bool CreateHNWar { get; set; }
        [ClaimDetails("Create Hardware - Warranty Jobs", "Can create jobs of this type (Requires: Create Jobs)")]
        public bool CreateHWar { get; set; }

        [ClaimDetails("Create Software - Application Jobs", "Can create jobs of this type (Requires: Create Jobs)")]
        public bool CreateSApp { get; set; }
        [ClaimDetails("Create Software - Reimage Jobs", "Can create jobs of this type (Requires: Create Jobs)")]
        public bool CreateSImg { get; set; }
        [ClaimDetails("Create Software - Operating System Jobs", "Can create jobs of this type (Requires: Create Jobs)")]
        public bool CreateSOS { get; set; }

        [ClaimDetails("Create User Management Jobs", "Can create jobs of this type (Requires: Create Jobs)")]
        public bool CreateUMgmt { get; set; }
    }
}
