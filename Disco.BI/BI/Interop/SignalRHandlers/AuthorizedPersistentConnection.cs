using Disco.Services.Users;
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
        private string authorizedClaim = null;

        protected virtual string AuthorizedClaim { get { return authorizedClaim; } }

        protected override bool AuthorizeRequest(IRequest request)
        {
            if (!request.User.Identity.IsAuthenticated)
                return false;
            else
            {
                var authToken = UserService.CurrentAuthorization;
                
                if (authToken == null)
                    return false; // No Current User

                if (authorizedClaim == null)
                    return true; // Just Authenticate - no Authorization
                else
                    return authToken.Has(authorizedClaim);
            }
        }
    }
}
