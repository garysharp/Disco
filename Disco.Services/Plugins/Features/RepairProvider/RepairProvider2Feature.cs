using Disco.Data.Repository;
using Disco.Models.BI.Config;
using Disco.Models.Repository;
using System;
using System.Collections.Generic;

namespace Disco.Services.Plugins.Features.RepairProvider
{
    [PluginFeatureCategory(DisplayName = "Repair Providers")]
    public abstract class RepairProvider2Feature : RepairProviderFeature
    {
        public override sealed string SubmitJob(DiscoDataContext Database, Job Job, OrganisationAddress Address, User TechUser, string RepairDescription, Dictionary<string, string> ProviderProperties)
        {
            throw new NotSupportedException();
        }

        public abstract string SubmitJob(DiscoDataContext database, Job job, OrganisationAddress address, User techUser, string description, List<JobAttachment> attachments, Dictionary<string, string> providerProperties);

    }
}
