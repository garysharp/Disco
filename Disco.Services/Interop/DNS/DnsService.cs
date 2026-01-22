using System;
using System.Collections.Generic;

namespace Disco.Services.Interop.DNS
{
    public class DnsService
    {
        public DnsService()
        {
        }

        public static List<T> Query<T>(string name, bool bypassCache = false) where T : DnsRecord
        {
            DnsRecordType recordType;
            if (typeof(T) == typeof(ADnsRecord))
                recordType = DnsRecordType.A;
            else if (typeof(T) == typeof(CnameDnsRecord))
                recordType = DnsRecordType.Cname;
            else if (typeof(T) == typeof(TxtDnsRecord))
                recordType = DnsRecordType.Txt;
            else if (typeof(T) == typeof(SrvDnsRecord))
                recordType = DnsRecordType.Srv;
            else
                throw new NotSupportedException($"Unsupported DNS record type: {typeof(T).Name}");
            var records = NativeDns.QueryRecords(recordType, name, bypassCache);
            return records.ConvertAll(r => (T)r);
        }

    }
}
