using Disco.Data.Repository;
using Disco.Models.Services.Devices.Importing;
using Disco.Services.Tasks;
using Quartz;
using System;

namespace Disco.Services.Devices.Importing
{
    public class DeviceImportParseTask : ScheduledTask
    {
        private const string JobDataMapContext = "Context";

        public override string TaskName { get { return "Import Devices - Parsing Records"; } }
        public override bool SingleInstanceTask { get { return false; } }
        public override bool CancelInitiallySupported { get { return false; } }

        public static ScheduledTaskStatus ScheduleNow(IDeviceImportContext Context)
        {
            if (Context == null)
                throw new ArgumentNullException("Context");

            // Build Data Map
            var task = new DeviceImportParseTask();
            JobDataMap taskData = new JobDataMap() { { JobDataMapContext, Context } };

            // Schedule Task
            return task.ScheduleTask(taskData);
        }

        protected override void ExecuteTask()
        {
            var context = (IDeviceImportContext)ExecutionContext.JobDetail.JobDataMap[JobDataMapContext];

            using (DiscoDataContext Database = new DiscoDataContext())
            {
                context.ParseRecords(Database, Status);
            }
        }
    }
}
