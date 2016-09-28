using System;

namespace Disco.Models.ClientServices.EnrolmentInformation
{
    public class Certificate
    {
        public string Store { get; set; }
        public string SubjectName { get; set; }
        public string Thumbprint { get; set; }
        public string FriendlyName { get; set; }
        public string DnsName { get; set; }
        public int Version { get; set; }
        public string SignatureAlgorithm { get; set; }
        public string Issuer { get; set; }
        public DateTime NotAfter { get; set; }
        public DateTime NotBefore { get; set; }
        public bool HasPrivateKey { get; set; }
    }
}
