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

            filterContext.Result = new HttpUnauthorizedResult(resultMessage);
        }
    }
}
