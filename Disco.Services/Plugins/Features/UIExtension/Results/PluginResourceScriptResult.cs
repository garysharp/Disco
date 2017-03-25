using Disco.Services.Web.Bundles;
using System.Collections.Generic;
using System.Web;

namespace Disco.Services.Plugins.Features.UIExtension.Results
{
    public class PluginResourceScriptResult : UIExtensionResult
    {
        private string _resource;
        private string _resourceUrl;
        private bool _placeInPageHead;

        public PluginResourceScriptResult(PluginFeatureManifest Source, string Resource, bool PlaceInPageHead)
            : base(Source)
        {
            _resource = Resource;
            _resourceUrl = Source.PluginManifest.WebResourceUrl(Resource);
            _placeInPageHead = PlaceInPageHead;

            if (_placeInPageHead)
            {
                var deferredBundles = HttpContext.Current.Items[BundleTable.UIExtensionScriptsKey] as List<string>;
                if (deferredBundles == null)
                {
                    deferredBundles = new List<string>();
                    HttpContext.Current.Items[BundleTable.UIExtensionScriptsKey] = deferredBundles;
                }
                if (!deferredBundles.Contains(_resourceUrl))
                    deferredBundles.Add(_resourceUrl);
            }
        }

        public override void ExecuteResult<T>(System.Web.Mvc.WebViewPage<T> page)
        {
            if (!_placeInPageHead)
            {
                page.WriteLiteral("<script src=\"");
                page.WriteLiteral(_resourceUrl);
                page.WriteLiteral("\" type=\"text/javascript\"></script>");
            }
        }
    }
}
