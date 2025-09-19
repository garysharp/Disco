using Disco.Data.Repository;
using Disco.Services.Tasks;
using Quartz;
using System;
using System.Data.Entity;
using System.Linq;

namespace Disco.Services.Devices.DeviceFlags
{
    public class FlagRemovalTask : ScheduledTask
    {
        public override string TaskName { get; } = "Flags - Scheduled Removal";

        public override bool SingleInstanceTask { get; } = false;
        public override bool CancelInitiallySupported { get; } = false;
        public override bool LogExceptionsOnly { get; } = true;

        public override void InitalizeScheduledTask(DiscoDataContext Database)
        {
            // Schedule in 1mins
            var trigger = TriggerBuilder.Create()
                .StartAt(DateTimeOffset.Now.AddMinutes(1));

            ScheduleTask(trigger);

            // Schedule every day at midnight
            trigger = TriggerBuilder.Create()
                .WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(0, 0));
            ScheduleTask(trigger);
        }

        protected override void ExecuteTask()
        {
            using (DiscoDataContext database = new DiscoDataContext())
            {
                var assignments = database.DeviceFlagAssignments
                    .Include(a => a.RemoveUser)
                    .Include(a => a.AddedUser)
                    .Where(a => a.RemovedDate == null && a.RemoveDate <= DateTime.Today)
                    .ToList();

                foreach (var assignment in assignments)
                {
                    assignment.OnRemoveUnsafe(database, assignment.RemoveUser ?? assignment.AddedUser, isScheduled: true);
                }
                database.SaveChanges();
            }

            using (DiscoDataContext database = new DiscoDataContext())
            {
                var assignments = database.UserFlagAssignments
                    .Include(a => a.RemoveUser)
                    .Include(a => a.AddedUser)
                    .Where(a => a.RemovedDate == null && a.RemoveDate <= DateTime.Today)
                    .ToList();

                foreach (var assignment in assignments)
                {
                    assignment.OnRemoveUnsafe(database, assignment.RemoveUser ?? assignment.AddedUser, isScheduled: true);
                }
                database.SaveChanges();
            }
        }
    }
}
