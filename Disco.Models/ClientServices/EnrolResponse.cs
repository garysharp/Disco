﻿using Disco.Models.ClientServices.EnrolmentInformation;

namespace Disco.Models.ClientServices
{
    public class EnrolResponse
    {
        public string SessionId { get; set; }

        public string DomainName { get; set; }
        public string ComputerName { get; set; }

        public string AssignedUserDomain { get; set; }
        public string AssignedUserUsername { get; set; }
        public string AssignedUserSID { get; set; }
        public string AssignedUserDescription { get; set; }

        public bool AssignedUserIsLocalAdmin { get; set; }

        public string OfflineDomainJoinManifest { get; set; }

        public CertificateStore Certificates { get; set; }

        public WirelessProfileStore WirelessProfiles { get; set; }

        public bool AllowBootstrapperUninstall { get; set; }
        public bool RequireReboot { get; set; }

        public string ErrorMessage { get; set; }
    }
}
