using System.Collections.Generic;

namespace Disco.Models.ClientServices.EnrolmentInformation
{
    public class DeviceHardware
    {
        public string SerialNumber { get; set; }
        public string UUID { get; set; }

        public string Manufacturer { get; set; }
        public string Model { get; set; }
        public string ModelType { get; set; }

        public List<Processor> Processors { get; set; }
        public List<PhysicalMemory> PhysicalMemory { get; set; }
        public List<DiskDrive> DiskDrives { get; set; }
        public List<NetworkAdapter> NetworkAdapters { get; set; }

    }
}
