using System;

namespace Disco.Services.Interop.DNS
{
    public class TxtDnsRecord : DnsRecord
    {
        public TxtDnsRecord(string name, TimeSpan timeToLive, string text)
            : base(name, DnsRecordType.Txt, timeToLive, text)
        {
        }
    }
}
