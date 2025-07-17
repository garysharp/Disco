using Disco.Data.Repository.Monitor;
using Disco.Models.Repository;
using Disco.Services.Authorization;
using Disco.Services.Users;
using Disco.Services.Web.Signalling;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Disco.Services.Devices
{
    [HubName("deviceUpdates"), DiscoHubAuthorize(Claims.Device.Show)]
    public class DeviceUpdatesHub : Hub
    {
        public static IHubContext HubContext { get; }

        private readonly static IDisposable RepositoryBeforeSubscription;
        private readonly static IDisposable RepositoryAfterSubscription;

        static DeviceUpdatesHub()
        {
            HubContext = GlobalHost.ConnectionManager.GetHubContext<DeviceUpdatesHub>();

            // Subscribe to Repository Monitor for Changes
            RepositoryBeforeSubscription = RepositoryMonitor.StreamBeforeCommit
                .Where(e =>
                    e.EventType == RepositoryMonitorEventType.Deleted && (
                        e.EntityType == typeof(DeviceComment) ||
                        e.EntityType == typeof(DeviceAttachment)
                        )
                ).Subscribe(RepositoryEventBefore);
            RepositoryAfterSubscription = RepositoryMonitor.StreamAfterCommit
                .Where(e =>
                    e.EventType == RepositoryMonitorEventType.Added && (
                        e.EntityType == typeof(DeviceComment) ||
                        e.EntityType == typeof(DeviceAttachment)
                        )
                ).Subscribe(RepositoryAfterEvent);
        }

        private static bool TryAttachmentGroupName(RepositoryMonitorEvent e, out string groupName)
        {
            var deviceSerialNumber = e.GetPreviousPropertyValue<string>(nameof(DeviceAttachment.DeviceSerialNumber));
            if (deviceSerialNumber == null)
            {
                groupName = null;
                return false;
            }
            groupName = AttachmentGroupName(deviceSerialNumber);
            return true;
        }

        private static string AttachmentGroupName(string deviceSerialNumber)
            => $"Device_Attachment_{deviceSerialNumber.ToLowerInvariant()}";

        private static bool TryCommentGroupName(RepositoryMonitorEvent e, out string groupName)
        {
            var deviceSerialNumber = e.GetPreviousPropertyValue<string>(nameof(DeviceComment.DeviceSerialNumber));
            if (deviceSerialNumber == null)
            {
                groupName = null;
                return false;
            }
            groupName = CommentGroupName(deviceSerialNumber);
            return true;
        }

        private static string CommentGroupName(string deviceSerialNumber)
            => $"Device_Comment_{deviceSerialNumber.ToLowerInvariant()}";

        public override Task OnConnected()
        {
            var deviceSerialNumber = Context.QueryString["DeviceSerialNumber"];

            if (string.IsNullOrWhiteSpace(deviceSerialNumber))
                throw new ArgumentNullException("DeviceSerialNumber");

            var authorization = UserService.GetAuthorization(Context.User.Identity.Name);

            if (authorization.Has(Claims.Device.ShowComments))
                Groups.Add(Context.ConnectionId, CommentGroupName(deviceSerialNumber));
            if (authorization.Has(Claims.Device.ShowAttachments))
                Groups.Add(Context.ConnectionId, AttachmentGroupName(deviceSerialNumber));

            return base.OnConnected();
        }

        private static void RepositoryEventBefore(RepositoryMonitorEvent e)
        {
            if (e.EventType == RepositoryMonitorEventType.Deleted)
            {
                if (e.Entity is DeviceComment comment && TryCommentGroupName(e, out var commentGroupName))
                    HubContext.Clients.Group(commentGroupName).commentRemoved(comment.Id);
                else if (e.Entity is DeviceAttachment attachment && TryAttachmentGroupName(e, out var attachmentGroupName))
                    HubContext.Clients.Group(attachmentGroupName).attachmentRemoved(attachment.Id);
            }
        }

        private static void RepositoryAfterEvent(RepositoryMonitorEvent e)
        {
            if (e.EventType == RepositoryMonitorEventType.Added)
            {
                if (e.Entity is DeviceComment comment)
                    HubContext.Clients.Group(CommentGroupName(comment.DeviceSerialNumber)).commentAdded(comment.Id);
                else if (e.Entity is DeviceAttachment attachment)
                    HubContext.Clients.Group(AttachmentGroupName(attachment.DeviceSerialNumber)).attachmentAdded(attachment.Id);
            }
        }
    }
}
