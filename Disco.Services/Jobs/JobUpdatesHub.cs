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
using Disco.Services.Users;

namespace Disco.Services.Jobs
{
    [HubName("jobUpdates"), DiscoHubAuthorize(Claims.Job.Show)]
    public class JobUpdatesHub : Hub
    {
        private const string JobLogsPrefix = "JobLogs_";
        private const string JobAttachmentsPrefix = "JobAttachments_";
        public static IHubContext HubContext { get; private set; }

        private static IDisposable RepositoryBeforeSubscription;
        private static IDisposable RepositoryAfterSubscription;

        static JobUpdatesHub()
        {
            HubContext = GlobalHost.ConnectionManager.GetHubContext<JobUpdatesHub>();

            // Subscribe to Repository Monitor for Changes
            RepositoryBeforeSubscription = RepositoryMonitor.StreamBeforeCommit
                .Where(e => (
                    e.EntityType == typeof(JobLog) && e.EventType == RepositoryMonitorEventType.Deleted ||
                    e.EntityType == typeof(JobAttachment) && e.EventType == RepositoryMonitorEventType.Deleted
                    ))
                .Subscribe(RepositoryEventBefore);
            RepositoryAfterSubscription = RepositoryMonitor.StreamAfterCommit
                .Where(e => (
                    e.EntityType == typeof(JobLog) && e.EventType == RepositoryMonitorEventType.Added ||
                    e.EntityType == typeof(JobAttachment) && e.EventType == RepositoryMonitorEventType.Added
                    ))
                .Subscribe(RepositoryAfterEvent);
        }

        private static string LogsGroupName(int JobId)
        {
            return JobLogsPrefix + JobId.ToString();
        }
        private static string AttachmentsGroupName(int JobId)
        {
            return JobAttachmentsPrefix + JobId.ToString();
        }

        public override Task OnConnected()
        {
            int jobId;
            string jobIdParam;

            jobIdParam = Context.QueryString["JobId"];

            if (string.IsNullOrWhiteSpace(jobIdParam))
                throw new ArgumentNullException("JobId");
            if (!int.TryParse(jobIdParam, out jobId))
                throw new ArgumentException("An integer was expected", "JobId");

            var userAuth = UserService.GetAuthorization(Context.User.Identity.Name);

            if (userAuth.Has(Claims.Job.ShowLogs))
                Groups.Add(Context.ConnectionId, LogsGroupName(jobId));
            if (userAuth.Has(Claims.Job.ShowAttachments))
                Groups.Add(Context.ConnectionId, AttachmentsGroupName(jobId));

            return base.OnConnected();
        }

        private static void RepositoryEventBefore(RepositoryMonitorEvent e)
        {
            if (e.EventType == RepositoryMonitorEventType.Deleted)
            {
                if (e.EntityType == typeof(JobLog))
                {
                    var repositoryLog = (JobLog)e.Entity;
                    int logJobId;

                    using (DiscoDataContext Database = new DiscoDataContext())
                        logJobId = Database.JobLogs.Where(l => l.Id == repositoryLog.Id).Select(a => a.JobId).First();

                    HubContext.Clients.Group(LogsGroupName(logJobId)).removeLog(repositoryLog.Id);
                }
                if (e.EntityType == typeof(JobAttachment))
                {
                    var repositoryAttachment = (JobAttachment)e.Entity;
                    int attachmentJobId;

                    using (DiscoDataContext Database = new DiscoDataContext())
                        attachmentJobId = Database.JobAttachments.Where(a => a.Id == repositoryAttachment.Id).Select(a => a.JobId).First();

                    HubContext.Clients.Group(AttachmentsGroupName(attachmentJobId)).removeAttachment(repositoryAttachment.Id);
                }
            }
        }

        private static void RepositoryAfterEvent(RepositoryMonitorEvent e)
        {
            if (e.EventType == RepositoryMonitorEventType.Added)
            {
                if (e.EntityType == typeof(JobLog))
                {
                    var a = (JobLog)e.Entity;

                    HubContext.Clients.Group(LogsGroupName(a.JobId)).addLog(a.Id);
                }
                if (e.EntityType == typeof(JobAttachment))
                {
                    var a = (JobAttachment)e.Entity;

                    HubContext.Clients.Group(AttachmentsGroupName(a.JobId)).addAttachment(a.Id);
                }
            }
        }
    }
}
