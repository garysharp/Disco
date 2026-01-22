using System;

namespace Disco.Models.UI.Config.Enrolment
{
    public interface ConfigEnrolmentIndexModel : BaseUIModel
    {
        string MacSshUsername { get; set; }
        int PendingTimeoutMinutes { get; set; }
        Uri MacEnrolUrl { get; set; }
        bool HostingPluginInstalled { get; set; }
        bool IsVicSmartDeployment { get; set; }
        bool IsServicesEducationVicGovAuDomain { get; set; }
        string DnsSrvRecordName { get; set; }
        string DnsSrvRecordValue { get; set; }
        bool LegacyDiscoveryEnabled { get; set; }
    }
}
