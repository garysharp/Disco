using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNet.SignalR;

namespace Disco.BI.Interop.SignalRHandlers
{
    public class UserHeldDevices : PersistentConnection
    {

        internal static void UserJobUpdated(string JobUserId)
        {
            var connectionManager = GlobalHost.ConnectionManager;
            var connectionContext = connectionManager.GetConnectionContext<UserHeldDevices>();
            if (connectionContext != null)
                connectionContext.Connection.Broadcast(JobUserId);
        }

    }
}
