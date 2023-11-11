using Disco.Models.Services.Devices.Exporting;
using Disco.Services.Tasks;
using Quartz;
using Disco.Data.Repository;
using Disco.Services.Exporting;

namespace Disco.Services.Devices.Exporting
{
    public class DeviceExportTask : ScheduledTask
    {
        private const string JobDataMapContext = "Context";

        public override string TaskName { get { return "Export Devices"; } }
        public override bool SingleInstanceTask { get { return false; } }
        public override bool CancelInitiallySupported { get { return false; } }

        public static ExportTaskContext<DeviceExportOptions> ScheduleNow(DeviceExportOptions Options)
        {
            // Build Context
            var context = new ExportTaskContext<DeviceExportOptions>(Options);

            // Build Data Map
            var task = new DeviceExportTask();
            JobDataMap taskData = new JobDataMap() { { JobDataMapContext, context} };
            
            // Schedule Task
            context.TaskStatus = task.ScheduleTask(taskData);

            return context;
        }

        protected override void ExecuteTask()
        {
            var context = (ExportTaskContext<DeviceExportOptions>)ExecutionContext.JobDetail.JobDataMap[JobDataMapContext];

            Status.UpdateStatus(10, "Exporting Device Records", "Starting...");

            using (DiscoDataContext Database = new DiscoDataContext())
            {
                context.Result = DeviceExport.GenerateExport(Database, context.Options, Status);
            }
        }
    }
}
