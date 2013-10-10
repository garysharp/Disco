using Disco.Services.Authorization;
using Disco.Services.Logging;
using Disco.Services.Logging.Models;
using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.BI.Interop.SignalRHandlers
{
    public class LogNotifications : AuthorizedPersistentConnection
    {
        public static bool initialized = false;

        protected override string AuthorizedClaim { get { return Claims.DiscoAdminAccount; } }

        public LogNotifications()
        {
            if (!initialized)
            {
                initialized = true;
                Disco.Services.Logging.Targets.LogLiveContext.LogBroadcast += Broadcast;
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

        internal static void Broadcast(LogBase logModule, LogEventType eventType, DateTime Timestamp, params object[] Arguments)
        {
            var message = LogLiveEvent.Create(logModule, eventType, Timestamp, Arguments);

            var connectionManager = GlobalHost.ConnectionManager;
            var connectionContext = connectionManager.GetConnectionContext<LogNotifications>();
            connectionContext.Groups.Send(_GroupNameAll, message);
            connectionContext.Groups.Send(logModule.ModuleName, message);
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
