using System;
using System.Net;

namespace Disco.Services.Interop.DNS
{
    public class ADnsRecord : DnsRecord
    {
        public IPAddress Address { get; }

        public ADnsRecord(string name, TimeSpan timeToLive, uint address)
            : base(name, DnsRecordType.A, timeToLive, UIntToIPAddress(address).ToString())
        {
            Address = UIntToIPAddress(address);
        }

        private static IPAddress UIntToIPAddress(uint address)
        {
            byte[] bytes = BitConverter.GetBytes(address);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            return new IPAddress(bytes);
        }
    }
}
