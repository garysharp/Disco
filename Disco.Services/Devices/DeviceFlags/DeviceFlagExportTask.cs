using Disco.Data.Repository;
using Disco.Models.Services.Devices.DeviceFlag;
using Disco.Services.Exporting;
using Disco.Services.Tasks;
using Quartz;

namespace Disco.Services.Devices.DeviceFlags
{
    public class DeviceFlagExportTask : ScheduledTask
    {
        private const string JobDataMapContext = "Context";

        public override string TaskName { get; } = "Export Device Flags";
        public override bool SingleInstanceTask { get { return false; } }
        public override bool CancelInitiallySupported { get { return false; } }

        public static ExportTaskContext<DeviceFlagExportOptions> ScheduleNow(DeviceFlagExportOptions options)
        {
            // Build Context
            var context = new ExportTaskContext<DeviceFlagExportOptions>(options);

            // Build Data Map
            var task = new DeviceFlagExportTask();
            JobDataMap taskData = new JobDataMap() { { JobDataMapContext, context } };

            // Schedule Task
            context.TaskStatus = task.ScheduleTask(taskData);

            return context;
        }

        protected override void ExecuteTask()
        {
            var context = (ExportTaskContext<DeviceFlagExportOptions>)ExecutionContext.JobDetail.JobDataMap[JobDataMapContext];

            Status.UpdateStatus(10, "Exporting Device Flag Records", "Starting...");

            using (DiscoDataContext Database = new DiscoDataContext())
            {
                var export = new DeviceFlagExport(Database, context.Options);

                context.Result = export.Generate(Status);
            }
        }
    }
}
