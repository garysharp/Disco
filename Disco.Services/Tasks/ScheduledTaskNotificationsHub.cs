using Disco.Services.Web.Signalling;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace Disco.Services.Tasks
{
    using System.Reactive.Subjects;
    using ChangedItem = KeyValuePair<string, object>;

    [HubName("scheduledTaskNotifications"), DiscoHubAuthorize()]
    public class ScheduledTaskNotificationsHub : Hub
    {
        private const string NotificationsPrefix = "Logging_";
        private static Subject<Tuple<string, IEnumerable<ChangedItem>>> taskUpdatesStream = new Subject<Tuple<string, IEnumerable<ChangedItem>>>();
        private static IDisposable taskUpdatesStreamSubscription;

        static ScheduledTaskNotificationsHub()
        {
            taskUpdatesStreamSubscription = taskUpdatesStream
                .DelayBuffer(TimeSpan.FromMilliseconds(200))
                .Subscribe(BroadcastBufferedEvents);
        }

        internal static void PublishEvent(string TaskSessionId, IEnumerable<ChangedItem> ChangedItems)
        {
            taskUpdatesStream.OnNext(Tuple.Create(TaskSessionId, ChangedItems));
        }

        public override System.Threading.Tasks.Task OnConnected()
        {
            var taskSessionId = Context.QueryString["TaskSessionId"];

            // Send Status:
            var currentStatus = GetScheduledTaskLiveStatus(taskSessionId);
            Clients.Caller.initializeTaskStatus(currentStatus);

            // Add to Group
            var groupName = NotificationsPrefix + taskSessionId;
            Groups.Add(Context.ConnectionId, groupName);

            return base.OnConnected();
        }

        private static void BroadcastBufferedEvents(IEnumerable<Tuple<string, IEnumerable<ChangedItem>>> Events)
        {
            var connectionManager = GlobalHost.ConnectionManager;
            var context = connectionManager.GetHubContext<ScheduledTaskNotificationsHub>();

            var taskStatusEvents = Events.GroupBy(e => e.Item1).Select(taskEventsGroup =>
            {
                Dictionary<string, object> changes = new Dictionary<string, object>();

                foreach (var changeEvents in taskEventsGroup.Select(taskEvents => taskEvents.Item2))
                    foreach (var changeEvent in changeEvents)
                        changes[changeEvent.Key] = changeEvent.Value;

                return Tuple.Create(taskEventsGroup.Key, changes);
            });

            foreach (var taskStatusEvent in taskStatusEvents)
            {
                var groupName = NotificationsPrefix + taskStatusEvent.Item1;
                context.Clients.Group(groupName).updateTaskStatus(taskStatusEvent.Item2);
            }
        }

        private ScheduledTaskStatusLive GetScheduledTaskLiveStatus(string TaskSessionId)
        {
            if (string.IsNullOrEmpty(TaskSessionId))
                throw new ArgumentNullException("TaskSessionId");

            var status = ScheduledTasks.GetTaskStatus(TaskSessionId);

            if (status == null)
                throw new ArgumentException("Invalid ScheduledTask SessionId", "TaskSessionId");

            // Send Status:
            return ScheduledTaskStatusLive.FromScheduledTaskStatus(status, null);
        }

        public ScheduledTaskStatusLive GetStatus()
        {
            var taskSessionId = Context.QueryString["TaskSessionId"];

            return GetScheduledTaskLiveStatus(taskSessionId);
        }
    }
}
