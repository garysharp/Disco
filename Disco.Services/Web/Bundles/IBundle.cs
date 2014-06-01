using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Disco.Services.Web.Bundles
{
    public interface IBundle
    {
        bool RemapRequest { get; }

        string Url { get; }
        string VersionUrl { get; }

        string ContentType { get; }

        void ProcessRequest(HttpContext context);
    }
}
