using System.Collections.Generic;

namespace Disco.Models.ClientServices.EnrolmentInformation
{
    public class CertificateStore
    {
        public List<byte[]> TrustedRootCertificates { get; set; }
        public List<string> TrustedRootRemoveThumbprints { get; set; }

        public List<byte[]> IntermediateCertificates { get; set; }
        public List<string> IntermediateRemoveThumbprints { get; set; }

        public List<byte[]> PersonalCertificates { get; set; }
        public List<string> PersonalRemoveThumbprints { get; set; }
    }
}
