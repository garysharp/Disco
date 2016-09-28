using Disco.Models.ClientServices.EnrolmentInformation;
using System.Collections.Generic;

namespace Disco.Models.ClientServices
{
    public class Enrol : ServiceBase<EnrolResponse>
    {
        public override string Feature
        {
            get { return "Enrol"; }
        }

        public string SerialNumber { get; set; }

        public string DNSDomainName { get; set; }
        public string ComputerName { get; set; }
        public bool IsPartOfDomain { get; set; }

        public string RunningUserName { get; set; }
        public string RunningUserDomain { get; set; }
        public bool RunningInteractively { get; internal set; }

        public DeviceHardware Hardware { get; set; }

        public List<Certificate> Certificates { get; set; }

        public List<WirelessProfile> WirelessProfiles { get; set; }
    }
}
