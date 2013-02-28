using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Disco.Models.ClientServices
{
    public class EnrolResponse
    {
        public string SessionId { get; set; }

        public string DeviceDomainName { get; set; }
        public string DeviceComputerName { get; set; }
        
        public string DeviceAssignedUserDomain { get; set; }
        public string DeviceAssignedUserName { get; set; }
        public string DeviceAssignedUserSID { get; set; }
        public string DeviceAssignedUserUsername { get; set; }
        
        public string OfflineDomainJoin { get; set; }
        
        public string DeviceCertificate { get; set; }
        public List<string> DeviceCertificateRemoveExisting { get; set; }

        // Actions
        public bool AllowBootstrapperUninstall { get; set; }
        public bool RequireReboot { get; set; }

        public string ErrorMessage { get; set; }
    }
}
