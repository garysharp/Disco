using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Services.Web.Bundles
{
    public class UrlBundle : IBundle
    {
        public bool RemapRequest { get { return false; } }
        
        public string Url { get; private set; }
        public string VersionUrl { get; private set; }

        public string ContentType { get; private set; }

        public void ProcessRequest(System.Web.HttpContext context)
        {
            // Not needed for Url Bundle
            throw new NotImplementedException();
        }

        public UrlBundle(string Url, string ContentType)
        {
            this.Url = Url;
            this.VersionUrl = Url;

            this.ContentType = ContentType;
        }
    }
}
