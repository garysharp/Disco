using System;
using System.Collections.Generic;

namespace Disco.Models.ClientServices.EnrolmentInformation
{
    public class NetworkAdapter
    {
        public string DeviceID { get; set; }
        public Guid ConnectionIdentifier { get; set; }
        public bool IsWlanAdapter { get; set; }
        public string Manufacturer { get; set; }
        public string ProductName { get; set; }
        public string AdapterType { get; set; }

        public string MACAddress { get; set; }
        public ulong Speed { get; set; }
        public string NetConnectionID { get; set; }
        public string NetConnectionStatus { get; set; }
        public string WlanStatus { get; set; }
        public bool NetEnabled { get; set; }
        public bool IPEnabled { get; set; }

        public List<string> IPAddresses { get; set; }
    }
}
