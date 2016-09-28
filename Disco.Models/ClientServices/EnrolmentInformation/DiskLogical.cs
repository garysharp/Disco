using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Models.ClientServices.EnrolmentInformation
{
    public class DiskLogical
    {

        public string DeviceID { get; set; }
        public string Description { get; set; }
        public int DriveType { get; set; }
        public int MediaType { get; set; }
        public string FileSystem { get; set; }
        public ulong Size { get; set; }
        public ulong FreeSpace { get; set; }
        public string VolumeName { get; set; }
        public string VolumeSerialNumber { get; set; }
        
    }
}
