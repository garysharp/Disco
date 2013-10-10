using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quartz;
using Disco.Models.BI.Job.Statistics;
using Disco.Data.Repository;
using Quartz.Impl;
using Disco.Services.Tasks;
using System.Reactive.Linq;
using Disco.Models.Repository;
using Disco.Data.Repository.Monitor;

namespace Disco.BI.JobBI.Statistics
{
    public class DailyOpenedClosed : ScheduledTask
    {

        private static List<DailyOpenedClosedItem> _data;
        private static object _dataLock = new object();
        private static IDisposable _streamSubscription;


        public override string TaskName { get { return "Job Statistics - Daily Opened/Closed Task"; } }
        public override bool SingleInstanceTask { get { return true; } }
        public override bool CancelInitiallySupported { get { return false; } }

        public override void InitalizeScheduledTask(DiscoDataContext Database)
        {
            // Trigger Daily @ 12:29am
            TriggerBuilder triggerBuilder = TriggerBuilder.Create().WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(0, 29));

            this.ScheduleTask(triggerBuilder);
        }
        protected override void ExecuteTask()
        {
            using (var database = new DiscoDataContext())
            {
                UpdateDataHistory(database, true);
            }
        }

        private static void UpdateDataHistory(DiscoDataContext Database, bool Refresh = false)
        {
            DateTime historyEnd = DateTime.Now.Date;

            if (Refresh || _data == null || _data.Count == 0 || _data.Last().Timestamp < historyEnd)
            {
                lock (_dataLock)
                {
                    if (Refresh || _data == null || _data.Count == 0 || _data.Last().Timestamp < historyEnd)
                    {
                        DateTime historyStart = DateTime.Now.AddDays(-28).Date;

                        // Initialize Memory Store
                        List<DailyOpenedClosedItem> resultData;
                        if (Refresh || _data == null)
                            resultData = new List<DailyOpenedClosedItem>();
                        else
                            resultData = _data;

                        // Remove Old Data
                        while (resultData.Count > 0 && resultData[0].Timestamp < historyStart)
                            resultData.RemoveAt(0);

                        // Calculate Update Scope
                        DateTime processDate = historyStart;
                        if (resultData.Count > 0)
                            processDate = resultData.Last().Timestamp.AddDays(-1);

                        // Cache Data
                        while (processDate <= historyEnd)
                        {
                            resultData.Add(Data(Database, processDate));
                            processDate = processDate.AddDays(1);
                        }
                        _data = resultData;

                        // Subscribe to Live Repository Events
                        if (_streamSubscription != null)
                            _streamSubscription.Dispose();
                        _streamSubscription = Disco.Data.Repository.Monitor.RepositoryMonitor.StreamBeforeCommit.Where(
                            e => e.EntityType == typeof(Job) &&
                                (e.EventType == RepositoryMonitorEventType.Added || (e.EventType == RepositoryMonitorEventType.Modified && e.ModifiedProperties.Contains("ClosedDate")))).Subscribe(RepositoryEvent_JobChange);
                    }
                }
            }
        }

        private static void RepositoryEvent_JobChange(RepositoryMonitorEvent e)
        {

            if (e.EventType == RepositoryMonitorEventType.Added)
            {
                // New Job
                var todaysStats = _data.Last();
                todaysStats.OpenedJobs += 1;
                todaysStats.TotalJobs += 1;
            }
            else
            {
                DateTime? previousValue = e.GetPreviousPropertyValue<DateTime?>("ClosedDate");
                DateTime? currentValue = e.GetCurrentPropertyValue<DateTime?>("ClosedDate");

                if (previousValue.HasValue)
                {
                    // Remove Statistics
                    var statItem = _data.FirstOrDefault(i => i.Timestamp == previousValue.Value.Date);
                    if (statItem != null)
                    {
                        statItem.ClosedJobs -= 1;
                        statItem.TotalJobs += 1;
                    }
                    foreach (var affectedStat in _data.Where(i => i.Timestamp > previousValue))
                    {
                        affectedStat.TotalJobs += 1;
                    }
                }

                if (currentValue.HasValue)
                {
                    // Add Statistics
                    // Remove Statistics
                    var statItem = _data.FirstOrDefault(i => i.Timestamp == currentValue.Value.Date);
                    if (statItem != null)
                    {
                        statItem.ClosedJobs += 1;
                        statItem.TotalJobs -= 1;
                    }
                    foreach (var affectedStat in _data.Where(i => i.Timestamp > currentValue))
                    {
                        affectedStat.TotalJobs -= 1;
                    }
                }
            }
        }

        private static DailyOpenedClosedItem Data(DiscoDataContext Database, DateTime ProcessDate)
        {
            DateTime processDateStart = ProcessDate;
            DateTime processDateEnd = ProcessDate.AddDays(1);

            int totalJobs = Database.Jobs.Where(j => j.OpenedDate < processDateEnd && (!j.ClosedDate.HasValue || j.ClosedDate > processDateEnd)).Count();
            int openedJobs = Database.Jobs.Where(j => j.OpenedDate > processDateStart && j.OpenedDate < processDateEnd).Count();
            int closedJobs = Database.Jobs.Where(j => j.ClosedDate > processDateStart && j.ClosedDate < processDateEnd).Count();

            return new DailyOpenedClosedItem()
            {
                Timestamp = ProcessDate,
                TotalJobs = totalJobs,
                OpenedJobs = openedJobs,
                ClosedJobs = closedJobs
            };
        }

        public static List<DailyOpenedClosedItem> Data(DiscoDataContext Database, bool FilterUnimportantWeekends = false)
        {
            List<DailyOpenedClosedItem> resultData;

            UpdateDataHistory(Database);

            if (FilterUnimportantWeekends)
                resultData = _data.Where(i => (i.Timestamp.DayOfWeek != DayOfWeek.Saturday && i.Timestamp.DayOfWeek != DayOfWeek.Sunday) ||
                    (i.OpenedJobs > 0 || i.ClosedJobs > 0)).ToList();
            else
                resultData = _data.ToList();

            return resultData;
        }
    }
}
