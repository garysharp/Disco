using Disco.Services.Authorization;
using Disco.Services.Tasks;
using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.BI.Interop.SignalRHandlers
{
    public class ScheduledTasksStatusNotifications : AuthorizedPersistentConnection
    {
        public static bool initialized = false;

        protected override string AuthorizedClaim { get { return Claims.DiscoAdminAccount; } }

        public ScheduledTasksStatusNotifications()
        {
            if (!initialized)
            {
                initialized = true;
                Disco.Services.Tasks.ScheduledTaskStatus.UpdatedBroadcast += Broadcast;
            }
        }

        protected override Task OnConnected(IRequest request, string connectionId)
        {
            string addToGroups = request.QueryString["addToGroups"];

            if (!string.IsNullOrWhiteSpace(addToGroups))
            {
                var groups = addToGroups.Split(',');
                foreach (var g in groups)
                {
                    this.Groups.Add(connectionId, g);
                }
            }

            return base.OnConnected(request, connectionId);
        }

        protected override System.Threading.Tasks.Task OnReceived(IRequest request, string connectionId, string data)
        {
            // Add to Group
            if (!string.IsNullOrWhiteSpace(data) && data.StartsWith("/addToGroups:") && data.Length > 13)
            {
                var groups = data.Substring(13).Split(',');
                foreach (var g in groups)
                {
                    this.Groups.Add(connectionId, g);
                }
            }
            return base.OnReceived(request, connectionId, data);
        }

        internal static void Broadcast(ScheduledTaskStatusLive SessionStatus)
        {
            var connectionManager = GlobalHost.ConnectionManager;
            var connectionContext = connectionManager.GetConnectionContext<ScheduledTasksStatusNotifications>();
            connectionContext.Groups.Send(_GroupNameAll, SessionStatus);
            connectionContext.Groups.Send(SessionStatus.SessionId, SessionStatus);
        }

        private const string _GroupNameAll = "__All";
        
        public static string AllNotifications
        {
            get
            {
                return _GroupNameAll;
            }
        }
    }
}
