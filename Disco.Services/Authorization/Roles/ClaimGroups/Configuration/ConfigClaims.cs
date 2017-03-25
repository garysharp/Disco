using Disco.Services.Authorization.Roles.ClaimGroups.Configuration.DeviceBatch;
using Disco.Services.Authorization.Roles.ClaimGroups.Configuration.DeviceCertificate;
using Disco.Services.Authorization.Roles.ClaimGroups.Configuration.DeviceModel;
using Disco.Services.Authorization.Roles.ClaimGroups.Configuration.DeviceProfile;
using Disco.Services.Authorization.Roles.ClaimGroups.Configuration.DocumentTemplate;
using Disco.Services.Authorization.Roles.ClaimGroups.Configuration.Enrolment;
using Disco.Services.Authorization.Roles.ClaimGroups.Configuration.JobPreferences;
using Disco.Services.Authorization.Roles.ClaimGroups.Configuration.JobQueue;
using Disco.Services.Authorization.Roles.ClaimGroups.Configuration.Logging;
using Disco.Services.Authorization.Roles.ClaimGroups.Configuration.Origanisation;
using Disco.Services.Authorization.Roles.ClaimGroups.Configuration.Plugin;
using Disco.Services.Authorization.Roles.ClaimGroups.Configuration.System;
using Disco.Services.Authorization.Roles.ClaimGroups.Configuration.UserFlag;

namespace Disco.Services.Authorization.Roles.ClaimGroups.Configuration
{
    [ClaimDetails("Configuration", "Permissions related to Disco Configuration")]
    public class ConfigClaims : BaseRoleClaimGroup
    {
        public ConfigClaims()
        {
            DeviceCertificate = new DeviceCertificateClaims();
            Enrolment = new EnrolmentClaims();
            DeviceBatch = new DeviceBatchClaims();
            DeviceModel = new DeviceModelClaims();
            DeviceProfile = new DeviceProfileClaims();
            DocumentTemplate = new DocumentTemplateClaims();
            Logging = new LoggingClaims();
            Plugin = new PluginClaims();
            System = new SystemClaims();
            Organisation = new OrganisationClaims();
            JobPreferences = new JobPreferencesClaims();
            JobQueue = new JobQueueClaims();
            UserFlag = new UserFlagClaims();
        }

        [ClaimDetails("Show Configuration", "Can show the configuration menu")]
        public bool Show { get; set; }

        public DeviceCertificateClaims DeviceCertificate { get; set; }

        public EnrolmentClaims Enrolment { get; set; }

        public DeviceBatchClaims DeviceBatch { get; set; }

        public DeviceModelClaims DeviceModel { get; set; }

        public DeviceProfileClaims DeviceProfile { get; set; }

        public DocumentTemplateClaims DocumentTemplate { get; set; }

        public LoggingClaims Logging { get; set; }

        public PluginClaims Plugin { get; set; }

        public SystemClaims System { get; set; }

        public OrganisationClaims Organisation { get; set; }

        public JobPreferencesClaims JobPreferences { get; set; }

        public JobQueueClaims JobQueue { get; set; }

        public UserFlagClaims UserFlag { get; set; }
    }
}
