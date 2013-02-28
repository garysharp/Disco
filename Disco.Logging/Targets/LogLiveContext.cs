using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SignalR;
using SignalR.Hosting.AspNet;
using SignalR.Infrastructure;

namespace Disco.Logging.Targets
{
    public class LogLiveContext : PersistentConnection
    {
        protected override System.Threading.Tasks.Task OnReceivedAsync(string connectionId, string data)
        {
            // Add to Group
            if (!string.IsNullOrWhiteSpace(data) && data.StartsWith("/addToGroups:") && data.Length > 13)
            {
                var groups = data.Substring(13).Split(',');
                foreach (var g in groups)
                {
                    this.AddToGroup(connectionId, g);
                }
            }

            return base.OnReceivedAsync(connectionId, data);
        }

        internal static void Broadcast(LogBase logModule, Models.LogEventType eventType, DateTime Timestamp, params object[] Arguments)
        {
            var message = Models.LogLiveEvent.Create(logModule, eventType, Timestamp, Arguments);

            var connectionManager = AspNetHost.DependencyResolver.Resolve<IConnectionManager>();
            var connection = connectionManager.GetConnection<LogLiveContext>();
            connection.Broadcast(_QualifiedTypeNameAll, message);
            connection.Broadcast(LiveLogNameGroup(logModule.ModuleName), message);
        }

        private const string _GroupNameAll = "__All";
        private static string _QualifiedTypeName = typeof(LogLiveContext).FullName + ".";
        private static string _QualifiedTypeNameAll = _QualifiedTypeName + "__All";
        private static string LiveLogNameGroup(string LogName)
        {
            return string.Concat(_QualifiedTypeName, LogName);
        }
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
