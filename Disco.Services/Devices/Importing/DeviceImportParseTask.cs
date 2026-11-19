using Disco.Services.Tasks;
using Quartz;
using System;

namespace Disco.Services.Devices.Importing
{
    public class DeviceImportParseTask : ScheduledTask
    {
        public override string TaskName { get { return "Import Devices - Parsing Records"; } }
        public override bool SingleInstanceTask { get { return false; } }
        public override bool CancelInitiallySupported { get { return false; } }

        public static ScheduledTaskStatus ScheduleNow(IDeviceImportContextActions context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            // Build Data Map
            var task = new DeviceImportParseTask();
            JobDataMap taskData = new JobDataMap() { { nameof(IDeviceImportContextActions), context } };

            // Schedule Task
            return task.ScheduleTask(taskData);
        }

        protected override void ExecuteTask()
        {
            var context = (IDeviceImportContextActions)ExecutionContext.JobDetail.JobDataMap[nameof(IDeviceImportContextActions)];

            context.Parse(Status);
        }
    }
}
