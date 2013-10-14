using Disco.Services.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Disco.Services.Authorization
{
    public class DiscoAuthorizeAnyAttribute : DiscoAuthorizeBaseAttribute
    {
        string[] authorizedClaims;

        public DiscoAuthorizeAnyAttribute(params string[] AuthorisedClaims)
        {
            if (AuthorisedClaims == null || AuthorisedClaims.Length == 0)
                throw new ArgumentNullException("AuthorisedClaims");

            this.authorizedClaims = AuthorisedClaims;
        }

        public override bool IsAuthorized(System.Web.HttpContextBase httpContext)
        {
            if (Token == null)
                return false; // No Current User

            return Token.HasAny(authorizedClaims);
        }

        public override string HandleUnauthorizedMessage()
        {
            return AuthorizationToken.BuildRequireAnyMessage(authorizedClaims);
        }
    }
}
