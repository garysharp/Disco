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
using System.Web.Mvc.Html;
using System.Web.Routing;
using System.Web.WebPages;

namespace Disco.Services.Plugins
{
    public class WebHelper<T>
    {
        private WebViewPage<T> ViewPage { get; set; }
        public PluginManifest Manifest { get; private set; }

        public WebHelper(WebViewPage<T> ViewPage, PluginManifest manifest)
        {
            this.ViewPage = ViewPage;
            this.Manifest = manifest;
        }

        #region Html Helpers

        #region Form Helpers
        public MvcForm BeginForm(string Action, FormMethod Method, bool MultipartEncoding, IDictionary<string, object> HtmlAttributes)
        {
            if (string.IsNullOrEmpty(Action))
                throw new ArgumentNullException("PluginAction");

            var url = ActionUrl(Action);

            return BeginForm_Helper(url.ToString(), Method, MultipartEncoding, HtmlAttributes);
        }
        public MvcForm BeginForm(string Action, FormMethod Method, bool MultipartEncoding)
        {
            return BeginForm(Action, Method, MultipartEncoding, null);
        }
        public MvcForm BeginForm(string Action, FormMethod Method, IDictionary<string, object> HtmlAttributes)
        {
            return BeginForm(Action, Method, false, HtmlAttributes);
        }
        public MvcForm BeginForm(string Action, FormMethod Method)
        {
            return BeginForm(Action, Method, false, null);
        }
        public MvcForm BeginForm(string Action)
        {
            return BeginForm(Action, FormMethod.Get, false, null);
        }
        private MvcForm BeginForm_Helper(string FormAction, FormMethod Method, bool MultipartEncoding, IDictionary<string, object> HtmlAttributes)
        {
            TagBuilder builder = new TagBuilder("form");
            builder.MergeAttributes(HtmlAttributes);
            if (MultipartEncoding)
                builder.MergeAttribute("enctype", "multipart/form-data");
            builder.MergeAttribute("action", FormAction);
            builder.MergeAttribute("method", HtmlHelper.GetFormMethodString(Method), true);

            bool useClientValidation = ViewPage.ViewContext.ClientValidationEnabled && !ViewPage.ViewContext.UnobtrusiveJavaScriptEnabled;
            if (useClientValidation)
            {
                object lastFormNumber = ViewPage.ViewContext.HttpContext.Items["DiscoPluginLastFormNum"];
                int num = (lastFormNumber != null) ? (((int)lastFormNumber) + 1) : 1000;
                ViewPage.ViewContext.HttpContext.Items["DiscoPluginLastFormNum"] = num;

                builder.GenerateId(string.Format(CultureInfo.InvariantCulture, "form{0}", new object[] { num }));
            }
            ViewPage.ViewContext.Writer.Write(builder.ToString(TagRenderMode.StartTag));
            MvcForm form = new MvcForm(ViewPage.ViewContext);
            if (useClientValidation)
            {
                ViewPage.ViewContext.FormContext.FormId = builder.Attributes["id"];
            }
            return form;
        }
        #endregion

        public void IncludeStyleSheet(string Resource)
        {
            ViewPage.Context.IncludeStyleSheetResource(Resource, this.Manifest);
        }
        public void IncludeJavaScript(string Resource)
        {
            ViewPage.Context.IncludeScriptResource(Resource, this.Manifest);
        }

        public HtmlString PartialCompiled<ViewType>(object Model) where ViewType : WebViewPage
        {
            using (System.IO.StringWriter writer = new StringWriter(CultureInfo.CurrentCulture))
            {
                RenderPartialCompiled<ViewType>(writer, Model);

                return new HtmlString(writer.ToString());
            }
        }
        public HtmlString PartialCompiled<ViewType>() where ViewType : WebViewPage
        {
            return PartialCompiled<ViewType>(null);
        }
        private void RenderPartialCompiled<ViewType>(TextWriter Writer, object Model)
        {
            if (Writer == null)
                throw new ArgumentNullException("Writer");
            WebViewPage page = Activator.CreateInstance(typeof(ViewType)) as WebViewPage;
            if (page == null)
                throw new InvalidOperationException("Invalid View Type");
            page.ViewContext = ViewPage.ViewContext;
            page.ViewData = new ViewDataDictionary(Model);
            page.InitHelpers();
            HttpContextBase httpContext = ViewPage.ViewContext.HttpContext;
            page.ExecutePageHierarchy(new WebPageContext(httpContext, null, Model), Writer, null);
        }

        #endregion

        #region Urls

        public HtmlString ConfigurationUrl()
        {
            var url = GenerateUrl("Config_Plugins_Configure", new Dictionary<string, object>() { { "PluginId", Manifest.Id } });

            return new HtmlString(url);
        }

        public HtmlString ActionUrl(string Action)
        {
            var url = GenerateUrl("Plugin", new Dictionary<string, object>() { { "PluginId", Manifest.Id }, { "PluginAction", Action } });

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
            return UrlHelper.GenerateUrl(RouteName, null, null, RouteValues, RouteTable.Routes, ViewPage.Request.RequestContext, false);
        }
        private string GenerateUrl(string RouteName, IDictionary<string, object> RouteValues)
        {
            return GenerateUrl(RouteName, new RouteValueDictionary(RouteValues));
        }
        #endregion
    }
}
