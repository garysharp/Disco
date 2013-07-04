using Disco.Data.Repository.Monitor;
using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.BI.Interop.SignalRHandlers
{
    public class RepositoryMonitorNotifications : AdminAuthorizedPersistentConnection
    {
        public static void Initialize()
        {
            RepositoryMonitor.StreamAfterCommit.Subscribe(AfterCommit);
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

        protected override Task OnReceived(IRequest request, string connectionId, string data)
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

        private static void AfterCommit(RepositoryMonitorEvent e)
        {
            GlobalHost.ConnectionManager.GetConnectionContext<RepositoryMonitorNotifications>().Groups.Send(e.EntityType.Name, e);
        }
    }
}
