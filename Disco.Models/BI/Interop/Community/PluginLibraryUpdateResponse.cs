using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Models.BI.Interop.Community
{
    public class PluginLibraryUpdateResponse
    {
        public DateTime ResponseTimestamp { get; set; }
        public List<PluginLibraryItem> Plugins { get; set; }
    }
}
