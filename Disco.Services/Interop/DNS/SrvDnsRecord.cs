using System;

namespace Disco.Services.Interop.DNS
{
    public class SrvDnsRecord : DnsRecord
    {
        public string Target { get; }
        public ushort Priority { get; }
        public ushort Weight { get; }
        public ushort Port { get; }

        public SrvDnsRecord(string name, TimeSpan timeToLive, string target, ushort priority, ushort weight, ushort port)
            : base(name, DnsRecordType.Srv, timeToLive, $"{priority} {weight} {port} {target}")
        {
            Target = target;
            Priority = priority;
            Weight = weight;
            Port = port;
        }
    }
}
