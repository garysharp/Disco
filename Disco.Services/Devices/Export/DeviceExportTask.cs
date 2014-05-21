using Disco.Models.Repository;
using Disco.Models.Services.Devices.Exporting;
using Disco.Services.Tasks;
using Quartz;
using System;
using System.IO;
using System.Linq;
using Disco.Data.Repository;

namespace Disco.Services.Devices.Export
{
    public class DeviceExportTask : ScheduledTask
    {
        private const string JobDataMapContext = "Context";

        public override string TaskName { get { return "Export Devices"; } }
        public override bool SingleInstanceTask { get { return false; } }
        public override bool CancelInitiallySupported { get { return false; } }

        public static DeviceExportTaskContext ScheduleNow(DeviceExportOptions Options)
        {
            // Build Context
            var context = new DeviceExportTaskContext(Options);

            // Build Data Map
            var task = new DeviceExportTask();
            JobDataMap taskData = new JobDataMap() { { JobDataMapContext, context} };
            
            // Schedule Task
            context.TaskStatus = task.ScheduleTask(taskData);

            return context;
        }

        protected override void ExecuteTask()
        {
            var context = (DeviceExportTaskContext)this.ExecutionContext.JobDetail.JobDataMap[JobDataMapContext];

            Status.UpdateStatus(10, "Exporting Device Records", "Starting...");

            using (DiscoDataContext Database = new DiscoDataContext())
            {
                context.CsvResult = DeviceExport.GenerateExport(Database, context.Options, this.Status);
            }
        }
    }
}
