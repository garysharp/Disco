using Disco.Data.Repository;
using Disco.Services.Tasks;
using Quartz;
using System;

namespace Disco.Services.Exporting
{
    public class SavedExportTask : ScheduledTask
    {
        public override string TaskName { get; } = "Saved Export Scheduler";
        public override bool SingleInstanceTask { get; } = true;
        public override bool CancelInitiallySupported { get; } = false;
        public override bool LogExceptionsOnly { get; } = true;

        public override void InitalizeScheduledTask(DiscoDataContext Database)
        {

            // run in 30 seconds, then every hour on the hour
            if (DateTime.Now.Minute != 59)
            {
                var immediateTrigger = TriggerBuilder.Create().StartAt(DateTimeOffset.Now.AddSeconds(30));
                ScheduleTask(immediateTrigger);
            }

            var nextHourTicks = DateTime.UtcNow.Ticks;
            nextHourTicks -= nextHourTicks % TimeSpan.TicksPerHour; // round down to the hour
            var nextHour = new DateTime(nextHourTicks, DateTimeKind.Utc)
                .AddHours(1)
                .AddSeconds(1);
            var hourlyTrigger = TriggerBuilder.Create()
                .StartAt(nextHour)
                .WithSchedule(SimpleScheduleBuilder.RepeatHourlyForever());
            ScheduleTask(hourlyTrigger);
        }

        protected override void ExecuteTask()
        {
            SavedExports.EvaluateSavedExports();
        }

    }
}
