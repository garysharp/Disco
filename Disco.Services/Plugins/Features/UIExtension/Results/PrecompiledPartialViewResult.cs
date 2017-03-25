using System;
using System.Web.Mvc;
using System.Web.WebPages;

namespace Disco.Services.Plugins.Features.UIExtension.Results
{
    public class PrecompiledPartialViewResult : UIExtensionResult
    {
        private Type viewType;
        private object model;

        public PrecompiledPartialViewResult(PluginFeatureManifest Source, Type ViewType, object Model = null)
            : base(Source)
        {
            if (!typeof(WebViewPage).IsAssignableFrom(ViewType))
                throw new ArgumentException("The View Type must inherit from WebViewPage", "ViewType");

            viewType = ViewType;
            model = Model;
        }

        public override void ExecuteResult<T>(System.Web.Mvc.WebViewPage<T> page)
        {
            WebViewPage partialView = Activator.CreateInstance(viewType) as WebViewPage;
            if (partialView == null)
                throw new InvalidOperationException("Invalid View Type");
            partialView.ViewContext = page.ViewContext;
            partialView.ViewData = new ViewDataDictionary(model);
            partialView.InitHelpers();
            partialView.ExecutePageHierarchy(new WebPageContext(page.ViewContext.HttpContext, null, model), page.ViewContext.Writer, null);
        }
    }
}
