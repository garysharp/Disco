using Disco.Data.Repository;
using Disco.Services.Tasks;
using Quartz;
using System;

namespace Disco.Services.Interop.DiscoServices
{
    public class OnlineServicesConnectStartTask : ScheduledTask
    {
        public override string TaskName => "Online Services Connect - Start";
        public override bool LogExceptionsOnly => true;

        public override void InitalizeScheduledTask(DiscoDataContext Database)
        {
            // Trigger in 1 + 0-14 minutes
            var rng = new Random();
            var delay = rng.Next(15) + 1;

            TriggerBuilder triggerBuilder = TriggerBuilder.Create()
                .StartAt(DateTimeOffset.Now.AddMinutes(delay))
                .WithSchedule(SimpleScheduleBuilder.RepeatHourlyForever());

            ScheduleTask(triggerBuilder);
        }

        protected override void ExecuteTask()
        {
            using (var database = new DiscoDataContext())
            {
                if (database.DiscoConfiguration.IsActivated)
                {
                    if (OnlineServicesConnect.State == OnlineServicesConnect.ConnectionState.Disconnected)
                        OnlineServicesConnect.QueueStart();
                }
            }
        }

    }
}
