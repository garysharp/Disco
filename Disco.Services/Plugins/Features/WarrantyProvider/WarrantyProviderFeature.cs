using Disco.Data.Repository;
using Disco.Models.BI.Config;
using Disco.Models.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Disco.Services.Plugins.Features.WarrantyProvider
{
    [PluginFeatureCategory(DisplayName = "Warranty Providers")]
    public abstract class WarrantyProviderFeature : PluginFeature
    {
        // Warranty Plugin Requirements
        public abstract string WarrantyProviderId { get; }
        public abstract Type SubmitJobViewType { get; }
        public abstract dynamic SubmitJobViewModel(DiscoDataContext Database, Controller controller, Job Job, OrganisationAddress Address, User TechUser);
        public abstract Dictionary<string, string> SubmitJobParseProperties(DiscoDataContext Database, FormCollection form, Controller controller, Job Job, OrganisationAddress Address, User TechUser, string FaultDescription);
        public abstract Dictionary<string, string> SubmitJobDiscloseInfo(DiscoDataContext Database, Job Job, OrganisationAddress Address, User TechUser, string FaultDescription, Dictionary<string, string> WarrantyProviderProperties);
        public abstract string SubmitJob(DiscoDataContext Database, Job Job, OrganisationAddress Address, User TechUser, string FaultDescription, Dictionary<string, string> WarrantyProviderProperties);

        public abstract Type JobDetailsViewType { get; }
        public bool JobDetailsSupported { get { return JobDetailsViewType != null; } }
        public abstract dynamic JobDetailsViewModel(DiscoDataContext Database, Controller controller, Job Job);

        public static PluginFeatureManifest FindPluginFeature(string PluginIdOrWarrantyProviderId)
        {
            var defs = Plugins.GetPluginFeatures(typeof(WarrantyProviderFeature));
            var def = defs.FirstOrDefault(d => d.PluginManifest.Id.Equals(PluginIdOrWarrantyProviderId, StringComparison.OrdinalIgnoreCase));
            if (def != null)
                return def;
            else
                foreach (var d in defs)
                {
                    using (var providerInstance = d.CreateInstance<WarrantyProviderFeature>())
                    {
                        if (providerInstance.WarrantyProviderId != null && providerInstance.WarrantyProviderId.Equals(PluginIdOrWarrantyProviderId, StringComparison.OrdinalIgnoreCase))
                        {
                            return d;
                        }
                    }
                }

            return null;
        }
    }
}
