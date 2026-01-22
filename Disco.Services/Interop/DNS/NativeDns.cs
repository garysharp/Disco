using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;

namespace Disco.Services.Interop.DNS
{
    internal static class NativeDns
    {

        [DllImport("dnsapi", EntryPoint = "DnsQuery_W", CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
        private static extern int DnsQuery([MarshalAs(UnmanagedType.VBByRefStr)] ref string pszName, NativeDnsQueryTypes wType, NativeDnsQueryOptions options, int aipServers, ref IntPtr ppQueryResults, int pReserved);

        [DllImport("dnsapi", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern void DnsRecordListFree(IntPtr pRecordList, int FreeType);
        private const int DNS_ERROR_RCODE_NAME_ERROR = 0x232B;
        private const int DNS_ERROR_BAD_PACKET = 0x251E;

        public static List<DnsRecord> QueryRecords(DnsRecordType type, string name, bool bypassCache)
        {
            NativeDnsQueryTypes queryType;
            Func<IntPtr, Tuple<DnsRecord, IntPtr>> marshaller;

            switch (type)
            {
                case DnsRecordType.A:
                    queryType = NativeDnsQueryTypes.DNS_TYPE_A;
                    marshaller = MarshalARecord;
                    break;
                case DnsRecordType.Cname:
                    queryType = NativeDnsQueryTypes.DNS_TYPE_CNAME;
                    marshaller = MarshalCnameRecord;
                    break;
                case DnsRecordType.Txt:
                    queryType = NativeDnsQueryTypes.DNS_TYPE_TEXT;
                    marshaller = MarshalTxtRecord;
                    break;
                case DnsRecordType.Srv:
                    queryType = NativeDnsQueryTypes.DNS_TYPE_SRV;
                    marshaller = MarshalSrvRecord;
                    break;
                default:
                    throw new NotSupportedException($"Unsupported DNS record type: {type}");
            }

            IntPtr rrPointers = IntPtr.Zero;
            var records = new List<DnsRecord>();
            var retry = 5;
        retry:
            try
            {
                int queryResult = DnsQuery(ref name, queryType, bypassCache ? NativeDnsQueryOptions.DNS_QUERY_BYPASS_CACHE : NativeDnsQueryOptions.DNS_QUERY_STANDARD, 0, ref rrPointers, 0);
                if (queryResult != 0)
                {
                    if (queryResult == DNS_ERROR_RCODE_NAME_ERROR)
                        return records;
                    else if (queryResult == DNS_ERROR_BAD_PACKET && retry > 0)
                    {
                        // Sometimes a BAD_PACKET error is returned, retry a few times
                        Thread.Sleep(100);
                        retry--;
                        goto retry;
                    }
                    else
                        throw new Win32Exception(queryResult);
                }
                for (var rrPointer = rrPointers; !rrPointer.Equals(IntPtr.Zero);)
                {
                    var (record, rrPointerNext) = marshaller(rrPointer);
                    records.Add(record);
                    rrPointer = rrPointerNext;
                }
            }
            finally
            {
                if (rrPointers != IntPtr.Zero)
                    DnsRecordListFree(rrPointers, 0);
            }
            return records;
        }

        private static Tuple<DnsRecord, IntPtr> MarshalARecord(IntPtr pointer)
        {
            var native = Marshal.PtrToStructure<NativeDnsAData>(pointer);
            var record = new ADnsRecord(native.pName, TimeSpan.FromSeconds(native.dwTtl), native.IpAddress);
            return Tuple.Create((DnsRecord)record, native.pNext);
        }

        private static Tuple<DnsRecord, IntPtr> MarshalCnameRecord(IntPtr pointer)
        {
            var native = Marshal.PtrToStructure<NativeDnsPtrData>(pointer);
            var record = new CnameDnsRecord(native.pName, TimeSpan.FromSeconds(native.dwTtl), native.pNameHost);
            return Tuple.Create((DnsRecord)record, native.pNext);
        }

        private static Tuple<DnsRecord, IntPtr> MarshalTxtRecord(IntPtr pointer)
        {
            var native = Marshal.PtrToStructure<NativeDnsTxtData>(pointer);
            var record = new TxtDnsRecord(native.pName, TimeSpan.FromSeconds(native.dwTtl), native.pStringArray);
            return Tuple.Create((DnsRecord)record, native.pNext);
        }

        private static Tuple<DnsRecord, IntPtr> MarshalSrvRecord(IntPtr pointer)
        {
            var native = Marshal.PtrToStructure<NativeDnsSrvData>(pointer);
            var record = new SrvDnsRecord(native.pName, TimeSpan.FromSeconds(native.dwTtl), native.pNameTarget, native.wPriority, native.wWeight, native.wPort);
            return Tuple.Create((DnsRecord)record, native.pNext);
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
            DNS_TYPE_A = 0x0001,
            DNS_TYPE_CNAME = 0x0005,
            DNS_TYPE_TEXT = 0x0010,
            DNS_TYPE_SRV = 0x0021
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct NativeDnsSrvData
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

        [StructLayout(LayoutKind.Sequential)]
        private struct NativeDnsTxtData
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

        [StructLayout(LayoutKind.Sequential)]
        private struct NativeDnsPtrData
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
            public string pNameHost;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct NativeDnsAData
        {
            public IntPtr pNext;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pName;
            public ushort wType;
            public ushort wDataLength;
            public int flags;
            public int dwTtl;
            public int dwReserved;
            public uint IpAddress;
        }

    }
}
