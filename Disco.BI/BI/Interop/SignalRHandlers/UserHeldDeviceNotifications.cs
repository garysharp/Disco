using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNet.SignalR;
using System.Reactive.Linq;
using Disco.Data.Repository.Monitor;
using Disco.Models.Repository;

namespace Disco.BI.Interop.SignalRHandlers
{
    public class UserHeldDeviceNotifications : PersistentConnection
    {
        private static bool subscribed = false;
        private static object subscribeLock = new object();

        static UserHeldDeviceNotifications()
        {
            if (!subscribed)
                lock (subscribeLock)
                    if (!subscribed)
                    {
                        Disco.Data.Repository.Monitor.RepositoryMonitor.StreamAfterCommit.Where(e => e.EntityType == typeof(Job)).Subscribe(UserJobUpdated);
                        subscribed = true;
                    }
        }

        private static void UserJobUpdated(RepositoryMonitorEvent e)
        {
            Job j = (Job)e.Entity;

            if (j.UserId != null)
            {
                var connectionManager = GlobalHost.ConnectionManager;
                var connectionContext = connectionManager.GetConnectionContext<UserHeldDeviceNotifications>();
                if (connectionContext != null)
                    connectionContext.Connection.Broadcast(j.UserId);
            }
        }
    }
}
