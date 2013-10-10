using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(Disco.Services.Web.Bundles.BundleModule), "PreApplicationStart")]

namespace Disco.Services.Web.Bundles
{
    public class BundleModule : IHttpModule
    {
        public void Init(HttpApplication context)
        {
            context.PostResolveRequestCache += new EventHandler(this.OnApplicationPostResolveRequestCache);
        }

        private void OnApplicationPostResolveRequestCache(object sender, EventArgs e)
        {
            HttpApplication app = (HttpApplication)sender;
            if (BundleTable.Count > 0)
            {
                BundleHandler.RemapHandlerForBundleRequests(app);
            }
        }

        private static bool _startWasCalled;
        public static void PreApplicationStart()
        {
            if (!_startWasCalled)
            {
                _startWasCalled = true;
                DynamicModuleUtility.RegisterModule(typeof(BundleModule));
            }
        }

        public void Dispose()
        {
            // Dispose Nothing
        }
    }
}
