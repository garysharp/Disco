using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Disco.Data.Repository;
using Disco.Models.BI.Config;
using Disco.Models.Repository;

namespace Disco.Services.Plugins.Categories.WarrantyProvider
{
    [PluginCategory(DisplayName = "Warranty Providers")]
    public abstract class WarrantyProviderPlugin : Plugin
    {
        public override sealed Type PluginCategoryType
        {
            get { return typeof(WarrantyProviderPlugin); }
        }

        // Warranty Plugin Requirements
        public abstract string WarrantyProviderId { get; }
        public abstract Type SubmitJobViewType { get; }
        public abstract dynamic SubmitJobViewModel(DiscoDataContext dbContext, Controller controller, Job Job, OrganisationAddress Address, User TechUser);
        public abstract Dictionary<string, string> SubmitJobParseProperties(DiscoDataContext dbContext, FormCollection form, Controller controller, Job Job, OrganisationAddress Address, User TechUser, string FaultDescription);
        public abstract Dictionary<string, string> SubmitJobDiscloseInfo(DiscoDataContext dbContext, Job Job, OrganisationAddress Address, User TechUser, string FaultDescription, Dictionary<string, string> WarrantyProviderProperties);
        public abstract string SubmitJob(DiscoDataContext dbContext, Job Job, OrganisationAddress Address, User TechUser, string FaultDescription, Dictionary<string, string> WarrantyProviderProperties);

        public abstract Type JobDetailsViewType { get; }
        public bool JobDetailsSupported { get { return this.JobDetailsViewType != null; } }
        public abstract dynamic JobDetailsViewModel(DiscoDataContext dbContext, Controller controller, Job Job);

        public static PluginDefinition FindPlugin(string PlugIdOrWarrantyProviderId)
        {
            var defs = Plugins.GetPlugins(typeof(WarrantyProviderPlugin));
            var def = defs.FirstOrDefault(d => d.Id.Equals(PlugIdOrWarrantyProviderId, StringComparison.InvariantCultureIgnoreCase));
            if (def != null)
                return def;
            else
                foreach (var d in defs)
                {
                    using (var providerInstance = d.CreateInstance<WarrantyProviderPlugin>())
                    {
                        if (providerInstance.WarrantyProviderId != null && providerInstance.WarrantyProviderId.Equals(PlugIdOrWarrantyProviderId, StringComparison.InvariantCultureIgnoreCase))
                        {
                            return d;
                        }
                    }
                }

            return null;
        }
    }
}
