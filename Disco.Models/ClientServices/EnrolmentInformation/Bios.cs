using System;

namespace Disco.Models.ClientServices.EnrolmentInformation
{
    public class Bios
    {
        public string[] BIOSVersion { get; set; }
        public string Manufacturer { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public string SerialNumber { get; set; }
        public string SMBIOSBIOSVersion { get; set; }
        public ushort? SMBIOSMajorVersion { get; set; }
        public ushort? SMBIOSMinorVersion { get; set; }
        public byte? SystemBiosMajorVersion { get; set; }
        public byte? SystemBiosMinorVersion { get; set; }
    }
}
