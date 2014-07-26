using System.Web.Mvc;
using System.Web.Routing;

namespace Disco.Web.Extensions
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

    }
}