using Disco.Services.Web.Bundles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            this._resource = Resource;
            this._resourceUrl = Source.PluginManifest.WebResourceUrl(Resource);
            this._placeInPageHead = PlaceInPageHead;

            if (this._placeInPageHead)
            {
                var deferredBundles = HttpContext.Current.Items[Bundle.UIExtensionScriptsKey] as List<string>;
                if (deferredBundles == null)
                {
                    deferredBundles = new List<string>();
                    HttpContext.Current.Items[Bundle.UIExtensionScriptsKey] = deferredBundles;
                }
                if (!deferredBundles.Contains(this._resourceUrl))
                    deferredBundles.Add(this._resourceUrl);
            }
        }

        public override void ExecuteResult<T>(System.Web.Mvc.WebViewPage<T> page)
        {
            if (!this._placeInPageHead)
            {
                page.WriteLiteral("<script src=\"");
                page.WriteLiteral(_resourceUrl);
                page.WriteLiteral("\" type=\"text/javascript\"></script>");
            }
        }
    }
}
