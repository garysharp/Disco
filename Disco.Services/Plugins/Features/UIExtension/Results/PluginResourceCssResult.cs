using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Disco.Services.Plugins.Features.UIExtension.Results
{
    public class PluginResourceCssResult : UIExtensionResult
    {
        private string _resource;
        private HtmlString _resourceUrl;

        public PluginResourceCssResult(PluginFeatureManifest Source, string Resource)
            : base(Source)
        {
            this._resource = Resource;
            this._resourceUrl = HttpContext.Current.Request.RequestContext.DiscoPluginResourceUrl(Resource, false, Source.PluginManifest);

            var deferredBundles = HttpContext.Current.Items["Bundles.UIExtensionCss"] as List<HtmlString>;
            if (deferredBundles == null)
            {
                deferredBundles = new List<HtmlString>();
                HttpContext.Current.Items["Bundles.UIExtensionCss"] = deferredBundles;
            }
            if (!deferredBundles.Contains(this._resourceUrl))
                deferredBundles.Add(this._resourceUrl);
        }

        public override void ExecuteResult<T>(System.Web.Mvc.WebViewPage<T> page)
        {
            // Nothing Done
        }
    }
}
