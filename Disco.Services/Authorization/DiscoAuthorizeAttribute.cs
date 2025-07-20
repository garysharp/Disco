using Disco.Services.Users;

namespace Disco.Services.Authorization
{
    public class DiscoAuthorizeAttribute : DiscoAuthorizeBaseAttribute
    {
        string authorizedClaim;

        public DiscoAuthorizeAttribute() { }

        public DiscoAuthorizeAttribute(string AuthorisedClaim)
        {
            authorizedClaim = AuthorisedClaim;
        }

        public override bool IsAuthorized(System.Web.HttpContextBase httpContext)
        {
            if (Token == null)
                return false; // No Current User

            if (authorizedClaim == null)
                return Token.RoleTokens.Count > 0; // Just Authenticate - no Authorization (but require at least 1 role)
            else
                return Token.Has(authorizedClaim);
        }

        public override string HandleUnauthorizedMessage()
        {
            string resultMessage;

            if (UserService.CurrentAuthorization == null)
                resultMessage = AuthorizationToken.RequireAuthenticationMessage;
            else if (string.IsNullOrEmpty(authorizedClaim))
                resultMessage = AuthorizationToken.RequireDiscoAuthorizationMessage;
            else
                resultMessage = AuthorizationToken.BuildRequireMessage(authorizedClaim);

            return resultMessage;
        }
    }
}
