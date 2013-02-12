using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Models.BI.Interop.Community
{
    public class PluginLibraryItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Author { get; set; }
        public string Url { get; set; }
        public string Blurb { get; set; }

        public string LatestVersion { get; set; }
        public string LatestChangeLog { get; set; }
        public string LatestHostVersionMin { get; set; }
        public string LatestHostVersionMax { get; set; }
        public string LatestDownloadUrl { get; set; }
    }
}
