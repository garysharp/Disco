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
        private HtmlString _resourceUrl;
        private bool _placeInPageHead;

        public PluginResourceScriptResult(PluginFeatureManifest Source, string Resource, bool PlaceInPageHead)
            : base(Source)
        {
            this._resource = Resource;
            this._resourceUrl = HttpContext.Current.Request.RequestContext.DiscoPluginResourceUrl(Resource, false, Source.PluginManifest);
            this._placeInPageHead = PlaceInPageHead;

            if (this._placeInPageHead)
            {
                var deferredBundles = HttpContext.Current.Items["Bundles.UIExtensionScripts"] as List<HtmlString>;
                if (deferredBundles == null)
                {
                    deferredBundles = new List<HtmlString>();
                    HttpContext.Current.Items["Bundles.UIExtensionScripts"] = deferredBundles;
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
