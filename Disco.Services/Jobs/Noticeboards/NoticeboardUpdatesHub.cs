using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System;
using System.Threading.Tasks;

namespace Disco.Services.Jobs.Noticeboards
{
    [HubName("noticeboardUpdates")] // Public
    public class NoticeboardUpdatesHub : Hub
    {
        public static IHubContext HubContext { get; private set; }

        static NoticeboardUpdatesHub()
        {
            HubContext = GlobalHost.ConnectionManager.GetHubContext<NoticeboardUpdatesHub>();
        }

        public override Task OnConnected()
        {
            var noticeboardId = Context.QueryString["Noticeboard"];

            if (string.IsNullOrWhiteSpace(noticeboardId))
                throw new ArgumentNullException("Noticeboard");

            switch (noticeboardId)
            {
                case HeldDevicesForUsers.Name:
                    Groups.Add(Context.ConnectionId, HeldDevicesForUsers.Name);
                    break;
                case HeldDevices.Name:
                    Groups.Add(Context.ConnectionId, HeldDevices.Name);
                    break;
                default:
                    throw new ArgumentException("Invalid Noticeboard Specified", "Noticeboard");
            }

            return base.OnConnected();
        }
    }
}
