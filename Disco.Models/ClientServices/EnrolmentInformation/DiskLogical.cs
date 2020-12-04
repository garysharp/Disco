namespace Disco.Models.ClientServices.EnrolmentInformation
{
    public class DiskLogical
    {
        public string DeviceID { get; set; }
        public string Description { get; set; }
        public string DriveType { get; set; }
        public string MediaType { get; set; }
        public string FileSystem { get; set; }
        public ulong? Size { get; set; }
        public ulong? FreeSpace { get; set; }
        public string VolumeName { get; set; }
        public string VolumeSerialNumber { get; set; }
    }
}
