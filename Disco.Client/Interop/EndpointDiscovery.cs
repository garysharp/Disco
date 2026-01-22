using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Threading;

namespace Disco.Client.Interop
{
    internal class EndpointDiscovery
    {
        [DllImport("dnsapi", EntryPoint = "DnsQuery_W", CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
        private static extern int DnsQuery([MarshalAs(UnmanagedType.VBByRefStr)] ref string pszName, NativeDnsQueryTypes wType, NativeDnsQueryOptions options, int aipServers, ref IntPtr ppQueryResults, int pReserved);

        [DllImport("dnsapi", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern void DnsRecordListFree(IntPtr pRecordList, int FreeType);
        private const int DNS_ERROR_RCODE_NAME_ERROR = 0x232B;
        private const int DNS_ERROR_BAD_PACKET = 0x251E;
        public static Tuple<Uri, string> DiscoverServer(Uri forcedServerUri)
        {
            // 1. Check first command line argument for server name
            if (forcedServerUri != null)
                return Tuple.Create(forcedServerUri, "Manual");

            // 2. Check for a DNS SRV record for _discoict._tcp.domain
            var domainSuffixes = new List<string>();
            var primaryDomain = IPGlobalProperties.GetIPGlobalProperties().DomainName;
            if (!string.IsNullOrEmpty(primaryDomain))
                domainSuffixes.Add(primaryDomain);
            var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces()
                .Where(ni => ni.OperationalStatus == OperationalStatus.Up);
            foreach (var ni in networkInterfaces)
            {
                var domainSuffix = ni.GetIPProperties().DnsSuffix;
                if (!string.IsNullOrWhiteSpace(domainSuffix))
                {
                    if (domainSuffix.Equals("mshome.net", StringComparison.OrdinalIgnoreCase))
                        continue;

                    if (!domainSuffixes.Contains(domainSuffix, StringComparer.OrdinalIgnoreCase))
                        domainSuffixes.Add(domainSuffix);
                }
            }
            foreach (var domain in domainSuffixes)
            {
                var dnsRecords = GetSRVRecords("_discoict._tcp." + domain);
                if (dnsRecords.Count > 0)
                {
                    var firstRecord = dnsRecords.OrderBy(r => r.Priority).ThenByDescending(r => r.Weight).First();
                    if (firstRecord.Port == 443)
                        return Tuple.Create(new Uri($"https://{firstRecord.Target}"), "SRV");
                    else
                        return Tuple.Create(new Uri($"https://{firstRecord.Target}:{firstRecord.Port}"), "SRV");
                }
            }

            // 3. Detect VicSmart network and try resolving with Disco ICT Online Services
            if (TryResolveVicSmartServer(domainSuffixes, out var vicSmartServerUrl))
                return Tuple.Create(vicSmartServerUrl, "VicSmart");

            // 4. Legacy: Ping 'disco' and assume port 9292
            using (Ping p = new Ping())
            {
                try
                {
                    PingReply pr = p.Send("disco", 2000);
                    if (pr.Status == IPStatus.Success)
                        return Tuple.Create(new Uri("http://disco:9292"), "Legacy");
                }
                catch (Exception)
                {
                }
            }
            throw new Exception("Could not locate Disco ICT server on the network.");
        }

        private static bool TryResolveVicSmartServer(List<string> domainSuffixes, out Uri serverUrl)
        {
            if (IsVicSmartNetwork(domainSuffixes))
            {
                var potentialVicSmartAddresses = NetworkInterface.GetAllNetworkInterfaces()
                    .Where(ni => ni.OperationalStatus == OperationalStatus.Up)
                    .SelectMany(ni => ni.GetIPProperties().UnicastAddresses)
                    .Where(ua => ua.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    .Select(ua => ua.Address.GetAddressBytes())
                    .Where(a => a[0] == 10)
                    .Select(a => (ushort)((a[1] >> 4) & 0x000F) | ((a[1] << 4) & 0x00F0) | ((a[2] << 12) & 0xF000) | ((a[2] << 4) & 0x0F00))
                    .Distinct()
                    .Select(a => $"{a:x4}.vicsmart.discoict.com")
                    .ToList();

                foreach (var potentialAddress in potentialVicSmartAddresses)
                {
                    var records = GetTxtRecords(potentialAddress);

                    foreach (var record in records)
                    {
                        if (!record.Content.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                            continue;

                        if (Uri.TryCreate(record.Content, UriKind.Absolute, out var discoveredUri))
                        {
                            serverUrl = discoveredUri;
                            return true;
                        }
                    }
                }
            }

            serverUrl = null;
            return false;
        }

        private static bool IsVicSmartNetwork(List<string> domainSuffixes)
        {
            if (domainSuffixes.Any(s => string.Equals("services.education.vic.gov.au", s, StringComparison.OrdinalIgnoreCase)) ||
                domainSuffixes.Any(s => string.Equals("education.vic.gov.au", s, StringComparison.OrdinalIgnoreCase))
                )
                return true;

            IPHostEntry doeWanDnsEntry;
            try
            {
                doeWanDnsEntry = Dns.GetHostEntry("broadband.doe.wan");
                if (doeWanDnsEntry.AddressList.Length > 0)
                    return true;
            }
            catch (Exception)
            { }
            return false;
        }

        private static List<DnsTxtRecord> GetTxtRecords(string name)
        {
            IntPtr resourceRecordsPointer = IntPtr.Zero;
            var records = new List<DnsTxtRecord>();
            var retry = 5;
        retry:
            try
            {
                int queryResult = DnsQuery(ref name, NativeDnsQueryTypes.DNS_TYPE_TEXT, NativeDnsQueryOptions.DNS_QUERY_STANDARD, 0, ref resourceRecordsPointer, 0);
                if (queryResult != 0)
                {
                    if (queryResult == DNS_ERROR_RCODE_NAME_ERROR)
                        return records;
                    else if (queryResult == DNS_ERROR_BAD_PACKET && retry > 0)
                    {
                        // Sometimes a BAD_PACKET error is returned, retry a few times
                        Thread.Sleep(200);
                        retry--;
                        goto retry;
                    }
                    else
                        throw new Win32Exception(queryResult);
                }
                NativeDnsTxtRecord record;
                for (var resourceRecordPointer = resourceRecordsPointer; !resourceRecordPointer.Equals(IntPtr.Zero); resourceRecordPointer = record.pNext)
                {
                    record = Marshal.PtrToStructure<NativeDnsTxtRecord>(resourceRecordPointer);
                    if (record.wType == (ushort)NativeDnsQueryTypes.DNS_TYPE_TEXT)
                        records.Add(DnsTxtRecord.FromNativeRecord(record));
                }
            }
            finally
            {
                if (resourceRecordsPointer != IntPtr.Zero)
                    DnsRecordListFree(resourceRecordsPointer, 0);
            }
            return records;
        }

        private static List<DnsSrvRecord> GetSRVRecords(string name)
        {
            IntPtr resourceRecordsPointer = IntPtr.Zero;
            var records = new List<DnsSrvRecord>();
            var retry = 5;
        retry:
            try
            {
                int queryResult = DnsQuery(ref name, NativeDnsQueryTypes.DNS_TYPE_SRV, NativeDnsQueryOptions.DNS_QUERY_STANDARD, 0, ref resourceRecordsPointer, 0);
                if (queryResult != 0)
                {
                    if (queryResult == DNS_ERROR_RCODE_NAME_ERROR)
                        return records;
                    else if (queryResult == DNS_ERROR_BAD_PACKET && retry > 0)
                    {
                        // Sometimes a BAD_PACKET error is returned, retry a few times
                        Thread.Sleep(200);
                        retry--;
                        goto retry;
                    }
                    else
                        throw new Win32Exception(queryResult);
                }
                NativeDnsSrvRecord record;
                for (var resourceRecordPointer = resourceRecordsPointer; !resourceRecordPointer.Equals(IntPtr.Zero); resourceRecordPointer = record.pNext)
                {
                    record = Marshal.PtrToStructure<NativeDnsSrvRecord>(resourceRecordPointer);
                    if (record.wType == (ushort)NativeDnsQueryTypes.DNS_TYPE_SRV)
                        records.Add(DnsSrvRecord.FromNativeRecord(record));
                }
            }
            finally
            {
                if (resourceRecordsPointer != IntPtr.Zero)
                    DnsRecordListFree(resourceRecordsPointer, 0);
            }
            return records;
        }

        private enum NativeDnsQueryOptions
        {
            DNS_QUERY_ACCEPT_TRUNCATED_RESPONSE = 1,
            DNS_QUERY_BYPASS_CACHE = 8,
            DNS_QUERY_DONT_RESET_TTL_VALUES = 0x100000,
            DNS_QUERY_NO_HOSTS_FILE = 0x40,
            DNS_QUERY_NO_LOCAL_NAME = 0x20,
            DNS_QUERY_NO_NETBT = 0x80,
            DNS_QUERY_NO_RECURSION = 4,
            DNS_QUERY_NO_WIRE_QUERY = 0x10,
            DNS_QUERY_RESERVED = -16777216,
            DNS_QUERY_RETURN_MESSAGE = 0x200,
            DNS_QUERY_STANDARD = 0,
            DNS_QUERY_TREAT_AS_FQDN = 0x1000,
            DNS_QUERY_USE_TCP_ONLY = 2,
            DNS_QUERY_WIRE_ONLY = 0x100
        }

        private enum NativeDnsQueryTypes
        {
            DNS_TYPE_TEXT = 0x0010,
            DNS_TYPE_SRV = 0x0021
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct NativeDnsSrvRecord
        {
            public IntPtr pNext;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pName;
            public ushort wType;
            public ushort wDataLength;
            public int flags;
            public int dwTtl;
            public int dwReserved;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pNameTarget;
            public ushort wPriority;
            public ushort wWeight;
            public ushort wPort;
            public ushort Pad;
        }

        private class DnsSrvRecord
        {
            public string Name { get; set; }
            public int Type { get; set; }
            public int Ttl { get; set; }
            public string Target { get; set; }
            public int Priority { get; set; }
            public int Weight { get; set; }
            public int Port { get; set; }

            public static DnsSrvRecord FromNativeRecord(NativeDnsSrvRecord nativeRecord)
            {
                return new DnsSrvRecord
                {
                    Name = nativeRecord.pName,
                    Type = nativeRecord.wType,
                    Ttl = nativeRecord.dwTtl,
                    Target = nativeRecord.pNameTarget,
                    Priority = nativeRecord.wPriority,
                    Weight = nativeRecord.wWeight,
                    Port = nativeRecord.wPort
                };
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct NativeDnsTxtRecord
        {
            public IntPtr pNext;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pName;
            public ushort wType;
            public ushort wDataLength;
            public int flags;
            public int dwTtl;
            public int dwReserved;
            public uint dwStringLength;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pStringArray;
        }

        private class DnsTxtRecord
        {
            public string Name { get; set; }
            public int Type { get; set; }
            public int Ttl { get; set; }
            public string Content { get; set; }

            public static DnsTxtRecord FromNativeRecord(NativeDnsTxtRecord nativeRecord)
            {
                return new DnsTxtRecord
                {
                    Name = nativeRecord.pName,
                    Type = nativeRecord.wType,
                    Ttl = nativeRecord.dwTtl,
                    Content = nativeRecord.pStringArray,
                };
            }
        }

    }
}
