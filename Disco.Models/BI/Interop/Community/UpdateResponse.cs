using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Models.BI.Interop.Community
{
    public class UpdateResponse
    {
        public string Version { get; set; }
        public DateTime VersionReleasedTimestamp { get; set; }
        public string Blurb { get; set; }
        public string UrlLink { get; set; }
        public DateTime ResponseTimestamp { get; set; }
        public bool BetaRelease { get; set; }

        public bool IsUpdatable(Version TestVersion)
        {
            var updateVersion = System.Version.Parse(this.Version);
            return (updateVersion > TestVersion);
        }
    }
}
