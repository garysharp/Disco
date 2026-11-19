using Disco.Services.Extensions;
using System;
using System.ComponentModel.DataAnnotations;

namespace Disco.Services.Devices.Importing.Intune
{
    internal class IntuneCsvDeviceRecord : Csv.CsvRecord
    {
        [Display(Name = "Device ID")]
        public string DeviceId { get; set; }

        [Display(Name = "Device name")]
        public string DeviceName { get; set; }

        [Display(Name = "Enrollment date")]
        public DateTime EnrolmentDate { get; set; }

        [Display(Name = "Last check-in")]
        public DateTime LastCheckIn { get; set; }

        [Display(Name = "OS version")]
        public string OSVersion { get; set; }

        [Display(Name = "Serial number")]
        public string SerialNumber { get; set; }

        [Display(Name = "Manufacturer")]
        public string Manufacturer { get; set; }

        [Display(Name = "Model")]
        public string Model { get; set; }

        [Display(Name = "Wi-Fi MAC")]
        public string WiFiMac { get; set; }

        [Display(Name = "Total storage")]
        public long TotalStorage { get; set; }

        [Display(Name = "Free storage")]
        public long FreeStorage { get; set; }

        [Display(Name = "Primary user UPN")]
        public string PrimaryUserUpn { get; set; }

        [Display(Name = "OS")]
        public string OS { get; set; }

        [Display(Name = "SkuFamily")]
        public string SkuFamily { get; set; }

        [Display(Name = "EthernetMAC")]
        public string EthernetMac { get; set; }

        [Display(Name = "ProcessorArchitecture")]
        public string ProcessorArchitecture { get; set; }

    }
}
