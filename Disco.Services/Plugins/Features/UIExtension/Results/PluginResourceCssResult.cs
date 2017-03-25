using Disco.Services.Web.Bundles;
using System.Collections.Generic;
using System.Web;

namespace Disco.Services.Plugins.Features.UIExtension.Results
{
    public class PluginResourceCssResult : UIExtensionResult
    {
        private string _resource;
        private string _resourceUrl;

        public PluginResourceCssResult(PluginFeatureManifest Source, string Resource)
            : base(Source)
        {
            _resource = Resource;
            _resourceUrl = Source.PluginManifest.WebResourceUrl(Resource);

            var deferredBundles = HttpContext.Current.Items[BundleTable.UIExtensionCssKey] as List<string>;
            if (deferredBundles == null)
            {
                deferredBundles = new List<string>();
                HttpContext.Current.Items[BundleTable.UIExtensionCssKey] = deferredBundles;
            }
            if (!deferredBundles.Contains(_resourceUrl))
                deferredBundles.Add(_resourceUrl);
        }

        public override void ExecuteResult<T>(System.Web.Mvc.WebViewPage<T> page)
        {
            // Nothing Done
        }
    }
}
