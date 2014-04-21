using Disco.Data.Repository;
using Disco.Services.Tasks;
using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Services.Interop.ActiveDirectory
{
    public class ADDiscoverForestServers : ScheduledTask
    {
        public override string TaskName { get { return "Active Directory - Discover Forest Servers"; } }
        public override bool SingleInstanceTask { get { return true; } }
        public override bool CancelInitiallySupported { get { return false; } }
        internal static List<string> ForestServers { get; set; }
        private static object _scheduleLock = new object();

        protected override void ExecuteTask()
        {
            var forestServers = DiscoverForestServers();
            ADDiscoverForestServers.ForestServers = forestServers;

            // Restrict Searching Entire Forest if to many servers
            using (DiscoDataContext Database = new DiscoDataContext())
            {
                var searchEntireForest = Database.DiscoConfiguration.ActiveDirectory.SearchAllForestServers;

                // Check explicitly configured: No
                if (!searchEntireForest.HasValue || searchEntireForest.Value)
                {
                    // Not Configured, or explicitly configured: Yes
                    if (forestServers.Count > ActiveDirectory.MaxForestServerSearch)
                    {
                        // Update Database
                        Database.DiscoConfiguration.ActiveDirectory.SearchAllForestServers = false;
                    }
                    else
                    {
                        // Default
                        Database.DiscoConfiguration.ActiveDirectory.SearchAllForestServers = true;
                    }

                    Database.SaveChanges();
                }
            }
        }

        internal static ScheduledTaskStatus ScheduleNow()
        {
            var taskStatus = ScheduledTasks.GetTaskStatuses(typeof(ADDiscoverForestServers)).Where(ts => ts.IsRunning).FirstOrDefault();
            if (taskStatus != null)
                return taskStatus;
            else
            {
                lock (_scheduleLock)
                {
                    taskStatus = ScheduledTasks.GetTaskStatuses(typeof(ADDiscoverForestServers)).Where(ts => ts.IsRunning).FirstOrDefault();
                    if (taskStatus != null)
                        return taskStatus;
                    else
                    {
                        var t = new ADDiscoverForestServers();
                        return t.ScheduleTask();
                    }
                }
            }
        }

        public static List<string> LoadForestServersBlocking()
        {
            if (ADDiscoverForestServers.ForestServers != null)
                return ADDiscoverForestServers.ForestServers;

            ScheduledTaskStatus status;
            lock (_scheduleLock)
            {
                if (ADDiscoverForestServers.ForestServers != null)
                    return ADDiscoverForestServers.ForestServers;

                status = ADDiscoverForestServers.ScheduleNow();
            }

            status.CompletionTask.Wait();
            return ForestServers;
        }

        public static Task<List<string>> LoadForestServersAsync()
        {
            if (ADDiscoverForestServers.ForestServers != null)
                return Task.FromResult(ADDiscoverForestServers.ForestServers);

            ScheduledTaskStatus status;
            lock (_scheduleLock)
            {
                if (ADDiscoverForestServers.ForestServers != null)
                    return Task.FromResult(ADDiscoverForestServers.ForestServers);

                status = ADDiscoverForestServers.ScheduleNow();
            }

            return status.CompletionTask.ContinueWith(t =>
            {
                return ADDiscoverForestServers.ForestServers;
            });
        }

        private static List<string> DiscoverForestServers()
        {
            using (var computerDomain = Domain.GetComputerDomain())
            {
                return computerDomain.Forest.Domains.Cast<Domain>().SelectMany(d => d.FindAllDomainControllers().Cast<DomainController>()).Select(dc => dc.Name).ToList();
            }
        }
    }
}
