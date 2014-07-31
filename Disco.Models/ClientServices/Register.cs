using System.ComponentModel.DataAnnotations;

namespace Disco.Models.ClientServices
{
    public class Register
    {
        [Required]
        public string DeviceUUID { get; set; }
        [Required]
        public string DeviceSerialNumber { get; set; }

        [Required]
        public string DeviceDNSDomainName { get; set; }
        [Required]
        public string DeviceComputerName { get; set; }
        [Required]
        public bool DeviceIsPartOfDomain { get; set; }

        [Required]
        public string DeviceManufacturer { get; set; }
        [Required]
        public string DeviceModel { get; set; }
        [Required]
        public string DeviceModelType { get; set; }
    }
}
