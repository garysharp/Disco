using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SignalR;
using SignalR.Hosting.AspNet;
using SignalR.Infrastructure;

namespace Disco.Services.Logging.Targets
{
    public class LogLiveContext : PersistentConnection
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

        internal static void Broadcast(LogBase logModule, Models.LogEventType eventType, DateTime Timestamp, params object[] Arguments)
        {
            var message = Models.LogLiveEvent.Create(logModule, eventType, Timestamp, Arguments);

            var connectionManager = GlobalHost.ConnectionManager;
            var connectionContext = connectionManager.GetConnectionContext<LogLiveContext>();
            connectionContext.Groups.Send(_GroupNameAll, message);
            connectionContext.Groups.Send(logModule.ModuleName, message);
        }

        private const string _GroupNameAll = "__All";
        //private static string _QualifiedTypeName = typeof(LogLiveContext).FullName + ".";
        //private static string _QualifiedTypeNameAll = _QualifiedTypeName + "__All";
        //private static string LiveLogNameGroup(string LogName)
        //{
        //    return string.Concat(_QualifiedTypeName, LogName);
        //}
        public static string LiveLogNameAll
        {
            get
            {
                //return _QualifiedTypeNameAll;
                return _GroupNameAll;
            }
        }

    }
}
