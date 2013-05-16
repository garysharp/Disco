using Disco.Data.Repository;
using Disco.Services.Tasks;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.BI.UserBI
{
    public class UserCachePruneTask : ScheduledTask
    {
        public override string TaskName { get { return "User Cache - Clean Stale Cache"; } }

        public override bool SingleInstanceTask { get { return true; } }
        public override bool CancelInitiallySupported { get { return false; } }
        public override bool LogExceptionsOnly { get { return true; } }

        public override void InitalizeScheduledTask(DiscoDataContext dbContext)
        {
            // Run @ every 15mins

            // Next 15min interval
            DateTime now = DateTime.Now;
            int mins = (15 - (now.Minute % 15));
            if (mins < 10)
                mins += 15;
            DateTimeOffset startAt = new DateTimeOffset(now).AddMinutes(mins).AddSeconds(now.Second * -1).AddMilliseconds(now.Millisecond * -1);

            TriggerBuilder triggerBuilder = TriggerBuilder.Create().StartAt(startAt).
                WithSchedule(SimpleScheduleBuilder.RepeatMinutelyForever(15));

            this.ScheduleTask(triggerBuilder);
        }

        protected override void ExecuteTask()
        {
            UserCache.CleanStaleCache();
        }
    }
}
