using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Models.BI.Device
{
    public class ImportDeviceSession
    {
        public string ImportParseTaskId { get; set; }
        public string ImportFilename { get; set; }
        public List<ImportDevice> ImportDevices { get; set; }
    }
}