namespace Disco.Services.Authorization.Roles.ClaimGroups.Configuration.JobPreferences
{
    [ClaimDetails("Job Preferences", "Permissions related to Job Preferences")]
    public class JobPreferencesClaims
    {
        [ClaimDetails("Show Job Preferences", "Can show job preferences")]
        public bool Show { get; set; }

        [ClaimDetails("Configure Job Preferences", "Can configure job preferences")]
        public bool Configure { get; set; }
    }
}
