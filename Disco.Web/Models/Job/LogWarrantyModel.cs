using Disco.Data.Repository;
using Disco.Services.Plugins;
using Disco.Services.Plugins.Features.WarrantyProvider;
using Disco.Services.Users;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Disco.Web.Models.Job
{
    public class LogWarrantyModel
    {
        public Disco.Models.Repository.Job Job { get; set; }
        public List<PluginFeatureManifest> WarrantyProviders { get; set; }
        public PluginFeatureManifest WarrantyProvider { get; set; }
        public List<Disco.Models.BI.Config.OrganisationAddress> OrganisationAddresses { get; set; }
        public Disco.Models.BI.Config.OrganisationAddress OrganisationAddress { get; set; }

        public List<Disco.Models.Repository.JobAttachment> PublishAttachments { get; set; }

        public Disco.Models.Repository.User TechUser { get; set; }

        [Required]
        public int JobId { get; set; }
        [Required(ErrorMessage = "Please specify a Repair Address")]
        public Nullable<int> OrganisationAddressId { get; set; }
        [Required(ErrorMessage = "Please specify a Warranty Provider")]
        public string WarrantyProviderId { get; set; }
        [Required(ErrorMessage = "A fault description is required"), DataType(System.ComponentModel.DataAnnotations.DataType.MultilineText)]
        public string FaultDescription { get; set; }
        public List<int> PublishAttachmentIds { get; set; }
        [Required]
        public string SubmissionAction { get; set; }

        public bool IsManualProvider
        {
            get
            {
                return WarrantyProviderId == "MANUAL";
            }
        }
        public string ManualProviderName { get; set; }
        public string ManualProviderReference { get; set; }

        public Type WarrantyProviderSubmitJobViewType { get; set; }
        public object WarrantyProviderSubmitJobModel { get; set; }
        public string ProviderPropertiesJson { get; set; }
        public Dictionary<string, string> ProviderProperties()
        {
            Dictionary<string, string> p = default(Dictionary<string, string>);
            if (!string.IsNullOrEmpty(this.ProviderPropertiesJson))
            {
                try
                {
                    p = JsonConvert.DeserializeObject<Dictionary<string, string>>(this.ProviderPropertiesJson);
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
                // Update Job User's Details [#12]
                string jobUserId = Database.Jobs.Where(j => j.Id == JobId).Select(j => j.UserId).FirstOrDefault();
                if (jobUserId != null)
                {
                    // Ignore update errors (Most commonly when the User Id no longer exists in AD)
                    try
                    {
                        UserService.GetUser(jobUserId, Database, true);
                    } catch (Exception) {}
                }

                Job = Database.Jobs.Include("Device.DeviceModel").Include("JobMetaWarranty").Include("JobSubTypes").Include("JobAttachments")
                    .Where(j => j.Id == JobId)
                    .FirstOrDefault();

                if (Job == null)
                    throw new ArgumentException("Invalid Job Number Specified", "JobId");
            }

            // Update TechUser's Details [#12]
            this.TechUser = UserService.GetUser(UserService.CurrentUserId, Database, true);

            WarrantyProviders = Plugins.GetPluginFeatures(typeof(WarrantyProviderFeature));

            if (!IsPostBack && string.IsNullOrEmpty(WarrantyProviderId))
            {
                WarrantyProviderId = Job.Device.DeviceModel.DefaultWarrantyProvider;

                if (string.IsNullOrEmpty(WarrantyProviderId))
                    WarrantyProviderId = "MANUAL";
            }

            if (!string.IsNullOrEmpty(WarrantyProviderId) && WarrantyProviderId != "MANUAL")
                WarrantyProvider = Plugins.GetPluginFeature(WarrantyProviderId, typeof(WarrantyProviderFeature));

            this.OrganisationAddresses = Database.DiscoConfiguration.OrganisationAddresses.Addresses.OrderBy(a => a.Name).ToList();

            if (!IsPostBack && !this.OrganisationAddressId.HasValue)
            {
                OrganisationAddressId = Job.Device.DeviceProfile.DefaultOrganisationAddress;
            }
            if (this.OrganisationAddressId.HasValue)
                this.OrganisationAddress = this.OrganisationAddresses.FirstOrDefault(oa => oa.Id == this.OrganisationAddressId.Value);

            if (!string.IsNullOrEmpty(FaultDescription))
                FaultDescription = FaultDescription.Trim();

            if (PublishAttachmentIds == null)
            {
                PublishAttachmentIds = new List<int>();
                PublishAttachments = new List<Disco.Models.Repository.JobAttachment>();
            }
            else
            {
                PublishAttachments = Job.JobAttachments.Where(ja => PublishAttachmentIds.Contains(ja.Id)).ToList();
            }
        }
    }
}