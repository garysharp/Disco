using Disco.Services.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Disco.Services.Authorization
{
    public class DiscoAuthorizeAttribute : AuthorizeAttribute
    {
        string authorizedClaim;

        public DiscoAuthorizeAttribute() { }

        public DiscoAuthorizeAttribute(string AuthorisedClaim)
        {
            this.authorizedClaim = AuthorisedClaim;
        }

        protected override bool AuthorizeCore(System.Web.HttpContextBase httpContext)
        {
            if (httpContext == null)
                throw new ArgumentNullException("httpContext");

            var authToken = UserService.CurrentAuthorization;

            if (authToken == null)
                return false; // No Current User

            if (authorizedClaim == null)
                return authToken.RoleTokens.Count > 0; // Just Authenticate - no Authorization (but require at least 1 role)
            else
                return authToken.Has(authorizedClaim);
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            string resultMessage;

            if (UserService.CurrentAuthorization == null)
                resultMessage = AuthorizationToken.RequireAuthenticationMessage;
            else
                if (string.IsNullOrEmpty(authorizedClaim))
                    resultMessage = AuthorizationToken.RequireDiscoAuthorizationMessage;
                else
                    resultMessage = AuthorizationToken.BuildRequireMessage(authorizedClaim);

            filterContext.Result = new HttpUnauthorizedResult(resultMessage);
        }
    }
}
