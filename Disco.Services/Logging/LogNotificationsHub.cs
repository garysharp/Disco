using Disco.Services.Authorization;
using Disco.Services.Logging.Models;
using Disco.Services.Web.Signalling;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Disco.Services.Logging
{
    [HubName("logNotifications"), DiscoHubAuthorize(Claims.Config.Logging.Show)]
    public class LogNotificationsHub : Hub
    {
        private const string NotificationsModulePrefix = "Logging_";
        public const string AllLoggingNotification = NotificationsModulePrefix + "_ALL";

        public override Task OnConnected()
        {
            var logModules = Context.QueryString["LogModules"];
            if (!string.IsNullOrWhiteSpace(logModules) && logModules.Length > 0)
            {
                foreach (var modules in ValidLogModuleGroupNames(logModules))
                    Groups.Add(Context.ConnectionId, modules);
            }

            return base.OnConnected();
        }

        internal static void BroadcastLog(LogBase logModule, LogEventType eventType, DateTime Timestamp, params object[] Arguments)
        {
            var message = LogLiveEvent.Create(logModule, eventType, Timestamp, Arguments);

            var connectionManager = GlobalHost.ConnectionManager;
            var context = connectionManager.GetHubContext<LogNotificationsHub>();
            var targets = new List<string> { AllLoggingNotification, NotificationsModulePrefix + logModule.ModuleName };
            context.Clients.Groups(targets).receiveLog(message);
        }

        private IEnumerable<string> ValidLogModuleGroupNames(string ModuleNames)
        {
            if (string.IsNullOrWhiteSpace(ModuleNames))
                yield break;

            var names = ModuleNames.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var name in names)
            {
                // Special Case: __ALL
                if (name.Equals(AllLoggingNotification, StringComparison.OrdinalIgnoreCase))
                {
                    yield return AllLoggingNotification;
                }
                else
                {
                    var module = LogContext.LogModules.Values.FirstOrDefault(m => m.ModuleName.Equals(name, StringComparison.OrdinalIgnoreCase));

                    if (module == null)
                        throw new ArgumentException(string.Format("Invalid Module Name specified: {0}", name), "ModuleNames");

                    yield return NotificationsModulePrefix + module.ModuleName;
                }
            }
        }
    }
}
