using Disco.Services.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Disco.Services.Authorization
{
    public abstract class DiscoAuthorizeBaseAttribute : AuthorizeAttribute
    {
        public string AuthorizeResource { get; set; }

        protected AuthorizationToken Token
        {
            get
            {
                return UserService.CurrentAuthorization;
            }
        }

        public abstract bool IsAuthorized(System.Web.HttpContextBase httpContext);
        public abstract string HandleUnauthorizedMessage();

        protected sealed override bool AuthorizeCore(System.Web.HttpContextBase httpContext)
        {
            if (httpContext == null)
                throw new ArgumentNullException("httpContext");

            return IsAuthorized(httpContext);
        }

        protected sealed override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            string resultMessage = HandleUnauthorizedMessage();
            string resultResource = BuildAuthorizeResource(filterContext);

            // Log Access Denied
            if (Token != null) // Don't log anonymous
                AuthorizationLog.LogAccessDenied(Token.User.UserId, resultResource, resultMessage);

            // Build Response View
            var ex = new AccessDeniedException(resultMessage, resultResource);
            HandleErrorInfo model = new HandleErrorInfo(ex, filterContext.ActionDescriptor.ControllerDescriptor.ControllerName, filterContext.ActionDescriptor.ActionName);
            ViewResult result = new ViewResult
            {
                ViewName = "Error",
                MasterName = Token == null ? "_PublicLayout" : "_Layout",
                ViewData = new ViewDataDictionary<HandleErrorInfo>(model),
                TempData = filterContext.Controller.TempData
            };

            filterContext.Result = result;
            var contextResponse = filterContext.HttpContext.Response;
            contextResponse.Clear();
            contextResponse.StatusCode = (int)HttpStatusCode.Unauthorized;
            contextResponse.TrySkipIisCustomErrors = true;
        }

        private string BuildAuthorizeResource(AuthorizationContext FilterContext)
        {
            var authResource = AuthorizeResource;
            var url = FilterContext.HttpContext.Request.RawUrl;

            if (authResource == null)
            {
                var controllerName = FilterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
                var actionName = FilterContext.ActionDescriptor.ActionName;

                authResource = string.Format("{0}::{1}", controllerName, actionName);
            }

            return string.Format("{0} [{1}]", authResource, url);
        }
    }
}
