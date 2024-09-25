using Disco.Data.Repository;
using Disco.Models.BI.Config;
using Disco.Models.Repository;
using System;
using System.Collections.Generic;

namespace Disco.Services.Plugins.Features.WarrantyProvider
{
    [PluginFeatureCategory(DisplayName = "Warranty Providers")]
    public abstract class WarrantyProvider2Feature : WarrantyProviderFeature
    {
        public override sealed string SubmitJob(DiscoDataContext Database, Job Job, OrganisationAddress Address, User TechUser, string FaultDescription, Dictionary<string, string> WarrantyProviderProperties)
        {
            throw new NotSupportedException();
        }

        public abstract string SubmitJob(DiscoDataContext database, Job job, OrganisationAddress address, User techUser, string description, List<JobAttachment> attachments, Dictionary<string, string> providerProperties);
    }
}
