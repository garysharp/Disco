using Disco.Data.Repository;
using Disco.Services.Tasks;
using Quartz;
using System;

namespace Disco.Services.Interop.DiscoServices
{
    public class ActivationCleanupTask : ScheduledTask
    {
        public override string TaskName { get { return "Activation Cleanup"; } }
        public override bool LogExceptionsOnly => true;

        public override void InitalizeScheduledTask(DiscoDataContext Database)
        {
            var service = new ActivationService(Database);

            if (service.RequiresCleanup)
            {
                // Trigger in 1 + 0-29 minutes
                var rng = new Random();
                var delay = rng.Next(30) + 1;

                TriggerBuilder triggerBuilder = TriggerBuilder.Create()
                    .StartAt(DateTimeOffset.Now.AddMinutes(delay));

                ScheduleTask(triggerBuilder);

                base.InitalizeScheduledTask(Database);
            }
        }

        protected override void ExecuteTask()
        {
            using (var database = new DiscoDataContext())
            {
                var service = new ActivationService(database);
                service.CleanupExpiredActivations();
            }
        }

    }
}
