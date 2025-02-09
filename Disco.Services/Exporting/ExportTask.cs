using Disco.Data.Repository;
using Disco.Services.Tasks;
using Quartz;
using System;
using System.Web;
using System.Web.Caching;

namespace Disco.Services.Exporting
{
    public class ExportTask : ScheduledTask
    {
        private IExport context;
        public override string TaskName { get => context?.Name ?? "Exporting"; }
        public override bool SingleInstanceTask { get; } = false;
        public override bool CancelInitiallySupported { get; } = false;

        public static ExportTaskContext ScheduleNow(IExport export)
        {
            // Build Context
            var taskContext = new ExportTaskContext(export);

            // Build Data Map
            var task = new ExportTask();
            JobDataMap taskData = new JobDataMap() { { nameof(ExportTask), taskContext } };

            // Schedule Task
            taskContext.TaskStatus = task.ScheduleTask(taskData);

            return taskContext;
        }

        private static string GetCacheKey(Guid exportId) => $"ExportTask_{exportId}";

        public static ExportTaskContext ScheduleNowCacheResult(IExport exportContext, Func<Guid, string> returnUrlBuilder)
        {
            var taskContext = ScheduleNow(exportContext);

            var key = GetCacheKey(taskContext.Id);
            HttpRuntime.Cache.Insert(key, taskContext, null, DateTime.Now.AddMinutes(60), Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable, null);

            taskContext.TaskStatus.SetFinishedUrl(returnUrlBuilder(taskContext.Id));

            return taskContext;
        }

        public static bool TryFromCache(Guid? exportId, out ExportTaskContext exportContext)
        {
            if (exportId != null)
            {
                var key = GetCacheKey(exportId.Value);

                if (HttpRuntime.Cache.Get(key) is ExportTaskContext context)
                {
                    exportContext = context;
                    return true;
                }
            }

            exportContext = null;
            return false;
        }

        protected override void ExecuteTask()
        {
            var context = (ExportTaskContext)ExecutionContext.JobDetail.JobDataMap[nameof(ExportTask)];
            this.context = context.ExportContext;

            Status.UpdateStatus(0, "Exporting", "Starting...");

            using (var database = new DiscoDataContext())
            {
                context.Result = context.ExportContext.Export(database, Status);
            }
        }
    }
}
