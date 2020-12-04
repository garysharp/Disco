namespace Disco.Models.ClientServices.EnrolmentInformation
{
    public class DiskDrivePartition
    {
        public string DeviceID { get; set; }
        public bool? Bootable { get; set; }
        public bool? BootPartition { get; set; }
        public bool? PrimaryParition { get; set; }
        public ulong? Size { get; set; }
        public ulong? StartingOffset { get; set; }
        public string Type { get; set; }

        public DiskLogical LogicalDisk { get; set; }
    }
}
