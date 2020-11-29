using System.Collections.Generic;

namespace Disco.Models.ClientServices.EnrolmentInformation
{
    public class DiskDrive
    {
        public string DeviceID { get; set; }
        public string Manufacturer { get; set; }
        public string Model { get; set; }
        public string MediaType { get; set; }
        public string InterfaceType { get; set; }
        public string SerialNumber { get; set; }
        public string FirmwareRevision { get; set; }
        public ulong Size { get; set; }

        public List<DiskDrivePartition> Partitions { get; set; }
    }
}
