using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Disco.Data.Repository;
using Disco.Services.Logging;
using Disco.Services.Tasks;
using Quartz;

namespace Disco.BI.Interop.Community
{
    public class UpdateCheckTask : ScheduledTask
    {
        public override string TaskName { get { return "Disco Community - Check for Update"; } }
        public override bool SingleInstanceTask { get { return true; } }
        public override bool CancelInitiallySupported { get { return false; } }

        public static ScheduledTaskStatus ScheduleNow()
        {

            var runningTasks = ScheduledTasks.GetTaskStatuses(typeof(UpdateCheckTask)).Where(ts => ts.IsRunning).ToList();
            if (runningTasks.Count > 0)
                return runningTasks.First();
            else
            {
                var t = new UpdateCheckTask();
                return t.ScheduleTask();
            }
        }
        public static ScheduledTaskStatus RunningStatus
        {
            get
            {
                return ScheduledTasks.GetTaskStatuses(typeof(UpdateCheckTask)).Where(ts => ts.IsRunning).FirstOrDefault();
            }
        }
        public static DateTime? NextScheduled
        {
            get
            {
                var runningTasks = ScheduledTasks.GetTaskStatuses(typeof(UpdateCheckTask)).ToList();
                DateTime timestamp = DateTime.MaxValue;
                foreach (var t in runningTasks)
                {
                    if (t.NextScheduledTimestamp != null && t.NextScheduledTimestamp.Value < timestamp)
                        timestamp = t.NextScheduledTimestamp.Value;
                }
                if (timestamp == DateTime.MaxValue)
                    return null;
                else
                    return timestamp;
            }
        }

        public override void InitalizeScheduledTask(Data.Repository.DiscoDataContext dbContext)
        {
            // Random time between midday and midnight.
            var rnd = new Random();

            var rndHour = rnd.Next(12, 23);
            var rndMinute = rnd.Next(0, 59);

            TriggerBuilder triggerBuilder = TriggerBuilder.Create().
                WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(rndHour, rndMinute));

            this.ScheduleTask(triggerBuilder);
        }

        protected override void ExecuteTask()
        {
            using (DiscoDataContext db = new DiscoDataContext())
            {
                try
                {
                    UpdateCheck.Check(db, true, this.Status);
                }
                catch (Exception ex)
                {
                    ScheduledTasksLog.LogScheduledTaskException(this.Status.TaskName, this.Status.SessionId, this.Status.TaskType, ex);
                    // Could be proxy error - try again without proxy:
                    UpdateCheck.Check(db, false, this.Status);
                }
            }
        }
    }
}
