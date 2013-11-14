using Disco.Services.Authorization;
using Disco.Services.Users;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Disco.Services.Web
{
    public static class HelperExtensions
    {
        public static void HandleException(this ExceptionContext filterContext)
        {
            var ex = filterContext.Exception;
            var contextResponse = filterContext.HttpContext.Response;

            LogException(ex);

            HttpException httpException = new HttpException(null, ex);
            int httpExceptionCode = httpException.GetHttpCode();

            string controllerName = (string)filterContext.RouteData.Values["controller"];
            string actionName = (string)filterContext.RouteData.Values["action"];
            HandleErrorInfo model = new HandleErrorInfo(ex, controllerName ?? "Unknown", actionName ?? "Unknown");
            ViewResult result = new ViewResult
            {
                ViewName = "Error",
                MasterName = "_Layout",
                ViewData = new ViewDataDictionary<HandleErrorInfo>(model),
                TempData = filterContext.Controller.TempData
            };
            
            filterContext.Result = result;
            filterContext.ExceptionHandled = true;
            contextResponse.Clear();
            contextResponse.StatusCode = httpExceptionCode;
            contextResponse.TrySkipIisCustomErrors = true;
        }
        public static void HandleException(this HttpApplication httpApplication)
        {
            var ex = httpApplication.Server.GetLastError();

            LogException(ex);
        }
        private static void LogException(Exception Exception)
        {

            // Log Exception:
            try
            {
                if (Exception is AccessDeniedException)
                {
                    var accessDeniedException = (AccessDeniedException)Exception;
                    var resource = accessDeniedException.Resource;
                    var httpContext = HttpContext.Current;
                    if (httpContext != null && httpContext.Request != null)
                        resource = string.Format("{0} [{1}]", resource, httpContext.Request.RawUrl);

                    AuthorizationLog.LogAccessDenied(UserService.CurrentUserId ?? "[Anonymous]", resource, accessDeniedException.Message);
                }
                else
                {
                    Disco.Services.Logging.SystemLog.LogException("Global Application Exception Caught", Exception);
                }
            }
            catch (Exception)
            {
                // Ignore all logging errors
            }
        }

    }
}
