using System.Web;

namespace Disco.Services.Web.Bundles
{
    internal sealed class BundleHandler : IHttpHandler
    {
        public IBundle RequestBundle { get; private set; }
        public string BundleVirtualPath { get; private set; }

        public BundleHandler(IBundle requestBundle, string bundleVirtualPath)
        {
            RequestBundle = requestBundle;
            BundleVirtualPath = bundleVirtualPath;
        }

        public bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext context)
        {
            context.Response.Clear();

            if (!string.IsNullOrEmpty(context.Request.Headers["If-Modified-Since"]))
                context.Response.StatusCode = 304;
            else
                RequestBundle.ProcessRequest(context);
        }

        internal static bool RemapHandlerForBundleRequests(HttpApplication app)
        {
            var context = app.Context;

            string bundleUrlFromContext = context.Request.AppRelativeCurrentExecutionFilePath + context.Request.PathInfo;
            var bundle = BundleTable.GetBundleFor(bundleUrlFromContext);

            if (bundle != null)
            {
                context.RemapHandler(new BundleHandler(bundle, bundleUrlFromContext));
                return true;
            }

            return false;
        }
    }
}
