using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.BI.Interop.SignalRHandlers
{
    public class AuthorizedPersistentConnection : PersistentConnection
    {
        private string[] authorizedUserTypes = null;

        protected virtual string[] AuthorizedUserTypes { get { return authorizedUserTypes; } }

        protected override bool AuthorizeRequest(IRequest request)
        {
            if (!request.User.Identity.IsAuthenticated)
                return false;
            else
            {
                var user = UserBI.UserCache.CurrentUser;
                if (user == null)
                    return false;

                if (AuthorizedUserTypes == null || AuthorizedUserTypes.Length == 0)
                    return true;

                if (AuthorizedUserTypes.Contains(user.Type))
                    return true;

                return false;
            }
        }
    }
}
