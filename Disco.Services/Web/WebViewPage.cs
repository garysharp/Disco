using Disco.Models.Repository;
using Disco.Services.Authorization;
using Disco.Services.Users;

namespace Disco.Services.Web
{
    public abstract class WebViewPage<T> : System.Web.Mvc.WebViewPage<T>
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
