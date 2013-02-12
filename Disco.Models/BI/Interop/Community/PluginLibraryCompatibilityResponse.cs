using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Models.BI.Interop.Community
{
    public class PluginLibraryCompatibilityResponse
    {
        public string HostVersion { get; set; }
        public DateTime ResponseTimestamp { get; set; }
        public List<PluginLibraryCompatibilityItem> Plugins { get; set; }
    }
}
