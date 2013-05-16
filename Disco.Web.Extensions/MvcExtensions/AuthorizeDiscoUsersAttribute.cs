using Disco.BI.UserBI;
using Disco.Models.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Disco.Web
{
    public class AuthorizeDiscoUsersAttribute : AuthorizeAttribute
    {
        string[] authorizedTypes;

        public AuthorizeDiscoUsersAttribute(params string[] AuthorizedUserTypes)
        {
            if (AuthorizedUserTypes == null)
                throw new ArgumentNullException("AuthorizedUserTypes");
            if (AuthorizedUserTypes.Length == 0)
                throw new ArgumentOutOfRangeException("AuthorizedUserTypes", "At least one Authorized User Type must be specified");
            
            authorizedTypes = AuthorizedUserTypes;
        }

        protected override bool AuthorizeCore(System.Web.HttpContextBase httpContext)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException("httpContext");
            }

            var DiscoUser = UserCache.CurrentUser;

            if (DiscoUser != null && authorizedTypes.Contains(DiscoUser.Type))
                return true;

            return false;
        }
    }
}
