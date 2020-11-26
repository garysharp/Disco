using Disco.Data.Repository;
using Disco.Services.Tasks;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Threading.Tasks;

namespace Disco.Services.Interop.ActiveDirectory
{
    public class ADDiscoverServers : ScheduledTask
    {
        public override string TaskName { get { return "Active Directory - Discover Servers"; } }
        public override bool SingleInstanceTask { get { return true; } }
        public override bool CancelInitiallySupported { get { return false; } }
        internal static List<string> AllServers { get; set; }
        private static object _scheduleLock = new object();

        protected override void ExecuteTask()
        {
            var allServers = DiscoverAllServers();
            AllServers = allServers;

            // Restrict Searching Entire Directory if to many servers
            using (DiscoDataContext Database = new DiscoDataContext())
            {
                var searchAllServers = Database.DiscoConfiguration.ActiveDirectory.SearchAllServers;

                // Check explicitly configured: No
                if (!searchAllServers.HasValue || searchAllServers.Value)
                {
                    // Not Configured, or explicitly configured: Yes
                    if (allServers.Count > ActiveDirectory.MaxAllServerSearch)
                    {
                        // Update Database
                        Database.DiscoConfiguration.ActiveDirectory.SearchAllServers = false;
                    }
                    else
                    {
                        // Default
                        Database.DiscoConfiguration.ActiveDirectory.SearchAllServers = true;
                    }

                    Database.SaveChanges();
                }
            }
        }

        internal static ScheduledTaskStatus ScheduleNow()
        {
            var taskStatus = ScheduledTasks.GetTaskStatuses(typeof(ADDiscoverServers)).Where(ts => ts.IsRunning).FirstOrDefault();
            if (taskStatus != null)
                return taskStatus;
            else
            {
                lock (_scheduleLock)
                {
                    taskStatus = ScheduledTasks.GetTaskStatuses(typeof(ADDiscoverServers)).Where(ts => ts.IsRunning).FirstOrDefault();
                    if (taskStatus != null)
                        return taskStatus;
                    else
                    {
                        var t = new ADDiscoverServers();
                        return t.ScheduleTask();
                    }
                }
            }
        }

        public static List<string> LoadAllServersSync()
        {
            if (AllServers != null)
                return AllServers;

            ScheduledTaskStatus status;
            lock (_scheduleLock)
            {
                if (AllServers != null)
                    return AllServers;

                status = ScheduleNow();
            }

            status.CompletionTask.Wait();
            return AllServers;
        }

        public static Task<List<string>> LoadAllServersAsync()
        {
            if (AllServers != null)
                return Task.FromResult(AllServers);

            ScheduledTaskStatus status;
            lock (_scheduleLock)
            {
                if (AllServers != null)
                    return Task.FromResult(AllServers);

                status = ScheduleNow();
            }

            return status.CompletionTask.ContinueWith(t =>
            {
                return AllServers;
            });
        }

        private static List<string> DiscoverAllServers()
        {
            return ActiveDirectory.Context.Domains
                .SelectMany(d => d.Domain.FindAllDomainControllers().Cast<DomainController>().Select(dc => dc.Name)).ToList();
        }
    }
}
