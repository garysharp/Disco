using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Models.ClientServices.EnrolmentInformation
{
    public class Processor
    {
        public string DeviceID { get; set; }
        public string Manufacturer { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Architecture { get; set; }
        public short Family { get; set; }
        public int MaxClockSpeed { get; set; }
        public int NumberOfCores { get; set; }
        public int NumberOfLogicalProcessors { get; set; }
    }
}
