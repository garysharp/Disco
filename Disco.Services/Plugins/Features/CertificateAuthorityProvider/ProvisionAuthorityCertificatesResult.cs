using Disco.Models.ClientServices;
using Disco.Models.Repository;
using System.Collections.Generic;

namespace Disco.Services.Plugins.Features.CertificateAuthorityProvider
{
    public class ProvisionAuthorityCertificatesResult
    {
        /// <summary>
        /// The <see cref="Device"/> associated with the provision result.
        /// </summary>
        public Device Device { get; set; }

        /// <summary>
        /// The <see cref="Enrol"/> associated with the provision result.
        /// </summary>
        public Enrol Enrolment { get; set; }

        /// <summary>
        /// A list of certificate thumbprints to be removed from the Trusted Root Certification Authorities store
        /// Matching certificates will be removed unless they match the <see cref="TrustedRootCertificates"/>.
        /// </summary>
        public List<string> TrustedRootCertificateRemoveThumbprints { get; set; }

        /// <summary>
        /// A list of certificates to be added to the Trusted Root Certification Authorities store on the client device.
        /// </summary>
        public List<byte[]> TrustedRootCertificates { get; set; }

        /// <summary>
        /// A list of certificate thumbprints to be removed from the Intermedate Certificate Authorities store.
        /// Matching certificates will be removed unless they match the <see cref="IntermediateCertificates"/>.
        /// </summary>
        public List<string> IntermediateCertificateRemoveThumbprints { get; set; }

        /// <summary>
        /// A list of certificates to be added to the Intermedate Certificate Authorities store on the client device.
        /// </summary>
        public List<byte[]> IntermediateCertificates { get; set; }
    }
}
