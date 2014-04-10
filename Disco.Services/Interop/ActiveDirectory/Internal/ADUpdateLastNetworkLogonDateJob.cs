using Disco.Data.Repository;
using Disco.Models.Interop.ActiveDirectory;
using Disco.Models.Repository;
using Disco.Services.Logging;
using Disco.Services.Tasks;
using Quartz;
using System;
using System.Collections;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;

namespace Disco.Services.Interop.ActiveDirectory.Internal
{
    public class ADUpdateLastNetworkLogonDateJob : ScheduledTask
    {

        public override string TaskName { get { return "Active Directory - Update Last Network Logon Dates Task"; } }
        public override bool SingleInstanceTask { get { return true; } }
        public override bool CancelInitiallySupported { get { return false; } }

        public override void InitalizeScheduledTask(DiscoDataContext Database)
        {
            // ActiveDirectoryUpdateLastNetworkLogonDateJob @ 11:30pm
            TriggerBuilder triggerBuilder = TriggerBuilder.Create().
                WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(23, 30));

            this.ScheduleTask(triggerBuilder);
        }

        protected override void ExecuteTask()
        {
            int changeCount;

            this.Status.UpdateStatus(1, "Starting", "Connecting to the Database and initializing the environment");
            using (DiscoDataContext database = new DiscoDataContext())
            {
                UpdateLastNetworkLogonDates(database, this.Status);
                this.Status.UpdateStatus(95, "Updating Database", "Writing last network logon dates to the Database");
                changeCount = database.SaveChanges();
                this.Status.Finished(string.Format("{0} Device last network logon dates updated", changeCount), "/Config/SystemConfig");
            }

            SystemLog.LogInformation(new string[]
                {
                    "Updated LastNetworkLogon Device Property for Device/s", 
                    changeCount.ToString()
                });
        }

        public static ScheduledTaskStatus ScheduleImmediately()
        {
            var existingTask = ScheduledTasks.GetTaskStatuses(typeof(ADUpdateLastNetworkLogonDateJob)).Where(s => s.IsRunning).FirstOrDefault();
            if (existingTask != null)
                return existingTask;

            var instance = new ADUpdateLastNetworkLogonDateJob();
            return instance.ScheduleTask();
        }

        public static bool UpdateLastNetworkLogonDate(Device Device)
        {
            const string ldapFilterTemplate = "(&(objectCategory=Computer)(sAMAccountName={0}))";
            string[] ldapProperties = new string[] { "lastLogon", "lastLogonTimestamp" };

            System.DateTime? lastLogon = null;

            if (!string.IsNullOrEmpty(Device.DeviceDomainId))
            {
                var deviceSamAccountName = UserExtensions.SplitUserId(Device.DeviceDomainId).Item2 + "$";
                var ldapFilter = string.Format(ldapFilterTemplate, ADInterop.EscapeLdapQuery(deviceSamAccountName));

                var domain = ADInterop.GetDomainFromId(Device.DeviceDomainId);
                IEnumerable<DomainController> domainControllers;

                if (ADInterop.SearchEntireForest)
                    domainControllers = domain.RetrieveReachableDomainControllers();
                else
                    domainControllers = ADInterop.Site.RetrieveReachableDomainControllers(domain);

                lastLogon = domainControllers.Select(dc =>
                {
                    using (var directoryRoot = dc.RetrieveDirectoryEntry(domain.DistinguishedName))
                    {
                        using (var directorySearcher = new DirectorySearcher(directoryRoot, ldapFilter, ldapProperties, SearchScope.Subtree))
                        {
                            var directoryResult = directorySearcher.FindOne();

                            if (directoryResult != null)
                            {
                                long lastLogonValue = default(long);
                                long lastLogonTimestampValue = default(long);

                                var lastLogonProperty = directoryResult.Properties["lastLogon"];
                                if (lastLogonProperty != null && lastLogonProperty.Count > 0)
                                    lastLogonValue = (long)lastLogonProperty[0];
                                var lastLogonTimestampProperty = directoryResult.Properties["lastLogonTimestamp"];
                                if (lastLogonTimestampProperty != null && lastLogonTimestampProperty.Count > 0)
                                    lastLogonTimestampValue = (long)lastLogonTimestampProperty[0];

                                long highedValue = Math.Max(lastLogonValue, lastLogonTimestampValue);

                                if (highedValue > 0)
                                    return (DateTime?)new DateTime((DateTime.FromFileTime(highedValue).Ticks / 10000000L) * 10000000L);
                                else
                                    return null;
                            }
                        }
                    }
                    return null;
                }).Where(dt => dt.HasValue).Max();
            }

            if (lastLogon.HasValue &&
                (
                !Device.LastNetworkLogonDate.HasValue
                || Device.LastNetworkLogonDate.Value < lastLogon
                ))
            {
                Device.LastNetworkLogonDate = lastLogon;
                return true;
            }
            return false;
        }

        private static void UpdateLastNetworkLogonDates(DiscoDataContext Database, ScheduledTaskStatus status)
        {
            const string ldapFilter = "(objectCategory=Computer)";
            string[] ldapProperties = new string[] { "sAMAccountName", "lastLogon" };

            status.UpdateStatus(2, "Initializing", "Determining Domains and Available Domain Controllers");

            // Determine Domain Controllers to Query
            IEnumerable<Tuple<ActiveDirectoryDomain, DomainController>> domainControllers;
            if (ADInterop.SearchEntireForest)
                domainControllers = ADInterop.Domains.SelectMany(d => d.RetrieveReachableDomainControllers(), (d, dc) => Tuple.Create(d, dc));
            else
                domainControllers = ADInterop.Domains.SelectMany(d => ADInterop.Site.RetrieveReachableDomainControllers(d), (d, dc) => Tuple.Create(d, dc));

            // Determine Queries
            var requiredRueries = domainControllers
                .Where(s => s.Item1.SearchContainers != null && s.Item1.SearchContainers.Count > 0)
                .SelectMany(s => s.Item1.SearchContainers, (s, c) => Tuple.Create(s.Item1, s.Item2, c)).ToList();

            var queries = Enumerable.Range(0, requiredRueries.Count).Select(i =>
            {
                var q = requiredRueries[i];
                return Tuple.Create(i, q.Item1, q.Item2, q.Item3);
            });

            var queryResults = queries.SelectMany(q =>
            {
                var queryIndex = q.Item1;
                var domain = q.Item2;
                var domainController = q.Item3;
                var searchRoot = q.Item4;

                // Update Status
                double progress = 5 + (queryIndex * (90 / requiredRueries.Count));
                status.UpdateStatus(progress, string.Format("Querying Domain [{0}] using controller [{1}]", domain.NetBiosName, domainController.Name), string.Format("Searching: {0}", searchRoot));

                // Perform Query
                using (var directoryRoot = domainController.RetrieveDirectoryEntry(searchRoot))
                {
                    using (var directorySearcher = new DirectorySearcher(directoryRoot, ldapFilter, ldapProperties, SearchScope.Subtree))
                    {
                        directorySearcher.PageSize = 500;

                        var directoryResults = directorySearcher.FindAll();

                        if (directoryResults != null)
                        {
                            return directoryResults.Cast<SearchResult>().Select(result =>
                            {
                                var samAccountProperity = result.Properties["sAMAccountName"];


                                long lastLogonValue = default(long);
                                long lastLogonTimestampValue = default(long);

                                var lastLogonProperty = result.Properties["lastLogon"];
                                if (lastLogonProperty != null && lastLogonProperty.Count > 0)
                                    lastLogonValue = (long)lastLogonProperty[0];
                                var lastLogonTimestampProperty = result.Properties["lastLogonTimestamp"];
                                if (lastLogonTimestampProperty != null && lastLogonTimestampProperty.Count > 0)
                                    lastLogonTimestampValue = (long)lastLogonTimestampProperty[0];

                                long highedValue = Math.Max(lastLogonValue, lastLogonTimestampValue);

                                if (highedValue > 0)
                                {
                                    var computerName = string.Format(@"{0}\{1}", domain.NetBiosName, samAccountProperity[0].ToString().TrimEnd('$'));
                                    var lastLogon = new DateTime((DateTime.FromFileTime(highedValue).Ticks / 10000000L) * 10000000L);
                                    return Tuple.Create(computerName, lastLogon);
                                }
                                else
                                    return null;
                            }).Where(i => i != null).ToList();
                        }
                        else
                        {
                            return Enumerable.Empty<Tuple<string, DateTime>>();
                        }
                    }
                }
            }).GroupBy(r => r.Item1, StringComparer.InvariantCultureIgnoreCase).ToDictionary(g => g.Key.ToUpper(), g => g.Max(i => i.Item2));

            status.UpdateStatus(90, "Processing Results", "Processing last network logon dates and looking for updates");

            foreach (Device device in Database.Devices.Where(device => device.DeviceDomainId != null))
            {
                DateTime lastLogonDate;
                if (queryResults.TryGetValue(device.DeviceDomainId.ToUpper(), out lastLogonDate))
                {
                    if (!device.LastNetworkLogonDate.HasValue)
                        device.LastNetworkLogonDate = lastLogonDate;
                    else
                    {
                        // Change accuracy to the second
                        lastLogonDate = new DateTime((lastLogonDate.Ticks / 10000000L) * 10000000L);

                        if (device.LastNetworkLogonDate.Value < lastLogonDate)
                            device.LastNetworkLogonDate = lastLogonDate;
                    }
                }
            }
        }

    }
}
