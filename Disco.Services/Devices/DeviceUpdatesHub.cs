using Disco.Data.Repository.Monitor;
using Disco.Services.Authorization;
using Disco.Services.Web.Signalling;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Linq;
using Disco.Models.Repository;
using Disco.Data.Repository;

namespace Disco.Services.Devices
{
    [HubName("deviceUpdates"), DiscoHubAuthorizeAll(Claims.Device.Show, Claims.Device.ShowAttachments)]
    public class DeviceUpdatesHub : Hub
    {
        private const string UserPrefix = "Device_";
        public static IHubContext HubContext { get; private set; }

        private static IDisposable RepositoryBeforeSubscription;
        private static IDisposable RepositoryAfterSubscription;

        static DeviceUpdatesHub()
        {
            HubContext = GlobalHost.ConnectionManager.GetHubContext<DeviceUpdatesHub>();

            // Subscribe to Repository Monitor for Changes
            RepositoryBeforeSubscription = RepositoryMonitor.StreamBeforeCommit
                .Where(e => e.EntityType == typeof(DeviceAttachment) && e.EventType == RepositoryMonitorEventType.Deleted)
                .Subscribe(RepositoryEventBefore);
            RepositoryAfterSubscription = RepositoryMonitor.StreamAfterCommit
                .Where(e => e.EntityType == typeof(DeviceAttachment) && e.EventType == RepositoryMonitorEventType.Added)
                .Subscribe(RepositoryAfterEvent);
        }

        private static string GroupName(string DeviceSerialNumber)
        {
            return UserPrefix + DeviceSerialNumber;
        }

        public override Task OnConnected()
        {
            var deviceSerialNumber = Context.QueryString["DeviceSerialNumber"];

            if (string.IsNullOrWhiteSpace(deviceSerialNumber))
                throw new ArgumentNullException("DeviceSerialNumber");

            Groups.Add(Context.ConnectionId, GroupName(deviceSerialNumber));

            return base.OnConnected();
        }

        private static void RepositoryEventBefore(RepositoryMonitorEvent e)
        {
            if (e.EventType == RepositoryMonitorEventType.Deleted)
            {
                if (e.EntityType == typeof(DeviceAttachment))
                {
                    var repositoryAttachment = (DeviceAttachment)e.Entity;
                    string attachmentDeviceSerialNumber;

                    using (DiscoDataContext Database = new DiscoDataContext())
                        attachmentDeviceSerialNumber = Database.DeviceAttachments.Where(a => a.Id == repositoryAttachment.Id).Select(a => a.DeviceSerialNumber).First();

                    HubContext.Clients.Group(GroupName(attachmentDeviceSerialNumber)).removeAttachment(repositoryAttachment.Id);
                }
            }
        }

        private static void RepositoryAfterEvent(RepositoryMonitorEvent e)
        {
            if (e.EventType == RepositoryMonitorEventType.Added)
            {
                if (e.EntityType == typeof(DeviceAttachment))
                {
                    var a = (DeviceAttachment)e.Entity;

                    HubContext.Clients.Group(GroupName(a.DeviceSerialNumber)).addAttachment(a.Id);
                }
            }
        }
    }
}
