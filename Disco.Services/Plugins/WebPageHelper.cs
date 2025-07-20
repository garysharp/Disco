using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.WebPages;

namespace Disco.Services.Plugins
{
    public class WebPageHelper<T> : WebHelper
    {
        protected WebViewPage<T> ViewPage { get; set; }

        public WebPageHelper(WebViewPage<T> ViewPage, PluginManifest Manifest)
            : base(ViewPage.Context, Manifest)
        {
            this.ViewPage = ViewPage;
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

                builder.GenerateId($"form{num}");
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

        public HtmlString PartialCompiled<ViewType>(object Model) where ViewType : WebViewPage
        {
            using (StringWriter writer = new StringWriter(CultureInfo.CurrentCulture))
            {
                RenderPartialCompiled<ViewType>(writer, Model);

                return new HtmlString(writer.ToString());
            }
        }
        public HtmlString PartialCompiled<ViewType>() where ViewType : WebViewPage
        {
            return PartialCompiled<ViewType>(null);
        }
        private void RenderPartialCompiled<ViewType>(TextWriter Writer, object Model) where ViewType : WebViewPage
        {
            if (Writer == null)
                throw new ArgumentNullException("Writer");
            WebViewPage page = Activator.CreateInstance(typeof(ViewType)) as WebViewPage;
            page.ViewContext = ViewPage.ViewContext;
            page.ViewData = new ViewDataDictionary(Model);
            page.InitHelpers();
            HttpContextBase httpContext = ViewPage.ViewContext.HttpContext;
            page.ExecutePageHierarchy(new WebPageContext(httpContext, null, Model), Writer, null);
        }

        #endregion

    }
}
