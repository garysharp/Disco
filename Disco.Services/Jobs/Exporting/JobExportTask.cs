using Disco.Data.Repository;
using Disco.Models.Services.Jobs.Exporting;
using Disco.Services.Exporting;
using Disco.Services.Tasks;
using Quartz;

namespace Disco.Services.Jobs.Exporting
{
    public class JobExportTask : ScheduledTask
    {
        private const string JobDataMapContext = "Context";

        public override string TaskName { get { return "Export Jobs"; } }
        public override bool SingleInstanceTask { get { return false; } }
        public override bool CancelInitiallySupported { get { return false; } }

        public static ExportTaskContext<JobExportOptions> ScheduleNow(JobExportOptions Options)
        {
            // Build Context
            var context = new ExportTaskContext<JobExportOptions>(Options);

            // Build Data Map
            var task = new JobExportTask();
            JobDataMap taskData = new JobDataMap() { { JobDataMapContext, context} };
            
            // Schedule Task
            context.TaskStatus = task.ScheduleTask(taskData);

            return context;
        }

        protected override void ExecuteTask()
        {
            var context = (ExportTaskContext<JobExportOptions>)ExecutionContext.JobDetail.JobDataMap[JobDataMapContext];

            Status.UpdateStatus(10, "Exporting Job Records", "Starting...");

            using (DiscoDataContext Database = new DiscoDataContext())
            {
                context.Result = JobExport.GenerateExport(Database, context.Options, Status);
            }
        }
    }
}
