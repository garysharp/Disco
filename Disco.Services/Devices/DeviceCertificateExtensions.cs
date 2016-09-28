using Disco.Data.Repository;
using Disco.Models.ClientServices;
using Disco.Models.ClientServices.EnrolmentInformation;
using Disco.Models.Repository;
using Disco.Services.Plugins.Features.CertificateAuthorityProvider;
using Disco.Services.Plugins.Features.CertificateProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace Disco.Services
{
    public static class DeviceCertificateExtensions
    {
        public static CertificateStore ProvisionCertificates(this Device device, DiscoDataContext Database, Enrol Enrolment, out List<DeviceCertificate> ProvisionedCertificates)
        {
            var personalCertificates = new List<byte[]>();
            var personalCertificatesRemove = new List<string>();

            var intermediateCertificates = new List<byte[]>();
            var intermediateCertificatesRemove = new List<string>();

            var trustedRootCertificates = new List<byte[]>();
            var trustedRootCertificatesRemove = new List<string>();

            ProvisionedCertificates = new List<DeviceCertificate>();

            foreach (var pluginFeature in device.DeviceProfile.GetCertificateProviders())
            {
                using (var providerFeature = pluginFeature.CreateInstance<CertificateProviderFeature>())
                {
                    var personalResult = providerFeature.ProvisionPersonalCertificate(Database, device, Enrolment);

                    if (personalResult != null)
                    {
                        if (personalResult.AllocatedCertificates != null && personalResult.AllocatedCertificates.Count > 0)
                        {
                            // Avoid transporting certificates if they are already installed
                            foreach (var certificate in personalResult.AllocatedCertificates)
                            {
                                var x509Certificate = new X509Certificate2(certificate.Content, "password");
                                if (!Enrolment.Certificates.Any(c => c.Thumbprint.Equals(x509Certificate.Thumbprint, StringComparison.OrdinalIgnoreCase)))
                                {
                                    personalCertificates.Add(certificate.Content);
                                    ProvisionedCertificates.Add(certificate);
                                }
                            }
                        }
                        if (personalResult.RemoveCertificateThumbprints != null && personalResult.RemoveCertificateThumbprints.Count > 0)
                        {
                            personalCertificatesRemove.AddRange(personalResult.RemoveCertificateThumbprints);
                        }
                    }
                }
            }

            foreach (var pluginFeature in device.DeviceProfile.GetCertificateAuthorityProviders())
            {
                using (var providerFeature = pluginFeature.CreateInstance<CertificateAuthorityProviderFeature>())
                {
                    var caResult = providerFeature.ProvisionAuthorityCertificates(Database, device, Enrolment);

                    if (caResult.TrustedRootCertificates != null && caResult.TrustedRootCertificates.Count > 0)
                    {
                        trustedRootCertificates.AddRange(caResult.TrustedRootCertificates);
                    }
                    if (caResult.TrustedRootCertificateRemoveThumbprints != null && caResult.TrustedRootCertificateRemoveThumbprints.Count > 0)
                    {
                        trustedRootCertificatesRemove.AddRange(caResult.TrustedRootCertificateRemoveThumbprints);
                    }
                    if (caResult.IntermediateCertificates != null && caResult.IntermediateCertificates.Count > 0)
                    {
                        intermediateCertificates.AddRange(caResult.IntermediateCertificates);
                    }
                    if (caResult.IntermediateCertificateRemoveThumbprints != null && caResult.IntermediateCertificateRemoveThumbprints.Count > 0)
                    {
                        intermediateCertificatesRemove.AddRange(caResult.IntermediateCertificateRemoveThumbprints);
                    }
                }
            }

            if (personalCertificates.Count == 0 && personalCertificatesRemove.Count == 0 &&
                intermediateCertificates.Count == 0 && intermediateCertificatesRemove.Count == 0 &&
                trustedRootCertificates.Count == 0 && trustedRootCertificatesRemove.Count == 0)
            {
                return null;
            }
            else
            {
                return new CertificateStore()
                {
                    TrustedRootCertificates = trustedRootCertificates.Count > 0 ? trustedRootCertificates : null,
                    TrustedRootRemoveThumbprints = trustedRootCertificatesRemove.Count > 0 ? trustedRootCertificatesRemove : null,

                    IntermediateCertificates = intermediateCertificates.Count > 0 ? intermediateCertificates : null,
                    IntermediateRemoveThumbprints = intermediateCertificatesRemove.Count > 0 ? intermediateCertificatesRemove : null,

                    PersonalCertificates = personalCertificates.Count > 0 ? personalCertificates : null,
                    PersonalRemoveThumbprints = personalCertificatesRemove.Count > 0 ? personalCertificatesRemove : null
                };
            }
        }

        public static DateTime? CertificateExpirationDate(this DeviceCertificate wc)
        {
            if (wc.Content == null || wc.Content.Length == 0)
            {
                return null;
            }
            var c = new X509Certificate2(wc.Content, "password");
            return c.NotAfter;
        }

    }
}
