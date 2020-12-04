using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Disco.Models.Repository
{
    public class DeviceDetail
    {
        public const string ScopeHardware = "Hardware";

        public const string HardwareKeyLanMacAddress = "LanMacAddress";
        public const string HardwareKeyWLanMacAddress = "WLanMacAddress";
        public const string HardwareKeyACAdapter = "ACAdapter";
        public const string HardwareKeyBattery = "Battery";
        public const string HardwareKeyKeyboard = "Keyboard";
        public const string HardwareKeyNetworkAdapters = "NetworkAdapters";
        public const string HardwareKeyProcessors = "Processors";
        public const string HardwareKeyPhysicalMemory = "PhysicalMemory";
        public const string HardwareKeyDiskDrives = "DiskDrives";
        public const string HardwareKeyBios = "Bios";
        public const string HardwareKeyBaseBoard = "BaseBoard";
        public const string HardwareKeyComputerSystem = "ComputerSystem";
        public const string HardwareKeyBatteries = "Batteries";

        [Column(Order = 0), Key]
        public string DeviceSerialNumber { get; set; }
        
        [Key, StringLength(100), Column(Order = 2)]
        public string Key { get; set; }
        
        [Column(Order = 1), StringLength(100), Key]
        public string Scope { get; set; }
        
        public string Value { get; set; }

        [ForeignKey("DeviceSerialNumber")]
        public virtual Device Device { get; set; }
    }
}
