using Disco.Services.Extensions;
using System;
using System.ComponentModel.DataAnnotations;

namespace Disco.Services.Devices.Importing.Google
{
    internal class GoogleCsvDeviceRecord : Csv.CsvRecord
    {
        [Display(Name = "serialNumber")]
        public string SerialNumber { get; set; }

        [Display(Name = "model")]
        public string Model { get; set; }

        [Display(Name = "lastPolicySync")]
        public DateTime? LastPolicySync { get; set; }

        [Display(Name = "annotatedUser")]
        public string AnnotatedUser { get; set; }

        [Display(Name = "mostRecentUser")]
        public string MostRecentUser { get; set; }

        [Display(Name = "annotatedLocation")]
        public string AnnotatedLocation { get; set; }

        [Display(Name = "ethernetMacAddress")]
        public string EthernetMacAddress { get; set; }

        [Display(Name = "macAddress")]
        public string MacAddress { get; set; }
    }
}
