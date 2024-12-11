using Disco.Data.Repository;
using Disco.Services.Plugins;
using Disco.Services.Plugins.Features.InsuranceProvider;
using Disco.Services.Users;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Data.Entity;

namespace Disco.Web.Models.Job
{
    public class LogInsuranceModel
    {
        public Disco.Models.Repository.Job Job { get; set; }
        public List<PluginFeatureManifest> Providers { get; set; }
        public PluginFeatureManifest Provider { get; set; }
        public List<Disco.Models.BI.Config.OrganisationAddress> OrganisationAddresses { get; set; }
        public Disco.Models.BI.Config.OrganisationAddress OrganisationAddress { get; set; }

        public List<Disco.Models.Repository.JobAttachment> Attachments { get; set; }

        public Disco.Models.Repository.User TechUser { get; set; }

        [Required]
        public int JobId { get; set; }
        [Required(ErrorMessage = "Please specify a Address")]
        public int? OrganisationAddressId { get; set; }
        [Required(ErrorMessage = "Please specify a Provider")]
        public string ProviderId { get; set; }
        public List<int> AttachmentIds { get; set; }
        [Required]
        public string SubmissionAction { get; set; }

        public bool IsManualProvider =>
            ProviderId == "MANUAL";
        public string ManualProviderName { get; set; }
        public string ManualProviderReference { get; set; }

        public Tuple<Type, object> ProviderSubmitJobBeginResult { get; set; }
        public string ProviderPropertiesJson { get; set; }
        public Dictionary<string, string> ProviderProperties()
        {
            var p = default(Dictionary<string, string>);
            if (!string.IsNullOrEmpty(ProviderPropertiesJson))
            {
                try
                {
                    p = JsonConvert.DeserializeObject<Dictionary<string, string>>(ProviderPropertiesJson);
                }
                catch (Exception)
                {
                    // Ignore Errors
                }
            }
            return p;
        }

        public Dictionary<string, string> DiscloseProperties { get; set; }

        public Exception Error { get; set; }

        public void UpdateModel(DiscoDataContext Database, bool IsPostBack)
        {
            Database.Configuration.LazyLoadingEnabled = true;
            if (Job == null)
            {
                string jobUserId = Database.Jobs.Where(j => j.Id == JobId).Select(j => j.UserId).FirstOrDefault();
                if (jobUserId != null)
                {
                    // Ignore update errors (Most commonly when the User Id no longer exists in AD)
                    try
                    {
                        UserService.GetUser(jobUserId, Database, true);
                    }
                    catch (Exception) { }
                }

                Job = Database.Jobs
                    .Include(j => j.Device.DeviceModel)
                    .Include(j => j.JobMetaNonWarranty)
                    .Include(j => j.JobMetaInsurance)
                    .Include(j => j.JobSubTypes)
                    .Include(j => j.JobAttachments)
                    .Where(j => j.Id == JobId)
                    .FirstOrDefault();

                if (Job == null)
                    throw new ArgumentException("Invalid Job Number Specified", "JobId");
            }

            TechUser = UserService.GetUser(UserService.CurrentUserId, Database, true);

            Providers = Plugins.GetPluginFeatures(typeof(InsuranceProviderFeature));

            if (!IsPostBack && string.IsNullOrEmpty(ProviderId))
                ProviderId = "MANUAL";

            if (!string.IsNullOrEmpty(ProviderId) && ProviderId != "MANUAL")
                Provider = Plugins.GetPluginFeature(ProviderId, typeof(InsuranceProviderFeature));

            OrganisationAddresses = Database.DiscoConfiguration.OrganisationAddresses.Addresses.OrderBy(a => a.Name).ToList();

            if (!IsPostBack && !OrganisationAddressId.HasValue)
            {
                OrganisationAddressId = Job.Device.DeviceProfile.DefaultOrganisationAddress;
                if (!OrganisationAddressId.HasValue && OrganisationAddresses.Count == 1)
                    OrganisationAddressId = OrganisationAddresses[0].Id;
            }
            if (OrganisationAddressId.HasValue)
                OrganisationAddress = OrganisationAddresses.FirstOrDefault(oa => oa.Id == OrganisationAddressId.Value);

            if (AttachmentIds == null)
            {
                if (Database.DiscoConfiguration.JobPreferences.LodgmentIncludeAllAttachmentsByDefault)
                {
                    Attachments = Job.JobAttachments.ToList();
                    AttachmentIds = Attachments.Select(a => a.Id).ToList();
                }
                else
                {
                    AttachmentIds = new List<int>();
                    Attachments = new List<Disco.Models.Repository.JobAttachment>();
                }
            }
            else
            {
                Attachments = Job.JobAttachments.Where(ja => AttachmentIds.Contains(ja.Id)).ToList();
            }
        }
    }
}