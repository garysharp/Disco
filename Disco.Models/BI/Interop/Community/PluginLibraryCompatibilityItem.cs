using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Models.BI.Interop.Community
{
    public class PluginLibraryCompatibilityItem
    {
        public string Id { get; set; }
        public string Version { get; set; }
        public bool Compatible { get; set; }
        public string Reason { get; set; }
    }
}
