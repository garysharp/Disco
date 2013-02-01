using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Disco.Data.Repository;
using System.ComponentModel.DataAnnotations;
using Disco.BI;
using System.Web.Script.Serialization;
using Disco.Services.Plugins;
using Disco.Services.Plugins.Features.WarrantyProvider;

namespace Disco.Web.Models.Job
{
    public class LogWarrantyModel
    {
        public Disco.Models.Repository.Job Job { get; set; }
        public List<PluginFeatureManifest> WarrantyProviders { get; set; }
        public PluginFeatureManifest WarrantyProvider { get; set; }
        public List<Disco.Models.BI.Config.OrganisationAddress> OrganisationAddresses { get; set; }
        public Disco.Models.BI.Config.OrganisationAddress OrganisationAddress { get; set; }

        public Disco.Models.Repository.User TechUser { get; set; }

        [Required]
        public int JobId { get; set; }
        [Required(ErrorMessage = "Please specify a Repair Address")]
        public Nullable<int> OrganisationAddressId { get; set; }
        [Required(ErrorMessage = "Please specify a Warranty Provider")]
        public string WarrantyProviderId { get; set; }
        [Required(ErrorMessage = "A fault description is required"), DataType(System.ComponentModel.DataAnnotations.DataType.MultilineText)]
        public string FaultDescription { get; set; }
        [Required]
        public string WarrantyAction { get; set; }

        public Type WarrantyProviderSubmitJobViewType { get; set; }
        public object WarrantyProviderSubmitJobModel { get; set; }
        public string WarrantyProviderPropertiesJson { get; set; }
        public Dictionary<string, string> WarrantyProviderProperties()
        {
            Dictionary<string, string> p = default(Dictionary<string, string>);
            if (string.IsNullOrEmpty(this.WarrantyProviderPropertiesJson))
            {
                JavaScriptSerializer s = new JavaScriptSerializer();
                try
                {
                    p = s.Deserialize<Dictionary<string, string>>(this.WarrantyProviderPropertiesJson);
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

        public void UpdateModel(DiscoDataContext dbContext, bool IsPostBack)
        {
            dbContext.Configuration.LazyLoadingEnabled = true;
            if (Job == null)
            {
                Job = (from j in dbContext.Jobs.Include("Device.DeviceModel").Include("JobMetaWarranty").Include("JobSubTypes")
                       where (j.Id == JobId)
                       select j).FirstOrDefault();
                if (Job == null)
                {
                    throw new ArgumentException("Invalid Job Number Specified", "JobId");
                }
            }

            this.TechUser = DiscoApplication.CurrentUser;

            WarrantyProviders = Plugins.GetPluginFeatures(typeof(WarrantyProviderFeature));

            if (!IsPostBack && string.IsNullOrEmpty(WarrantyProviderId))
            {
                WarrantyProviderId = Job.Device.DeviceModel.DefaultWarrantyProvider;
            }

            if (!string.IsNullOrEmpty(WarrantyProviderId))
                WarrantyProvider = Plugins.GetPluginFeature(WarrantyProviderId, typeof(WarrantyProviderFeature));

            this.OrganisationAddresses = dbContext.DiscoConfiguration.OrganisationAddresses.Addresses;

            if (!IsPostBack && !this.OrganisationAddressId.HasValue)
            {
                OrganisationAddressId = Job.Device.DeviceProfile.DefaultOrganisationAddress;
            }
            if (this.OrganisationAddressId.HasValue)
                this.OrganisationAddress = this.OrganisationAddresses.FirstOrDefault(oa => oa.Id == this.OrganisationAddressId.Value);

            if (!string.IsNullOrEmpty(FaultDescription))
                FaultDescription = FaultDescription.Trim();
        }
    }
}