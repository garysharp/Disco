using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Disco.Data.Repository;
using System.IO;
using System.Web.Mvc;
using System.Web.WebPages;
using System.Web.Routing;
using System.Web;
using System.Web.Mvc.Html;
using System.Globalization;

namespace Disco.Services.Plugins
{
    public static class PluginExtensions
    {
        #region Model Binding from Controller
        public static bool TryUpdateModel<TModel>(this Controller controller, TModel model) where TModel : class
        {
            return controller.TryUpdateModel<TModel>(model, null, controller.ValueProvider);
        }
        public static bool TryUpdateModel<TModel>(this Controller controller, TModel model, IValueProvider valueProvider) where TModel : class
        {
            return controller.TryUpdateModel<TModel>(model, null, valueProvider);
        }
        public static bool TryUpdateModel<TModel>(this Controller controller, TModel model, string prefix) where TModel : class
        {
            return controller.TryUpdateModel<TModel>(model, prefix, controller.ValueProvider);
        }
        public static bool TryUpdateModel<TModel>(this Controller controller, TModel model, string prefix, IValueProvider valueProvider) where TModel : class
        {
            if (model == null)
                throw new ArgumentNullException("model");
            if (valueProvider == null)
                throw new ArgumentNullException("valueProvider");

            Predicate<string> predicate = propertyName => true;
            IModelBinder binder = ModelBinders.Binders.GetBinder(typeof(TModel));

            ModelBindingContext context2 = new ModelBindingContext
            {
                ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => model, typeof(TModel)),
                ModelName = prefix,
                ModelState = controller.ModelState,
                PropertyFilter = predicate,
                ValueProvider = valueProvider
            };

            ModelBindingContext bindingContext = context2;

            binder.BindModel(controller.ControllerContext, bindingContext);

            return controller.ModelState.IsValid;
        }
        public static bool TryUpdateModel(this Controller controller, object model)
        {
            return controller.TryUpdateModel(model, null, controller.ValueProvider);
        }
        public static bool TryUpdateModel(this Controller controller, object model, IValueProvider valueProvider)
        {
            return controller.TryUpdateModel(model, null, valueProvider);
        }
        public static bool TryUpdateModel(this Controller controller, object model, string prefix)
        {
            return controller.TryUpdateModel(model, prefix, controller.ValueProvider);
        }
        public static bool TryUpdateModel(this Controller controller, object model, string prefix, IValueProvider valueProvider)
        {
            if (model == null)
                throw new ArgumentNullException("model");
            if (valueProvider == null)
                throw new ArgumentNullException("valueProvider");

            Predicate<string> predicate = propertyName => true;
            IModelBinder binder = ModelBinders.Binders.GetBinder(model.GetType());

            ModelBindingContext context2 = new ModelBindingContext
            {
                ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => model, model.GetType()),
                ModelName = prefix,
                ModelState = controller.ModelState,
                PropertyFilter = predicate,
                ValueProvider = valueProvider
            };

            ModelBindingContext bindingContext = context2;

            binder.BindModel(controller.ControllerContext, bindingContext);

            return controller.ModelState.IsValid;
        }
        #endregion

        #region Virtual Directories
        //public static string WebHandlerResource(this PluginManifest pluginManifest, string resourcePath, RequestContext requestContext)
        //{
        //    var rootPath = WebHandlerRootUrl(pluginManifest, requestContext);
        //    return string.Concat(rootPath, resourcePath);
        //}
        //public static string WebHandlerRootUrl(this PluginManifest pluginManifest, RequestContext requestContext)
        //{
        //    var tempPath = pluginManifest.WebHandlerActionUrl(requestContext, "_");
        //    return tempPath.Substring(0, tempPath.LastIndexOf(@"/") + 1);
        //}
        //public static string WebHandlerActionUrl(this PluginManifest pluginManifest, RequestContext requestContext, string PluginAction)
        //{
        //    var routeValues = new RouteValueDictionary(new { PluginId = pluginManifest.Id, PluginAction = PluginAction });
        //    return UrlHelper.GenerateUrl("Plugin", "PluginWebHandler", "Index", routeValues, RouteTable.Routes, requestContext, true);
        //}
        //public static string WebHandlerResourceUrl(this PluginManifest pluginManifest, RequestContext requestContext, string PluginAction)
        //{
        //    var routeValues = new RouteValueDictionary(new { PluginId = pluginManifest.Id, PluginAction = PluginAction });



        //    return UrlHelper.GenerateUrl("Plugin", "PluginWebHandler", "Index", routeValues, RouteTable.Routes, requestContext, true);
        //}

        public static HtmlString DiscoPluginResourceUrl<T>(this WebViewPage<T> ViewPage, string Resource)
        {
            return ViewPage.DiscoPluginResourceUrl(Resource, false);
        }
        public static HtmlString DiscoPluginResourceUrl<T>(this WebViewPage<T> ViewPage, string Resource, bool Download)
        {
            if (string.IsNullOrEmpty(Resource))
                throw new ArgumentNullException("Resource");

            // Find Plugin
            var pageType = ViewPage.GetType();
            var pageAssembly = pageType.Assembly;
            var manifest = Plugins.GetPlugin(pageAssembly);

            return ViewPage.DiscoPluginResourceUrl(Resource, false, manifest);
        }
        public static HtmlString DiscoPluginResourceUrl<T>(this WebViewPage<T> ViewPage, string Resource, bool Download, PluginManifest manifest)
        {
            return ViewPage.ViewContext.RequestContext.DiscoPluginResourceUrl(Resource, Download, manifest);
        }
        public static HtmlString DiscoPluginResourceUrl(this RequestContext RequestContext, string Resource, bool Download, PluginManifest manifest)
        {
            var resourcePath = manifest.WebResourcePath(Resource);

            var routeValues = new RouteValueDictionary(new { PluginId = manifest.Id, res = Resource });
            string pluginActionUrl = UrlHelper.GenerateUrl("Plugin_Resources", null, null, routeValues, RouteTable.Routes, RequestContext, false);

            pluginActionUrl += string.Format("?v={0}", resourcePath.Item2);

            if (Download)
                pluginActionUrl += "&Download=true";

            return new HtmlString(pluginActionUrl);
        }
        public static HtmlString DiscoPluginActionUrl<T>(this WebViewPage<T> ViewPage, string PluginAction)
        {
            if (string.IsNullOrEmpty(PluginAction))
                throw new ArgumentNullException("PluginAction");

            // Find Plugin
            var pageType = ViewPage.GetType();
            var pageAssembly = pageType.Assembly;
            var manifest = Plugins.GetPlugin(pageAssembly);

            return ViewPage.DiscoPluginActionUrl(PluginAction, manifest);
        }
        public static HtmlString DiscoPluginActionUrl<T>(this WebViewPage<T> ViewPage, string PluginAction, PluginManifest manifest)
        {
            return ViewPage.ViewContext.RequestContext.DiscoPluginActionUrl(PluginAction, manifest);
        }
        public static HtmlString DiscoPluginActionUrl(this RequestContext RequestContext, string PluginAction, PluginManifest manifest)
        {
            var routeValues = new RouteValueDictionary(new { PluginId = manifest.Id, PluginAction = PluginAction });
            string pluginActionUrl = UrlHelper.GenerateUrl("Plugin", null, null, routeValues, RouteTable.Routes, RequestContext, false);
            return new HtmlString(pluginActionUrl);
        }
        public static HtmlString DiscoPluginConfigureUrl<T>(this WebViewPage<T> ViewPage)
        {
            // Find Plugin
            var pageType = ViewPage.GetType();
            var pageAssembly = pageType.Assembly;
            var manifest = Plugins.GetPlugin(pageAssembly);

            return ViewPage.DiscoPluginConfigureUrl(manifest);
        }
        public static HtmlString DiscoPluginConfigureUrl<T>(this WebViewPage<T> ViewPage, PluginManifest manifest)
        {
            return new HtmlString(ViewPage.ViewContext.RequestContext.DiscoPluginConfigureUrl(manifest));
        }
        public static string DiscoPluginConfigureUrl(this RequestContext RequestContext, PluginManifest manifest)
        {
            var routeValues = new RouteValueDictionary(new { PluginId = manifest.Id });
            string pluginActionUrl = UrlHelper.GenerateUrl("Config_Plugins_Configure", null, null, routeValues, RouteTable.Routes, RequestContext, false);
            return pluginActionUrl;
        }
        public static MvcForm DiscoPluginActionBeginForm<T>(this WebViewPage<T> ViewPage, string PluginAction, FormMethod method, IDictionary<string, object> htmlAttributes)
        {
            if (string.IsNullOrEmpty(PluginAction))
                throw new ArgumentNullException("PluginAction");

            // Find Plugin
            var pageType = ViewPage.GetType();
            var pageAssembly = pageType.Assembly;
            var manifest = Plugins.GetPlugin(pageAssembly);

            var routeValues = new RouteValueDictionary(new { PluginId = manifest.Id, PluginAction = PluginAction });
            string pluginActionUrl = UrlHelper.GenerateUrl("Plugin", null, null, routeValues, RouteTable.Routes, ViewPage.ViewContext.RequestContext, false);

            return ViewPage.FormHelper(pluginActionUrl, method, htmlAttributes);
        }
        public static MvcForm DiscoPluginActionBeginForm<T>(this WebViewPage<T> ViewPage, string PluginAction, FormMethod method)
        {
            return ViewPage.DiscoPluginActionBeginForm(PluginAction, method, null);
        }
        public static MvcForm DiscoPluginActionBeginForm<T>(this WebViewPage<T> ViewPage, string PluginAction, IDictionary<string, object> htmlAttributes)
        {
            return ViewPage.DiscoPluginActionBeginForm(PluginAction, FormMethod.Post, htmlAttributes);
        }
        public static MvcForm DiscoPluginActionBeginForm<T>(this WebViewPage<T> ViewPage, string PluginAction)
        {
            return ViewPage.DiscoPluginActionBeginForm(PluginAction, FormMethod.Post, null);
        }

        private static MvcForm FormHelper<T>(this WebViewPage<T> ViewPage, string formAction, FormMethod method, IDictionary<string, object> htmlAttributes)
        {
            TagBuilder builder = new TagBuilder("form");
            builder.MergeAttributes<string, object>(htmlAttributes);
            builder.MergeAttribute("action", formAction);
            builder.MergeAttribute("method", HtmlHelper.GetFormMethodString(method), true);
            bool flag = ViewPage.ViewContext.ClientValidationEnabled && !ViewPage.ViewContext.UnobtrusiveJavaScriptEnabled;
            if (flag)
            {
                object obj2 = ViewPage.ViewContext.HttpContext.Items["DiscoPluginLastFormNum"];
                int num = (obj2 != null) ? (((int)obj2) + 1) : 1000;
                ViewPage.ViewContext.HttpContext.Items["DiscoPluginLastFormNum"] = num;

                builder.GenerateId(string.Format(CultureInfo.InvariantCulture, "form{0}", new object[] { num }));
            }
            ViewPage.ViewContext.Writer.Write(builder.ToString(TagRenderMode.StartTag));
            MvcForm form = new MvcForm(ViewPage.ViewContext);
            if (flag)
            {
                ViewPage.ViewContext.FormContext.FormId = builder.Attributes["id"];
            }
            return form;
        }


        public static void DiscoPluginRegisterStylesheet<T>(this WebViewPage<T> ViewPage, string Resource)
        {
            if (string.IsNullOrEmpty(Resource))
                throw new ArgumentNullException("Resource");

            // Find Plugin
            var pageType = ViewPage.GetType();
            var pageAssembly = pageType.Assembly;
            var manifest = Plugins.GetPlugin(pageAssembly);

            ViewPage.DiscoPluginRegisterStylesheet(Resource, manifest);
        }
        public static void DiscoPluginRegisterStylesheet<T>(this WebViewPage<T> ViewPage, string Resource, PluginManifest manifest)
        {
            ViewPage.ViewContext.RequestContext.DiscoPluginRegisterStylesheet(Resource, manifest);
        }
        public static void DiscoPluginRegisterStylesheet(this RequestContext RequestContext, string Resource, PluginManifest manifest)
        {
            var resourcePath = manifest.WebResourcePath(Resource);

            var routeValues = new RouteValueDictionary(new { PluginId = manifest.Id, res = Resource });
            string pluginResourceUrl = UrlHelper.GenerateUrl("Plugin_Resources", null, null, routeValues, RouteTable.Routes, RequestContext, false);

            pluginResourceUrl += string.Format("?v={0}", resourcePath.Item2);

            HtmlString pluginResourceUrlHtml = new HtmlString(pluginResourceUrl);

            var deferredBundles = RequestContext.HttpContext.Items["Bundles.UIExtensionCss"] as List<HtmlString>;
            if (deferredBundles == null)
            {
                deferredBundles = new List<HtmlString>();
                HttpContext.Current.Items["Bundles.UIExtensionCss"] = deferredBundles;
            }
            if (!deferredBundles.Contains(pluginResourceUrlHtml))
                deferredBundles.Add(pluginResourceUrlHtml);
        }

        #endregion

        #region Request Caching
        public static void SetCacheability(this PluginWebHandler Handler, TimeSpan CacheDuration)
        {
            var cache = Handler.HostController.Response.Cache;
            cache.SetOmitVaryStar(true);
            cache.SetExpires(DateTime.Now.Add(CacheDuration));
            cache.SetValidUntilExpires(true);
            cache.SetCacheability(HttpCacheability.Private);
        }
        public static void SetCacheabilityOff(this PluginWebHandler Handler)
        {
            var cache = Handler.HostController.Response.Cache;
            cache.SetExpires(DateTime.Now.AddDays(-1));
            cache.SetCacheability(HttpCacheability.NoCache);
        }
        #endregion

        #region Render Partial Compiled
        private static void RenderPartialCompiledInternal(this HtmlHelper htmlHelper, Type viewType, object model, TextWriter writer)
        {
            if (writer == null)
                throw new ArgumentNullException("writer");
            WebViewPage page = Activator.CreateInstance(viewType) as WebViewPage;
            if (page == null)
                throw new InvalidOperationException("Invalid View Type");
            page.ViewContext = htmlHelper.ViewContext;
            page.ViewData = new ViewDataDictionary(model);
            page.InitHelpers();
            HttpContextBase httpContext = htmlHelper.ViewContext.HttpContext;
            page.ExecutePageHierarchy(new WebPageContext(httpContext, null, model), writer, null);
        }
        public static MvcHtmlString PartialCompiled(this HtmlHelper htmlHelper, Type viewType)
        {
            return PartialCompiled(htmlHelper, viewType, null);
        }
        public static MvcHtmlString PartialCompiled(this HtmlHelper htmlHelper, Type viewType, object model)
        {
            using (StringWriter writer = new StringWriter(CultureInfo.CurrentCulture))
            {
                htmlHelper.RenderPartialCompiledInternal(viewType, model, writer);
                return MvcHtmlString.Create(writer.ToString());
            }
        }
        #endregion
    }
}
