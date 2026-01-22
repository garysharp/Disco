using System;

namespace Disco.Services.Interop.DNS
{
    public class CnameDnsRecord : DnsRecord
    {
        public CnameDnsRecord(string name, TimeSpan timeToLive, string canonicalName)
            : base(name, DnsRecordType.Cname, timeToLive, canonicalName)
        {
        }
    }
}
