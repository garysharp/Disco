using Disco.Services.Users;
using Microsoft.AspNet.SignalR;
using System;
using System.Security.Principal;

namespace Disco.Services.Web.Signalling
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
    public class DiscoHubAuthorizeAttribute : AuthorizeAttribute
    {
        string authorizedClaim;

        public DiscoHubAuthorizeAttribute() { }

        public DiscoHubAuthorizeAttribute(string AuthorisedClaim)
        {
            this.authorizedClaim = AuthorisedClaim;
        }

        protected override bool UserAuthorized(IPrincipal user)
        {
            if (user == null || !user.Identity.IsAuthenticated)
                return false;

            var username = user.Identity.Name;
            var userToken = UserService.GetAuthorization(username);

            if (userToken == null)
                return false; // No User

            if (authorizedClaim == null)
                return userToken.RoleTokens.Count > 0; // Just Authenticate - no Authorization (but require at least 1 role)
            else
                return userToken.Has(authorizedClaim);
        }
    }
}
