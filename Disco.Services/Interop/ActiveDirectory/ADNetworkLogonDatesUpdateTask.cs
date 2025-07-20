using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Services.Tasks;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Disco.Services.Interop.ActiveDirectory
{
    public class ADNetworkLogonDatesUpdateTask : ScheduledTask
    {
        public override string TaskName { get { return "Active Directory - Update Last Network Logon Dates Task"; } }
        public override bool SingleInstanceTask { get { return true; } }
        public override bool CancelInitiallySupported { get { return false; } }

        public override void InitalizeScheduledTask(DiscoDataContext Database)
        {
            // ADNetworkLogonDatesUpdateTask @ 11:30pm
            TriggerBuilder triggerBuilder = TriggerBuilder.Create().
                WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(23, 30));

            ScheduleTask(triggerBuilder);
        }

        protected override void ExecuteTask()
        {
            int changeCount;

            Status.UpdateStatus(1, "Starting", "Connecting to the Database and initializing the environment");
            using (DiscoDataContext database = new DiscoDataContext())
            {
                UpdateLastNetworkLogonDates(database, Status);
                Status.UpdateStatus(95, "Updating Database", "Writing last network logon dates to the Database");
                changeCount = database.SaveChanges();
                Status.Finished($"{changeCount} Device last network logon dates updated", "/Config/SystemConfig");
            }

            Status.LogInformation($"Updated LastNetworkLogon Device Property for Device/s, {changeCount:N0} changes");
        }

        public static ScheduledTaskStatus ScheduleImmediately()
        {
            var existingTask = ScheduledTasks.GetTaskStatuses(typeof(ADNetworkLogonDatesUpdateTask)).Where(s => s.IsRunning).FirstOrDefault();
            if (existingTask != null)
                return existingTask;

            var instance = new ADNetworkLogonDatesUpdateTask();
            return instance.ScheduleTask();
        }

        public static bool UpdateLastNetworkLogonDate(Device Device)
        {
            const string ldapFilterTemplate = "(&(objectCategory=Computer)(sAMAccountName={0}))";
            string[] ldapProperties = new string[] { "lastLogon", "lastLogonTimestamp" };

            DateTime? lastLogon = null;

            if (!string.IsNullOrEmpty(Device.DeviceDomainId) && Device.DeviceDomainId.Contains('\\'))
            {
                var context = ActiveDirectory.Context;
                string deviceSamAccountName;
                ADDomain deviceDomain;

                ActiveDirectory.ParseDomainAccountId(Device.DeviceDomainId + "$", out deviceSamAccountName, out deviceDomain);
                
                var ldapFilter = string.Format(ldapFilterTemplate, ADHelpers.EscapeLdapQuery(deviceSamAccountName));
                IEnumerable<ADDomainController> domainControllers;

                if (context.SearchAllServers)
                    domainControllers = deviceDomain.GetAllReachableDomainControllers();
                else
                    domainControllers = deviceDomain.GetReachableSiteDomainControllers();

                lastLogon = domainControllers.Select(dc =>
                {
                    var result = dc.SearchEntireDomain(ldapFilter, ldapProperties, ActiveDirectory.SingleSearchResult).FirstOrDefault();

                    if (result != null)
                    {
                        long lastLogonValue = default(long);
                        long lastLogonTimestampValue = default(long);

                        lastLogonValue = result.Value<long>("lastLogon");
                        lastLogonTimestampValue = result.Value<long>("lastLogonTimestamp");

                        long highedValue = Math.Max(lastLogonValue, lastLogonTimestampValue);

                        if (highedValue > 0)
                            return (DateTime?)new DateTime((DateTime.FromFileTime(highedValue).Ticks / 10000000L) * 10000000L);
                        else
                            return null;
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

        public static void UpdateLastNetworkLogonDates(DiscoDataContext Database, IScheduledTaskStatus status)
        {
            var context = ActiveDirectory.Context;
            const string ldapFilter = "(objectCategory=Computer)";
            string[] ldapProperties = new string[] { "sAMAccountName", "lastLogon" };

            status.UpdateStatus(2, "Initializing", "Determining Domains and Available Domain Controllers");

            // Determine Domain Scopes to Query
            var domainQueries = context.Domains
                .Select(d => Tuple.Create(d, d.SearchContainers ?? new List<string>() { d.DistinguishedName }))
                .Where(d => d.Item2.Count > 0);

            // Determine Domain Controllers to Query
            IEnumerable<Tuple<ADDomain, ADDomainController, List<string>>> serverQueries;
            if (context.SearchAllServers)
                serverQueries = domainQueries.SelectMany(q => q.Item1.GetAllReachableDomainControllers(), (q, dc) => Tuple.Create(q.Item1, dc, q.Item2));
            else
                serverQueries = domainQueries.SelectMany(q => q.Item1.GetReachableSiteDomainControllers(), (q, dc) => Tuple.Create(q.Item1, dc, q.Item2));

            var scopedQueries = serverQueries.SelectMany(q => q.Item3, (q, scope) => Tuple.Create(q.Item1, q.Item2, scope)).ToList();

            var queries = Enumerable.Range(0, scopedQueries.Count).Select(i =>
            {
                var q = scopedQueries[i];
                return Tuple.Create(i, q.Item1, q.Item2, q.Item3);
            });

            var queryResults = queries.SelectMany(q =>
            {
                var queryIndex = q.Item1;
                var domain = q.Item2;
                var domainController = q.Item3;
                var searchRoot = q.Item4;

                // Update Status
                double progress = 5 + (queryIndex * (90 / scopedQueries.Count));
                status.UpdateStatus(progress, $"Querying Domain [{domain.NetBiosName}] using controller [{domainController.Name}]", $"Searching: {searchRoot}");

                // Perform Query
                var directoryResults = domainController.SearchInternal(searchRoot, ldapFilter, ldapProperties, null);

                return directoryResults.Select(result =>
                {
                    var samAccountName = result.Value<string>("sAMAccountName");

                    long lastLogonValue = default(long);
                    long lastLogonTimestampValue = default(long);

                    lastLogonValue = result.Value<long>("lastLogon");
                    lastLogonTimestampValue = result.Value<long>("lastLogonTimestamp");

                    long highedValue = Math.Max(lastLogonValue, lastLogonTimestampValue);

                    if (highedValue > 0)
                    {
                        var computerName = $@"{domain.NetBiosName}\{samAccountName.TrimEnd('$')}";
                        var lastLogon = new DateTime((DateTime.FromFileTime(highedValue).Ticks / 10000000L) * 10000000L);
                        return Tuple.Create(computerName, lastLogon);
                    }
                    else
                        return null;
                }).Where(i => i != null).ToList();

            }).GroupBy(r => r.Item1, StringComparer.OrdinalIgnoreCase).ToDictionary(g => g.Key.ToUpper(), g => g.Max(i => i.Item2));

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
