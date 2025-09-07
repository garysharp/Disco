using Disco.Models.ClientServices.EnrolmentInformation;
using Newtonsoft.Json;
using System;

namespace Disco.Models.ClientServices
{
    public class EnrolResponse
    {
        public string SessionId { get; set; }

        public string DomainName { get; set; }
        public string ComputerName { get; set; }

        [JsonIgnore]
        public int? DeviceProfileId { get; set; }
        [JsonIgnore]
        public int? DeviceBatchId { get; set; }

        public string AssignedUserDomain { get; set; }
        public string AssignedUserUsername { get; set; }
        public string AssignedUserSID { get; set; }
        public string AssignedUserDescription { get; set; }

        public bool AssignedUserIsLocalAdmin { get; set; }
        public bool SetAssignedUserForLogon { get; set; }

        public string OfflineDomainJoinManifest { get; set; }

        public CertificateStore Certificates { get; set; }

        public WirelessProfileStore WirelessProfiles { get; set; }

        public bool AllowBootstrapperUninstall { get; set; }
        public bool RequireReboot { get; set; }

        public string ErrorMessage { get; set; }

        public bool IsPending { get; set; }
        public string PendingAuthorization { get; set; }
        public string PendingReason { get; set; }
        public DateTimeOffset PendingTimeout { get; set; }
        public string PendingIdentifier { get; set; }
    }
}
