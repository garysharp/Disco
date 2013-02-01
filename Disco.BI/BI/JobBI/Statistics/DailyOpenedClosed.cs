using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quartz;
using Disco.Models.BI.Job.Statistics;
using Disco.Data.Repository;
using Quartz.Impl;
using Disco.Services.Tasks;

namespace Disco.BI.JobBI.Statistics
{
    public class DailyOpenedClosed : ScheduledTask
    {

        private static List<DailyOpenedClosedItem> _data;
        private static object _dataLock = new object();


        public override string TaskName { get { return "Job Statistics - Daily Opened/Closed Task"; } }
        public override bool SingleInstanceTask { get { return true; } }
        public override bool CancelInitiallySupported { get { return false; } }

        public override void InitalizeScheduledTask(DiscoDataContext dbContext)
        {
            // Trigger Daily @ 12:29am
            TriggerBuilder triggerBuilder = TriggerBuilder.Create().WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(0, 29));

            this.ScheduleTask(triggerBuilder);
        }
        protected override void ExecuteTask()
        {
            using (var dbContext = new DiscoDataContext())
            {
                UpdateDataHistory(dbContext, true);
            }
        }

        //public void InitalizeScheduledTask(DiscoDataContext dbContext, IScheduler Scheduler)
        //{
        //    // Run @ 12:29am
        //    IJobDetail jobDetail = new JobDetailImpl("JobStatisticsDailyOpenedClosed", typeof(DailyOpenedClosed));
        //    ITrigger trigger = TriggerBuilder.Create().
        //        WithIdentity("JobStatisticsDailyOpenedClosedTrigger").
        //        StartNow().
        //        WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(0, 29)).
        //        Build();
        //    Scheduler.ScheduleJob(jobDetail, trigger);
        //}

        //public void Execute(IJobExecutionContext context)
        //{
        //    try
        //    {
        //        using (var dbContext = new DiscoDataContext())
        //        {
        //            UpdateDataHistory(dbContext, true);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Logging.SystemLog.LogException("Disco.BI.JobBI.Statistics.DailyOpenedClosed", ex);
        //    }
        //}

        private static void UpdateDataHistory(DiscoDataContext dbContext, bool Refresh = false)
        {
            DateTime historyEnd = DateTime.Now.AddDays(-1).Date;

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
                            resultData.Add(Data(dbContext, processDate));
                            processDate = processDate.AddDays(1);
                        }

                        _data = resultData;
                    }
                }
            }
        }

        private static DailyOpenedClosedItem Data(DiscoDataContext dbContext, DateTime ProcessDate)
        {
            DateTime processDateStart = ProcessDate;
            DateTime processDateEnd = ProcessDate.AddDays(1);

            int totalJobs = dbContext.Jobs.Where(j => j.OpenedDate < processDateEnd && (!j.ClosedDate.HasValue || j.ClosedDate > processDateEnd)).Count();
            int openedJobs = dbContext.Jobs.Where(j => j.OpenedDate > processDateStart && j.OpenedDate < processDateEnd).Count();
            int closedJobs = dbContext.Jobs.Where(j => j.ClosedDate > processDateStart && j.ClosedDate < processDateEnd).Count();

            return new DailyOpenedClosedItem()
            {
                Timestamp = ProcessDate,
                TotalJobs = totalJobs,
                OpenedJobs = openedJobs,
                ClosedJobs = closedJobs
            };
        }

        public static List<DailyOpenedClosedItem> Data(DiscoDataContext dbContext, bool FilterUnimportantWeekends = false)
        {
            List<DailyOpenedClosedItem> resultData;

            UpdateDataHistory(dbContext);

            if (FilterUnimportantWeekends)
                resultData = _data.Where(i => (i.Timestamp.DayOfWeek != DayOfWeek.Saturday && i.Timestamp.DayOfWeek != DayOfWeek.Sunday) ||
                    (i.OpenedJobs > 0 || i.ClosedJobs > 0)).ToList();
            else
                resultData = _data.ToList();

            resultData.Add(Data(dbContext, DateTime.Today));

            return resultData;
        }
    }
}
