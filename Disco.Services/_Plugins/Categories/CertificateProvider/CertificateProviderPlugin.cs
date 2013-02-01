using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Disco.Data.Repository;
using Disco.Models.Repository;

namespace Disco.Services.Plugins.Categories.CertificateProvider
{
    [PluginCategory(DisplayName = "Certificate Providers")]
    public abstract class CertificateProviderPlugin : Plugin
    {
        public override sealed Type PluginCategoryType
        {
            get { return typeof(CertificateProviderPlugin); }
        }

        // Certificate Plugin Requirements
        public abstract string CertificateProviderId { get; }
        public abstract Tuple<DeviceCertificate, List<string>> AllocateCertificate(DiscoDataContext dbContext, Device Device);

    }
}
