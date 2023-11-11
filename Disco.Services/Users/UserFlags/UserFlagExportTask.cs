using Disco.Data.Repository;
using Disco.Models.Services.Users.UserFlags;
using Disco.Services.Exporting;
using Disco.Services.Tasks;
using Quartz;

namespace Disco.Services.Users.UserFlags
{
    public class UserFlagExportTask : ScheduledTask
    {
        private const string JobDataMapContext = "Context";

        public override string TaskName { get; } = "Export User Flags";
        public override bool SingleInstanceTask { get { return false; } }
        public override bool CancelInitiallySupported { get { return false; } }

        public static ExportTaskContext<UserFlagExportOptions> ScheduleNow(UserFlagExportOptions options)
        {
            // Build Context
            var context = new ExportTaskContext<UserFlagExportOptions>(options);

            // Build Data Map
            var task = new UserFlagExportTask();
            JobDataMap taskData = new JobDataMap() { { JobDataMapContext, context } };

            // Schedule Task
            context.TaskStatus = task.ScheduleTask(taskData);

            return context;
        }

        protected override void ExecuteTask()
        {
            var context = (ExportTaskContext<UserFlagExportOptions>)ExecutionContext.JobDetail.JobDataMap[JobDataMapContext];

            Status.UpdateStatus(10, "Exporting User Flag Records", "Starting...");

            using (DiscoDataContext Database = new DiscoDataContext())
            {
                var export = new UserFlagExport(Database, context.Options);

                context.Result = export.Generate(Status);
            }
        }
    }
}
