using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Disco.Models.ClientServices
{
    public class MacEnrolResponse
    {
        public string DeviceComputerName { get; set; }
        
        public string DeviceAssignedUserDomain { get; set; }
        public string DeviceAssignedUserName { get; set; }
        public string DeviceAssignedUserSID { get; set; }
        public string DeviceAssignedUserUsername { get; set; }

        public string ErrorMessage { get; set; }
    }
}
