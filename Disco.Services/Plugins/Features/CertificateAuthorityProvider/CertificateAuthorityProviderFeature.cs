using Disco.Data.Repository;
using Disco.Models.ClientServices;
using Disco.Models.Repository;

namespace Disco.Services.Plugins.Features.CertificateAuthorityProvider
{
    [PluginFeatureCategory(DisplayName = "Certificate Authority Providers")]
    public abstract class CertificateAuthorityProviderFeature : PluginFeature
    {
        public abstract string CertificateProviderId { get; }

        public abstract ProvisionAuthorityCertificatesResult ProvisionAuthorityCertificates(DiscoDataContext Database, Device Device, Enrol Enrolment);
    }
}
