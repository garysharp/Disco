using Disco.Models.ClientServices;
using Disco.Models.Repository;
using System.Collections.Generic;

namespace Disco.Services.Plugins.Features.CertificateProvider
{
    public class ProvisionPersonalCertificateResult
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
        /// A list of <see cref="DeviceCertificate"/> assigned to the device.
        /// </summary>
        public List<DeviceCertificate> AllocatedCertificates { get; set; }

        /// <summary>
        /// A list of certificate thumbprints to be removed from the Personal Certificate store.
        /// Matching certificates will be removed unless they match the <see cref="AllocatedCertificate"/>.
        /// </summary>
        public List<string> RemoveCertificateThumbprints { get; set; }

    }
}
