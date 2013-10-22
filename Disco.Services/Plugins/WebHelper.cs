using Disco.Services.Web.Bundles;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Disco.Services.Plugins
{
    public class WebHelper
    {
        protected HttpContextBase Context { get; set; }
        public PluginManifest Manifest { get; private set; }

        public WebHelper(HttpContextBase Context, PluginManifest Manifest)
        {
            this.Context = Context;
            this.Manifest = Manifest;
        }

        public void IncludeStyleSheet(string Resource)
        {
            Context.IncludeStyleSheetResource(Resource, this.Manifest);
        }
        public void IncludeJavaScript(string Resource)
        {
            Context.IncludeScriptResource(Resource, this.Manifest);
        }

        #region Urls

        public HtmlString ConfigurationUrl()
        {
            var url = GenerateUrl("Config_Plugins_Configure", new Dictionary<string, object>() { { "PluginId", Manifest.Id } });

            return new HtmlString(url);
        }

        public HtmlString ActionUrl(string Action)
        {
            var url = Manifest.WebActionUrl(Action);

            return new HtmlString(url);
        }

        public HtmlString ResourceUrl(string Resource)
        {
            return ResourceUrl(Resource, false);
        }

        public HtmlString ResourceUrl(string Resource, bool Download)
        {
            var url = Manifest.WebResourceUrl(Resource);

            if (Download)
                url += "&Download=true";

            return new HtmlString(url);
        }

        #endregion

        #region Helpers
        private string GenerateUrl(string RouteName, RouteValueDictionary RouteValues)
        {
            return UrlHelper.GenerateUrl(RouteName, null, null, RouteValues, RouteTable.Routes, Context.Request.RequestContext, false);
        }
        private string GenerateUrl(string RouteName, IDictionary<string, object> RouteValues)
        {
            return GenerateUrl(RouteName, new RouteValueDictionary(RouteValues));
        }
        #endregion
    }
}
