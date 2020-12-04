namespace Disco.Models.ClientServices.EnrolmentInformation
{
    public class Processor
    {
        public string DeviceID { get; set; }
        public string Manufacturer { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Architecture { get; set; }
        public ushort? Family { get; set; }
        public uint? MaxClockSpeed { get; set; }
        public uint? NumberOfCores { get; set; }
        public uint? NumberOfLogicalProcessors { get; set; }
    }
}
