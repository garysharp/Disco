using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SignalR;
using SignalR.Hosting.AspNet;
using SignalR.Infrastructure;

namespace Disco.Services.Tasks
{
    public class ScheduledTasksLiveStatusService : PersistentConnection
    {

        protected override System.Threading.Tasks.Task OnReceivedAsync(IRequest request, string connectionId, string data)
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
            
            return base.OnReceivedAsync(request, connectionId, data);
        }

        internal static void Broadcast(ScheduledTaskStatusLive SessionStatus)
        {
            //var message = Models.LogLiveEvent.Create(logModule, eventType, Timestamp, Arguments);

            var connectionManager = GlobalHost.ConnectionManager; //AspNetHost.DependencyResolver.Resolve<IConnectionManager>();
            var connectionContext = connectionManager.GetConnectionContext<ScheduledTasksLiveStatusService>();
            connectionContext.Groups.Send(_GroupNameAll, SessionStatus);
            connectionContext.Groups.Send(SessionStatus.SessionId, SessionStatus);
        }

        private const string _GroupNameAll = "__All";
        //private static string _QualifiedSessionName = typeof(ScheduledTasksLiveStatusService).FullName + ".";
        //private static string _QualifiedSessionNameAll = _QualifiedSessionName + "__All";
        //private static string LiveStatusGroup(string SessionId)
        //{
        //    return string.Concat(_QualifiedSessionName, SessionId);
        //}
        public static string LiveStatusAll
        {
            get
            {
                //return _QualifiedTypeNameAll;
                return _GroupNameAll;
            }
        }

    }
}
