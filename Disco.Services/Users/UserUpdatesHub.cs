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

namespace Disco.Services.Users
{
    [HubName("userUpdates"), DiscoHubAuthorizeAll(Claims.User.Show, Claims.User.ShowAttachments)]
    public class UserUpdatesHub : Hub
    {
        private const string UserPrefix = "User_";
        public static IHubContext HubContext { get; private set; }

        private static IDisposable RepositoryBeforeSubscription;
        private static IDisposable RepositoryAfterSubscription;

        static UserUpdatesHub()
        {
            HubContext = GlobalHost.ConnectionManager.GetHubContext<UserUpdatesHub>();

            // Subscribe to Repository Monitor for Changes
            RepositoryBeforeSubscription = RepositoryMonitor.StreamBeforeCommit
                .Where(e => e.EntityType == typeof(UserAttachment) && e.EventType == RepositoryMonitorEventType.Deleted)
                .Subscribe(RepositoryEventBefore);
            RepositoryAfterSubscription = RepositoryMonitor.StreamAfterCommit
                .Where(e => e.EntityType == typeof(UserAttachment) && e.EventType == RepositoryMonitorEventType.Added)
                .Subscribe(RepositoryAfterEvent);
        }

        private static string GroupName(string UserId)
        {
            return UserPrefix + UserId;
        }

        public override Task OnConnected()
        {
            var userId = Context.QueryString["UserId"];

            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException("UserId");

            Groups.Add(Context.ConnectionId, GroupName(userId));

            return base.OnConnected();
        }

        private static void RepositoryEventBefore(RepositoryMonitorEvent e)
        {
            if (e.EventType == RepositoryMonitorEventType.Deleted)
            {
                if (e.EntityType == typeof(UserAttachment))
                {
                    var repositoryAttachment = (UserAttachment)e.Entity;
                    string attachmentUserId;

                    using (DiscoDataContext Database = new DiscoDataContext())
                        attachmentUserId = Database.UserAttachments.Where(a => a.Id == repositoryAttachment.Id).Select(a => a.UserId).First();

                    HubContext.Clients.Group(GroupName(attachmentUserId)).removeAttachment(repositoryAttachment.Id);
                }
            }
        }

        private static void RepositoryAfterEvent(RepositoryMonitorEvent e)
        {
            if (e.EventType == RepositoryMonitorEventType.Added)
            {
                if (e.EntityType == typeof(UserAttachment))
                {
                    var a = (UserAttachment)e.Entity;

                    HubContext.Clients.Group(GroupName(a.UserId)).addAttachment(a.Id);
                }
            }
        }
    }
}
