using System;

namespace Disco.Services.Interop.DNS
{
    public abstract class DnsRecord
    {
        public string Name { get; }
        public DnsRecordType Type { get; }
        public TimeSpan TimeToLive { get; }
        public string Content { get; }

        protected DnsRecord(string name, DnsRecordType type, TimeSpan timeToLive, string content)
        {
            Name = name;
            Type = type;
            TimeToLive = timeToLive;
            Content = content;
        }
    }
}
