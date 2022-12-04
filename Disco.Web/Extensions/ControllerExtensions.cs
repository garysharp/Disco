using RazorGenerator.Mvc;
using System;
using System.Web.Mvc;
using System.Web.Routing;

namespace Disco.Web
{
    public static class ControllerExtensions
    {
        public static ActionResult RedirectToAction(this Controller controller, ActionResult result, string urlFragment)
        {
            var callInfo = result.GetT4MVCResult();

            if (!string.IsNullOrWhiteSpace(urlFragment))
            {
                var url = UrlHelper.GenerateUrl(null, null, null, callInfo.RouteValueDictionary, RouteTable.Routes, controller.HttpContext.Request.RequestContext, false);

                url = string.Concat(url, "#", urlFragment);

                return new RedirectResult(url, false);
            }
            else
            {
                return new RedirectToRouteResult(callInfo.RouteValueDictionary);
            }
        }

        private static string[] _viewFileNames = new string[] { "cshtml" };
        public static ActionResult PrecompiledPartialView<ViewType>(this Controller controller, object model) where ViewType : WebViewPage
        {
            return PrecompiledPartialView(controller, typeof(ViewType), model);
        }

        public static ActionResult PrecompiledPartialView(this Controller controller, Type viewType, object model)
        {
            if (!typeof(WebViewPage).IsAssignableFrom(viewType))
                throw new ArgumentException("ViewType must be assignable from WebViewPage", "viewType");

            IView v = new PrecompiledMvcView(controller.Request.Path, viewType, false, _viewFileNames);

            if (model != null)
                controller.ViewData.Model = model;

            return new PartialViewResult { View = v, ViewData = controller.ViewData, TempData = controller.TempData };
        }
    }
}