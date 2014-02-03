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

namespace Disco.Services.Authorization.Roles.ClaimGroups.Configuration
{
    [ClaimDetails("Configuration", "Permissions related to Disco Configuration")]
    public class ConfigClaims : BaseRoleClaimGroup
    {
        public ConfigClaims()
        {
            this.DeviceCertificate = new DeviceCertificateClaims();
            this.Enrolment = new EnrolmentClaims();
            this.DeviceBatch = new DeviceBatchClaims();
            this.DeviceModel = new DeviceModelClaims();
            this.DeviceProfile = new DeviceProfileClaims();
            this.DocumentTemplate = new DocumentTemplateClaims();
            this.Logging = new LoggingClaims();
            this.Plugin = new PluginClaims();
            this.System = new SystemClaims();
            this.Organisation = new OrganisationClaims();
            this.JobPreferences = new JobPreferencesClaims();
            this.JobQueue = new JobQueueClaims();
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
    }
}
