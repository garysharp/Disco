namespace Disco.Models.ClientServices.EnrolmentInformation
{
    public class ComputerSystem
    {
        public string ChassisSKUNumber { get; set; }
        public short? CurrentTimeZone { get; set;}
        public string Description { get; set; }
        public string[] OEMStringArray { get; set; }
        public string PCSystemType { get; set; }
        public string PrimaryOwnerContact { get; set; }
        public string PrimaryOwnerName { get; set; }
        public string[] Roles { get; set; }
        public string SystemSKUNumber { get; set; }
        public string SystemType { get; set; }
    }
}
