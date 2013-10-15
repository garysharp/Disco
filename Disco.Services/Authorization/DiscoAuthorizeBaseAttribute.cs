using Disco.Services.Users;
using System;
using System.Collections.Generic;
using System.Linq;
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

            LogAccessDenied(filterContext, resultMessage);

            filterContext.Result = new HttpUnauthorizedResult(resultMessage);
        }

        public void LogAccessDenied(AuthorizationContext FilterContext, string ResultMessage)
        {
            // Don't log anonymous
            if (Token != null)
            {
                // Calculate Authorize Resource
                if (AuthorizeResource == null)
                {
                    var controllerName = FilterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
                    var actionName = FilterContext.ActionDescriptor.ActionName;

                    AuthorizeResource = string.Format("{0}::{1}", controllerName, actionName);
                }

                var resource = string.Format("{0} [{1}]", AuthorizeResource, FilterContext.HttpContext.Request.RawUrl);

                AuthorizationLog.LogAccessDenied(Token.User.Id, resource, ResultMessage);
            }
        }
    }
}
