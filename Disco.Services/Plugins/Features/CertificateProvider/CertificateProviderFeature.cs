using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Disco.Data.Repository;
using Disco.Models.Repository;

namespace Disco.Services.Plugins.Features.CertificateProvider
{
    [PluginFeatureCategory(DisplayName = "Certificate Providers")]
    public abstract class CertificateProviderFeature : PluginFeature
    {
        // Certificate Plugin Requirements
        public abstract string CertificateProviderId { get; }
        public abstract Tuple<DeviceCertificate, List<string>> AllocateCertificate(DiscoDataContext dbContext, Device Device);
    }
}
