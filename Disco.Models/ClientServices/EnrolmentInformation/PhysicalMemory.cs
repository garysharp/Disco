using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Models.ClientServices.EnrolmentInformation
{
    public class PhysicalMemory
    {
        public string Tag { get; set; }
        public string SerialNumber { get; set; }
        public string Manufacturer { get; set; }
        public string PartNumber { get; set; }

        public ulong Capacity { get; set; }
        public int ClockSpeed { get; set; }
        public int Voltage { get; set; }

        public string Location { get; set; }

    }
}
