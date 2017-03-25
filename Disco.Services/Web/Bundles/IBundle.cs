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
