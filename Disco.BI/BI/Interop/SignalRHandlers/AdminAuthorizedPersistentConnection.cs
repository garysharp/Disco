using Disco.Models.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.BI.Interop.SignalRHandlers
{
    public class AdminAuthorizedPersistentConnection : AuthorizedPersistentConnection
    {
        private string[] authorizedUserTypes = { User.Types.Admin };

        protected override string[] AuthorizedUserTypes { get { return authorizedUserTypes; } }
    }
}