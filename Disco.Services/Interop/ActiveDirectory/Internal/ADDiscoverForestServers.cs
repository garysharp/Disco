using Disco.Data.Repository;
using Disco.Services.Tasks;
using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Services.Interop.ActiveDirectory.Internal
{
    public class ADDiscoverForestServers : ScheduledTask
    {
        public override string TaskName { get { return "Active Directory - Discover Forest Servers"; } }
        public override bool SingleInstanceTask { get { return true; } }
        public override bool CancelInitiallySupported { get { return false; } }

        protected override void ExecuteTask()
        {
            var forestServers = DiscoverForestServers();
            ADInterop._ForestServers = forestServers;

            // Restrict Searching Entire Forest if to many servers
            using (DiscoDataContext Database = new DiscoDataContext())
            {
                var searchEntireForest = Database.DiscoConfiguration.ActiveDirectory.SearchEntireForest;

                // Check explicitly configured: No
                if (!searchEntireForest.HasValue || searchEntireForest.Value)
                {
                    // Not Configured, or explicitly configured: Yes
                    if (forestServers.Count > ActiveDirectory.MaxForestServerSearch)
                    {
                        // Update Database
                        Database.DiscoConfiguration.ActiveDirectory.SearchEntireForest = false;
                    }
                    else
                    {
                        // Default
                        Database.DiscoConfiguration.ActiveDirectory.SearchEntireForest = true;
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
                var t = new ADDiscoverForestServers();
                return t.ScheduleTask();
            }
        }

        internal static List<string> DiscoverForestServers()
        {
            using (var computerDomain = Domain.GetComputerDomain())
            {
                return computerDomain.Forest.Domains.Cast<Domain>().SelectMany(d => d.FindAllDomainControllers().Cast<DomainController>()).Select(dc => dc.Name).ToList();
            }
        }
    }
}
