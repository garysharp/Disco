using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Disco.Web.Extensions.MvcExtensions.Bundles
{
    internal sealed class BundleHandler : IHttpHandler
    {
        public Bundle RequestBundle { get; private set; }
        public string BundleVirtualPath { get; private set; }
        
        public BundleHandler(Bundle requestBundle, string bundleVirtualPath)
        {
            this.RequestBundle = requestBundle;
            this.BundleVirtualPath = bundleVirtualPath;
        }

        public bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext context)
        {
            context.Response.Clear();

            if (!string.IsNullOrEmpty(context.Request.Headers["If-Modified-Since"]))
                context.Response.StatusCode = 0x130;
            else
                this.RequestBundle.ProcessRequest(context);
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
