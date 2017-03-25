namespace Disco.Services.Authorization.Roles.ClaimGroups.Configuration.Enrolment
{
    [ClaimDetails("Enrolment", "Permissions related to Device Enrolment")]
    public class EnrolmentClaims
    {
        [ClaimDetails("Show Enrolment", "Can show device enrolment")]
        public bool Show { get; set; }

        [ClaimDetails("Configure Enrolment", "Can configure device enrolment")]
        public bool Configure { get; set; }

        [ClaimDetails("Show Enrolment Status", "Can show the enrolment status")]
        public bool ShowStatus { get; set; }

        [ClaimDetails("Download Bootstrapper", "Can download the Device Bootstrapper")]
        public bool DownloadBootstrapper { get; set; }
    }
}
