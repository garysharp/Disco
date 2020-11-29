using Disco.Data.Repository;
using Disco.Data.Repository.Monitor;
using Disco.Models.Repository;
using Disco.Services.Authorization;
using Disco.Services.Web.Signalling;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Disco.Services.Devices
{
    [HubName("deviceBatchUpdates"), DiscoHubAuthorizeAny(Claims.Config.DeviceBatch.Show, Claims.Config.DeviceBatch.Configure)]
    public class DeviceBatchUpdatesHub : Hub
    {
        private const string UserPrefix = "DeviceBatch_";
        public static IHubContext HubContext { get; private set; }

        private static IDisposable RepositoryBeforeSubscription;
        private static IDisposable RepositoryAfterSubscription;

        static DeviceBatchUpdatesHub()
        {
            HubContext = GlobalHost.ConnectionManager.GetHubContext<DeviceBatchUpdatesHub>();

            // Subscribe to Repository Monitor for Changes
            RepositoryBeforeSubscription = RepositoryMonitor.StreamBeforeCommit
                .Where(e => e.EntityType == typeof(DeviceBatchAttachment) && e.EventType == RepositoryMonitorEventType.Deleted)
                .Subscribe(RepositoryEventBefore);
            RepositoryAfterSubscription = RepositoryMonitor.StreamAfterCommit
                .Where(e => e.EntityType == typeof(DeviceBatchAttachment) && e.EventType == RepositoryMonitorEventType.Added)
                .Subscribe(RepositoryAfterEvent);
        }

        private static string GroupName(int deviceBatchId)
        {
            return $"{UserPrefix}{deviceBatchId}";
        }

        public override Task OnConnected()
        {
            if (!int.TryParse(Context.QueryString["DeviceBatchId"], out var deviceBatchId))
                throw new ArgumentNullException("DeviceBatchId");

            Groups.Add(Context.ConnectionId, GroupName(deviceBatchId));

            return base.OnConnected();
        }

        private static void RepositoryEventBefore(RepositoryMonitorEvent e)
        {
            if (e.EventType == RepositoryMonitorEventType.Deleted && e.Entity is DeviceBatchAttachment attachment)
            {
                using (DiscoDataContext Database = new DiscoDataContext())
                {
                    var deviceBatchId = Database.DeviceBatchAttachments.Where(a => a.Id == attachment.Id).Select(a => a.DeviceBatchId).First();
                    HubContext.Clients.Group(GroupName(deviceBatchId)).removeAttachment(attachment.Id);
                }
            }
        }

        private static void RepositoryAfterEvent(RepositoryMonitorEvent e)
        {
            if (e.EventType == RepositoryMonitorEventType.Added && e.Entity is DeviceBatchAttachment attachment)
            {
                HubContext.Clients.Group(GroupName(attachment.DeviceBatchId)).addAttachment(attachment.Id);
            }
        }
    }
}
