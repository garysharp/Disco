using Disco.Data.Repository;
using Disco.Models.ClientServices;
using Disco.Models.Repository;

namespace Disco.Services.Plugins.Features.CertificateProvider
{
    [PluginFeatureCategory(DisplayName = "Certificate Providers")]
    public abstract class CertificateProviderFeature : PluginFeature
    {
        public abstract string CertificateProviderId { get; }

        public abstract ProvisionPersonalCertificateResult ProvisionPersonalCertificate(DiscoDataContext Database, Device Device, Enrol Enrolment);
    }
}
