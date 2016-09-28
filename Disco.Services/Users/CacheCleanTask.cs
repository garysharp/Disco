using Disco.Data.Repository;
using Disco.Services.Tasks;
using Quartz;
using System;

namespace Disco.Services.Users
{
    public class CacheCleanTask : ScheduledTask
    {
        public override string TaskName { get { return "User Cache - Clean Stale Cache"; } }

        public override bool SingleInstanceTask { get { return true; } }
        public override bool CancelInitiallySupported { get { return false; } }
        public override bool LogExceptionsOnly { get { return true; } }

        public override void InitalizeScheduledTask(DiscoDataContext Database)
        {
            // Run @ every 15mins

            // Next 15min interval
            var now = DateTime.Now;
            var mins = (15 - (now.Minute % 15));
            if (mins < 10)
                mins += 15;
            var startAt = new DateTimeOffset(now).AddMinutes(mins).AddSeconds(now.Second * -1).AddMilliseconds(now.Millisecond * -1);

            var triggerBuilder = TriggerBuilder.Create().StartAt(startAt).
                WithSchedule(SimpleScheduleBuilder.RepeatMinutelyForever(15));

            ScheduleTask(triggerBuilder);
        }

        protected override void ExecuteTask()
        {
            Cache.CleanStaleCache();
        }
    }
}
