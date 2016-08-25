using Disco.Data.Repository;
using Disco.Services.Tasks;
using Quartz;
using System;
using System.Linq;

namespace Disco.Services.Expressions
{
    public class ExpressionCachePreloadTask : ScheduledTask
    {

        public override string TaskName { get { return "Expression Cache - Preload Task"; } }
        public override bool SingleInstanceTask { get { return true; } }
        public override bool CancelInitiallySupported { get { return false; } }
        public override bool LogExceptionsOnly { get { return true; } }

        public override void InitalizeScheduledTask(DiscoDataContext Database)
        {
            // Run in Background 5 Second after Scheduled (on App Startup)
            TriggerBuilder triggerBuilder = TriggerBuilder.Create().StartAt(new DateTimeOffset(DateTime.Now).AddSeconds(5));

            this.ScheduleTask(triggerBuilder);
        }

        protected override void ExecuteTask()
        {
            // Cache Document Template Filter Expressions
            using (DiscoDataContext database = new DiscoDataContext())
            {
                foreach (var documentTemplate in database.DocumentTemplates.Where(dt => dt.FilterExpression != null && dt.FilterExpression != string.Empty))
                {
                    if (!string.IsNullOrWhiteSpace(documentTemplate.FilterExpression))
                        documentTemplate.FilterExpressionFromCache();
                }
            }
        }
    }
}
