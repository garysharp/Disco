using Disco.Services.Users;
using Microsoft.AspNet.SignalR;
using System;
using System.Security.Principal;

namespace Disco.Services.Web.Signalling
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
    public class DiscoHubAuthorizeAnyAttribute : AuthorizeAttribute
    {
        string[] authorizedClaims;

        public DiscoHubAuthorizeAnyAttribute(params string[] AuthorisedClaims)
        {
            if (AuthorisedClaims == null || AuthorisedClaims.Length == 0)
                throw new ArgumentNullException("AuthorisedClaims");

            authorizedClaims = AuthorisedClaims;
        }

        protected override bool UserAuthorized(IPrincipal user)
        {
            if (user == null || !user.Identity.IsAuthenticated)
                return false;

            var username = user.Identity.Name;
            var userToken = UserService.GetAuthorization(username);

            if (userToken == null)
                return false; // No User

            return userToken.HasAny(authorizedClaims);
        }
    }
}
