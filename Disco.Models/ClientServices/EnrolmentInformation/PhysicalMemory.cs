namespace Disco.Models.ClientServices.EnrolmentInformation
{
    public class PhysicalMemory
    {
        public string Tag { get; set; }
        public string SerialNumber { get; set; }
        public string Manufacturer { get; set; }
        public string PartNumber { get; set; }

        public ulong? Capacity { get; set; }
        public uint? ConfiguredClockSpeed { get; set; }
        public uint? Speed { get; set; }

        public string DeviceLocator { get; set; }
    }
}
