using Disco.Data.Repository;
using Disco.Services.Tasks;
using Quartz;
using System;
using System.Linq;

namespace Disco.Services.Interop.DiscoServices
{
    public class UpdateQueryTask : ScheduledTask
    {
        public override string TaskName { get { return "Disco ICT - Check for Update"; } }
        public override bool SingleInstanceTask { get { return true; } }
        public override bool CancelInitiallySupported { get { return false; } }

        public override void InitalizeScheduledTask(DiscoDataContext Database)
        {
            // Random time between midday and midnight.
            var rnd = new Random();

            var rndHour = rnd.Next(12, 23);
            var rndMinute = rnd.Next(0, 59);

            TriggerBuilder triggerBuilder = TriggerBuilder.Create().
                WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(rndHour, rndMinute));

            ScheduleTask(triggerBuilder);
        }

        protected override void ExecuteTask()
        {
            using (DiscoDataContext database = new DiscoDataContext())
            {
                try
                {
                    UpdateQuery.Check(database, true, Status);
                }
                catch (Exception ex)
                {
                    ScheduledTasksLog.LogScheduledTaskException(Status.TaskName, Status.SessionId, Status.TaskType, ex);

                    // Could be proxy error - try again without proxy:
                    UpdateQuery.Check(database, false, Status);
                }
            }
        }

        public static ScheduledTaskStatus ScheduleNow()
        {
            var taskStatus = RunningStatus;
            if (taskStatus != null)
                return taskStatus;
            else
            {
                var task = new UpdateQueryTask();
                return task.ScheduleTask();
            }
        }

        public static DateTime? NextScheduled
        {
            get
            {
                DateTime timestamp = DateTime.MaxValue;
                var tasks = ScheduledTasks.GetTaskStatuses(typeof(UpdateQueryTask)).ToList();

                foreach (var t in tasks)
                    if (t.NextScheduledTimestamp != null && t.NextScheduledTimestamp.Value < timestamp)
                        timestamp = t.NextScheduledTimestamp.Value;

                if (timestamp != DateTime.MaxValue)
                    return timestamp;
                else
                    return null;
            }
        }

        public static ScheduledTaskStatus RunningStatus
        {
            get
            {
                return ScheduledTasks.GetTaskStatuses(typeof(UpdateQueryTask)).Where(ts => ts.IsRunning).FirstOrDefault();
            }
        }

    }
}