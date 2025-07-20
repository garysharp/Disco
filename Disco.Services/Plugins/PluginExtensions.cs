using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Mvc;
using System.Web.WebPages;
using System.Web.Routing;
using System.Web;
using System.Web.Mvc.Html;
using System.Globalization;
using Disco.Services.Web.Bundles;

namespace Disco.Services.Plugins
{
    public static class PluginExtensions
    {
        #region Model Binding from Controller
        public static bool TryUpdateModel<TModel>(this Controller controller, TModel model) where TModel : class
        {
            return controller.TryUpdateModel(model, null, controller.ValueProvider);
        }
        public static bool TryUpdateModel<TModel>(this Controller controller, TModel model, IValueProvider valueProvider) where TModel : class
        {
            return controller.TryUpdateModel(model, null, valueProvider);
        }
        public static bool TryUpdateModel<TModel>(this Controller controller, TModel model, string prefix) where TModel : class
        {
            return controller.TryUpdateModel(model, prefix, controller.ValueProvider);
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

        #region Bundling
        public static void IncludeStyleSheetResource(this HttpContextBase Context, string Resource, PluginManifest manifest)
        {
            var resourceUrl = manifest.WebResourceUrl(Resource);

            var deferredBundles = Context.Items[BundleTable.UIExtensionCssKey] as List<string>;
            if (deferredBundles == null)
            {
                deferredBundles = new List<string>();
                HttpContext.Current.Items[BundleTable.UIExtensionCssKey] = deferredBundles;
            }
            if (!deferredBundles.Contains(resourceUrl))
                deferredBundles.Add(resourceUrl);
        }
        public static void IncludeScriptResource(this HttpContextBase Context, string Resource, PluginManifest manifest)
        {
            var resourcePath = manifest.WebResourceUrl(Resource);

            var deferredBundles = Context.Items[BundleTable.UIExtensionScriptsKey] as List<string>;
            if (deferredBundles == null)
            {
                deferredBundles = new List<string>();
                HttpContext.Current.Items[BundleTable.UIExtensionScriptsKey] = deferredBundles;
            }
            if (!deferredBundles.Contains(resourcePath))
                deferredBundles.Add(resourcePath);
        }
        #endregion

        #region Virtual Directories

        [Obsolete("Inherit ViewPages from 'Disco.Services.Plugins.WebViewPage' instead.")]
        public static HtmlString DiscoPluginResourceUrl<T>(this WebViewPage<T> ViewPage, string Resource)
        {
            return ViewPage.DiscoPluginResourceUrl(Resource, false);
        }
        [Obsolete("Inherit ViewPages from 'Disco.Services.Plugins.WebViewPage' instead.")]
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
        [Obsolete("Inherit ViewPages from 'Disco.Services.Plugins.WebViewPage' instead.")]
        public static HtmlString DiscoPluginResourceUrl<T>(this WebViewPage<T> ViewPage, string Resource, bool Download, PluginManifest manifest)
        {
            return ViewPage.ViewContext.RequestContext.DiscoPluginResourceUrl(Resource, Download, manifest);
        }
        [Obsolete]
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
        [Obsolete("Inherit ViewPages from 'Disco.Services.Plugins.WebViewPage' instead.")]
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
        [Obsolete("Inherit ViewPages from 'Disco.Services.Plugins.WebViewPage' instead.")]
        public static HtmlString DiscoPluginActionUrl<T>(this WebViewPage<T> ViewPage, string PluginAction, PluginManifest manifest)
        {
            return ViewPage.ViewContext.RequestContext.DiscoPluginActionUrl(PluginAction, manifest);
        }
        [Obsolete("Inherit ViewPages from 'Disco.Services.Plugins.WebViewPage' instead.")]
        public static HtmlString DiscoPluginActionUrl(this RequestContext RequestContext, string PluginAction, PluginManifest manifest)
        {
            var routeValues = new RouteValueDictionary(new { PluginId = manifest.Id, PluginAction = PluginAction });
            string pluginActionUrl = UrlHelper.GenerateUrl("Plugin", null, null, routeValues, RouteTable.Routes, RequestContext, false);
            return new HtmlString(pluginActionUrl);
        }
        [Obsolete("Inherit ViewPages from 'Disco.Services.Plugins.WebViewPage' instead.")]
        public static HtmlString DiscoPluginConfigureUrl<T>(this WebViewPage<T> ViewPage)
        {
            // Find Plugin
            var pageType = ViewPage.GetType();
            var pageAssembly = pageType.Assembly;
            var manifest = Plugins.GetPlugin(pageAssembly);

            return ViewPage.DiscoPluginConfigureUrl(manifest);
        }
        [Obsolete("Inherit ViewPages from 'Disco.Services.Plugins.WebViewPage' instead.")]
        public static HtmlString DiscoPluginConfigureUrl<T>(this WebViewPage<T> ViewPage, PluginManifest manifest)
        {
            return new HtmlString(ViewPage.ViewContext.RequestContext.DiscoPluginConfigureUrl(manifest));
        }
        [Obsolete]
        public static string DiscoPluginConfigureUrl(this RequestContext RequestContext, PluginManifest manifest)
        {
            var routeValues = new RouteValueDictionary(new { PluginId = manifest.Id });
            string pluginActionUrl = UrlHelper.GenerateUrl("Config_Plugins_Configure", null, null, routeValues, RouteTable.Routes, RequestContext, false);
            return pluginActionUrl;
        }
        [Obsolete("Inherit ViewPages from 'Disco.Services.Plugins.WebViewPage' instead.")]
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

#pragma warning disable 618
            return ViewPage.FormHelper(pluginActionUrl, method, htmlAttributes);
#pragma warning restore 618
        }
        [Obsolete("Inherit ViewPages from 'Disco.Services.Plugins.WebViewPage' instead.")]
        public static MvcForm DiscoPluginActionBeginForm<T>(this WebViewPage<T> ViewPage, string PluginAction, FormMethod method)
        {
#pragma warning disable 618
            return ViewPage.DiscoPluginActionBeginForm(PluginAction, method, null);
#pragma warning restore 618
        }
        [Obsolete("Inherit ViewPages from 'Disco.Services.Plugins.WebViewPage' instead.")]
        public static MvcForm DiscoPluginActionBeginForm<T>(this WebViewPage<T> ViewPage, string PluginAction, IDictionary<string, object> htmlAttributes)
        {
#pragma warning disable 618
            return ViewPage.DiscoPluginActionBeginForm(PluginAction, FormMethod.Post, htmlAttributes);
#pragma warning restore 618
        }
        [Obsolete("Inherit ViewPages from 'Disco.Services.Plugins.WebViewPage' instead.")]
        public static MvcForm DiscoPluginActionBeginForm<T>(this WebViewPage<T> ViewPage, string PluginAction)
        {
#pragma warning disable 618
            return ViewPage.DiscoPluginActionBeginForm(PluginAction, FormMethod.Post, null);
#pragma warning restore 618
        }

        [Obsolete("Inherit ViewPages from 'Disco.Services.Plugins.WebViewPage' instead.")]
        private static MvcForm FormHelper<T>(this WebViewPage<T> ViewPage, string formAction, FormMethod method, IDictionary<string, object> htmlAttributes)
        {
            TagBuilder builder = new TagBuilder("form");
            builder.MergeAttributes(htmlAttributes);
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

        [Obsolete("Inherit ViewPages from 'Disco.Services.Plugins.WebViewPage' instead.")]
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
        [Obsolete("Inherit ViewPages from 'Disco.Services.Plugins.WebViewPage' instead.")]
        public static void DiscoPluginRegisterStylesheet<T>(this WebViewPage<T> ViewPage, string Resource, PluginManifest manifest)
        {
            ViewPage.ViewContext.RequestContext.DiscoPluginRegisterStylesheet(Resource, manifest);
        }
        [Obsolete]
        public static void DiscoPluginRegisterStylesheet(this RequestContext RequestContext, string Resource, PluginManifest manifest)
        {
            var resourcePath = manifest.WebResourcePath(Resource);

            var routeValues = new RouteValueDictionary(new { PluginId = manifest.Id, res = Resource });
            string pluginResourceUrl = UrlHelper.GenerateUrl("Plugin_Resources", null, null, routeValues, RouteTable.Routes, RequestContext, false);

            pluginResourceUrl += string.Format("?v={0}", resourcePath.Item2);

            HtmlString pluginResourceUrlHtml = new HtmlString(pluginResourceUrl);

            var deferredBundles = RequestContext.HttpContext.Items[BundleTable.UIExtensionCssKey] as List<HtmlString>;
            if (deferredBundles == null)
            {
                deferredBundles = new List<HtmlString>();
                HttpContext.Current.Items[BundleTable.UIExtensionCssKey] = deferredBundles;
            }
            if (!deferredBundles.Contains(pluginResourceUrlHtml))
                deferredBundles.Add(pluginResourceUrlHtml);
        }

        #endregion

        #region Request Caching
        [Obsolete]
        public static void SetCacheability(this PluginWebHandler Handler, TimeSpan CacheDuration)
        {
            var cache = Handler.HostController.Response.Cache;
            cache.SetOmitVaryStar(true);
            cache.SetExpires(DateTime.Now.Add(CacheDuration));
            cache.SetValidUntilExpires(true);
            cache.SetCacheability(HttpCacheability.Private);
        }
        [Obsolete]
        public static void SetCacheabilityOff(this PluginWebHandler Handler)
        {
            var cache = Handler.HostController.Response.Cache;
            cache.SetExpires(DateTime.Now.AddDays(-1));
            cache.SetCacheability(HttpCacheability.NoCache);
        }
        #endregion

        #region Render Partial Compiled
        [Obsolete]
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
        [Obsolete]
        public static MvcHtmlString PartialCompiled(this HtmlHelper htmlHelper, Type viewType)
        {
            return PartialCompiled(htmlHelper, viewType, null);
        }
        [Obsolete]
        public static MvcHtmlString PartialCompiled(this HtmlHelper htmlHelper, Type viewType, object model)
        {
            using (StringWriter writer = new StringWriter(CultureInfo.CurrentCulture))
            {
#pragma warning disable 618
                htmlHelper.RenderPartialCompiledInternal(viewType, model, writer);
#pragma warning restore 618
                return MvcHtmlString.Create(writer.ToString());
            }
        }
        #endregion
    }
}
