using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Disco.Models.ClientServices
{
    public class Enrol : ServiceBase<EnrolResponse>
    {
        public override string Feature
        {
            get { return "Enrol"; }
        }

        public string DeviceSerialNumber { get; set; }
        public string DeviceUUID { get; set; }

        public string DeviceComputerName { get; set; }
        public bool DeviceIsPartOfDomain { get; set; }
        
        public string DeviceManufacturer { get; set; }
        public string DeviceModel { get; set; }
        public string DeviceModelType { get; set; }

        public string DeviceLanMacAddress { get; set; }
        
        public string DeviceWlanMacAddress { get; set; }

        public List<string> DeviceCertificates { get; set; }
    }
}
