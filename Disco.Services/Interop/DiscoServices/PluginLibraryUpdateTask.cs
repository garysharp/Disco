using Disco.Data.Repository;
using Disco.Services.Tasks;
using Quartz;
using System;
using System.Linq;

namespace Disco.Services.Interop.DiscoServices
{
    public class PluginLibraryUpdateTask : ScheduledTask
    {
        public override string TaskName { get { return "Disco ICT - Update Plugin Library"; } }
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
                Status.UpdateStatus(1, "Updating Plugin Library Manifest", "Initializing");

                var manifest = PluginLibrary.UpdateManifest(database, Status);

                Status.SetFinishedMessage("The Plugin Library Manifest was updated successfully");
            }
        }

        public static ScheduledTaskStatus ScheduleNow()
        {
            var taskStatus = RunningStatus;
            if (taskStatus != null)
                return taskStatus;
            else
            {
                var task = new PluginLibraryUpdateTask();
                return task.ScheduleTask();
            }
        }

        public static ScheduledTaskStatus RunningStatus
        {
            get
            {
                return ScheduledTasks.GetTaskStatuses(typeof(PluginLibraryUpdateTask)).Where(ts => ts.IsRunning).FirstOrDefault();
            }
        }
    }
}
