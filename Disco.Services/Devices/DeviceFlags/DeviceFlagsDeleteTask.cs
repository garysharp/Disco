using Disco.Data.Repository;
using Disco.Services.Tasks;
using Quartz;

namespace Disco.Services.Devices.DeviceFlags
{
    public class DeviceFlagDeleteTask : ScheduledTask
    {
        public override string TaskName { get { return "Device Flags - Delete Flag"; } }

        public override bool SingleInstanceTask { get { return false; }}
        public override bool CancelInitiallySupported { get { return false; } }
        public override bool LogExceptionsOnly { get { return true; } }

        protected override void ExecuteTask()
        {
            int deviceFlagId = (int)ExecutionContext.JobDetail.JobDataMap["DeviceFlagId"];

            using (DiscoDataContext Database = new DiscoDataContext())
            {
                DeviceFlagService.DeleteDeviceFlag(Database, deviceFlagId, Status);
            }
        }

        public static ScheduledTaskStatus ScheduleNow(int deviceFlagId)
        {
            var taskData = new JobDataMap() { { "DeviceFlagId", deviceFlagId } };

            var instance = new DeviceFlagDeleteTask();

            return instance.ScheduleTask(taskData);
        }
    }
}