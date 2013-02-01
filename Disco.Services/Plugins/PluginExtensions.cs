using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Disco.Data.Repository;
using System.IO;
using System.Web.Mvc;
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

            var resourcePath = manifest.WebResourcePath(Resource);

            var routeValues = new RouteValueDictionary(new { PluginId = manifest.Id, res = Resource });
            string pluginActionUrl = UrlHelper.GenerateUrl("Plugin_Resources", null, null, routeValues, RouteTable.Routes, ViewPage.ViewContext.RequestContext, false);

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

            var routeValues = new RouteValueDictionary(new { PluginId = manifest.Id, PluginAction = PluginAction });
            string pluginActionUrl = UrlHelper.GenerateUrl("Plugin", null, null, routeValues, RouteTable.Routes, ViewPage.ViewContext.RequestContext, false);
            return new HtmlString(pluginActionUrl);
        }
        public static HtmlString DiscoPluginConfigureUrl<T>(this WebViewPage<T> ViewPage)
        {
            // Find Plugin
            var pageType = ViewPage.GetType();
            var pageAssembly = pageType.Assembly;
            var manifest = Plugins.GetPlugin(pageAssembly);

            var routeValues = new RouteValueDictionary(new { PluginId = manifest.Id });
            string pluginActionUrl = UrlHelper.GenerateUrl("Config_Plugins_Configure", null, null, routeValues, RouteTable.Routes, ViewPage.ViewContext.RequestContext, false);
            return new HtmlString(pluginActionUrl);
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




        #endregion
    }
}
