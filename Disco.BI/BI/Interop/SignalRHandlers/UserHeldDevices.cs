using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SignalR;
using SignalR.Hosting.AspNet;
using SignalR.Infrastructure;

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
