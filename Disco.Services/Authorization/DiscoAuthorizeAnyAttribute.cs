using Disco.Services.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Disco.Services.Authorization
{
    public class DiscoAuthorizeAnyAttribute : AuthorizeAttribute
    {
        string[] authorizedClaims;

        public DiscoAuthorizeAnyAttribute(params string[] AuthorisedClaims)
        {
            if (AuthorisedClaims == null || AuthorisedClaims.Length == 0)
                throw new ArgumentNullException("AuthorisedClaims");

            this.authorizedClaims = AuthorisedClaims;
        }

        protected override bool AuthorizeCore(System.Web.HttpContextBase httpContext)
        {
            if (httpContext == null)
                throw new ArgumentNullException("httpContext");

            var authToken = UserService.CurrentAuthorization;

            if (authToken == null)
                return false; // No Current User

            return authToken.HasAny(authorizedClaims);
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            filterContext.Result = new HttpUnauthorizedResult(AuthorizationToken.BuildRequireAnyMessage(authorizedClaims));
        }
    }
}
