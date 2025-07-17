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

namespace Disco.Services.Users
{
    [HubName("userUpdates"), DiscoHubAuthorize(Claims.User.Show)]
    public class UserUpdatesHub : Hub
    {
        public static IHubContext HubContext { get; }

        private static readonly IDisposable repositoryBeforeSubscription;
        private static readonly IDisposable repositoryAfterSubscription;

        static UserUpdatesHub()
        {
            HubContext = GlobalHost.ConnectionManager.GetHubContext<UserUpdatesHub>();

            // Subscribe to Repository Monitor for Changes
            repositoryBeforeSubscription = RepositoryMonitor.StreamBeforeCommit
                .Where(e =>
                    e.EventType == RepositoryMonitorEventType.Deleted && (
                        e.EntityType == typeof(UserComment) ||
                        e.EntityType == typeof(UserAttachment)
                        )
                ).Subscribe(RepositoryEventBefore);
            repositoryAfterSubscription = RepositoryMonitor.StreamAfterCommit
                .Where(e =>
                    e.EventType == RepositoryMonitorEventType.Added && (
                        e.EntityType == typeof(UserComment) ||
                        e.EntityType == typeof(UserAttachment)
                        )
                ).Subscribe(RepositoryAfterEvent);
        }

        private static bool TryAttachmentGroupName(RepositoryMonitorEvent e, out string groupName)
        {
            var userId = e.GetPreviousPropertyValue<string>(nameof(UserAttachment.UserId));
            if (userId == null)
            {
                groupName = null;
                return false;
            }
            groupName = AttachmentGroupName(userId);
            return true;
        }

        private static string AttachmentGroupName(string userId)
            => $"User_Attachment_{userId.ToLowerInvariant()}";

        private static bool TryCommentGroupName(RepositoryMonitorEvent e, out string groupName)
        {
            var userId = e.GetPreviousPropertyValue<string>(nameof(UserComment.UserId));
            if (userId == null)
            {
                groupName = null;
                return false;
            }
            groupName = CommentGroupName(userId);
            return true;
        }

        private static string CommentGroupName(string userId)
            => $"User_Comment_{userId.ToLowerInvariant()}";

        public override Task OnConnected()
        {
            var userId = Context.QueryString["UserId"];

            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException("UserId");

            var authorization = UserService.GetAuthorization(Context.User.Identity.Name);

            if (authorization.Has(Claims.User.ShowComments))
                Groups.Add(Context.ConnectionId, CommentGroupName(userId));
            if (authorization.Has(Claims.User.ShowAttachments))
                Groups.Add(Context.ConnectionId, AttachmentGroupName(userId));

            return base.OnConnected();
        }

        private static void RepositoryEventBefore(RepositoryMonitorEvent e)
        {
            if (e.EventType == RepositoryMonitorEventType.Deleted)
            {
                if (e.Entity is UserComment comment && TryCommentGroupName(e, out var commentGroupName))
                    HubContext.Clients.Group(commentGroupName).commentRemoved(comment.Id);
                else if (e.Entity is UserAttachment attachment && TryAttachmentGroupName(e, out var attachmentGroupName))
                    HubContext.Clients.Group(attachmentGroupName).attachmentRemoved(attachment.Id);
            }
        }

        private static void RepositoryAfterEvent(RepositoryMonitorEvent e)
        {
            if (e.EventType == RepositoryMonitorEventType.Added)
            {
                if (e.Entity is UserComment comment)
                    HubContext.Clients.Group(CommentGroupName(comment.UserId)).commentAdded(comment.Id);
                else if (e.Entity is UserAttachment attachment)
                    HubContext.Clients.Group(AttachmentGroupName(attachment.UserId)).attachmentAdded(attachment.Id);
            }
        }
    }
}
