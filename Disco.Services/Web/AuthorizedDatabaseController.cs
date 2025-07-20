using Disco.Models.Repository;
using Disco.Services.Authorization;
using Disco.Services.Users;

namespace Disco.Services.Web
{
    [DiscoAuthorize]
    public abstract class AuthorizedDatabaseController : DatabaseController
    {
        public AuthorizationToken Authorization
        {
            get
            {
                return UserService.CurrentAuthorization;
            }
        }

        public User CurrentUser
        {
            get
            {
                return UserService.CurrentUser;
            }
        }
    }
}
