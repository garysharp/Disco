using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
