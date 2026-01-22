using Disco.Models.UI.Config.Enrolment;
using System;

namespace Disco.Web.Areas.Config.Models.Enrolment
{
    public class IndexModel : ConfigEnrolmentIndexModel
    {
        public string MacSshUsername { get; set; }
        public int PendingTimeoutMinutes { get; set; }
        public Uri MacEnrolUrl { get; set; }
        public bool HostingPluginInstalled { get; set; }
        public bool IsVicSmartDeployment { get; set; }
        public bool IsServicesEducationVicGovAuDomain { get; set; }
        public string DnsSrvRecordName { get; set; }
        public string DnsSrvRecordValue { get; set; }
        public bool LegacyDiscoveryEnabled { get; set; }
    }
}