using Disco.Data.Repository;
using Disco.Services.Tasks;
using Quartz;

namespace Disco.Services.Users.UserFlags
{
    public class UserFlagDeleteTask : ScheduledTask
    {
        public override string TaskName { get { return "User Flags - Delete Flag"; } }

        public override bool SingleInstanceTask { get { return false; }}
        public override bool CancelInitiallySupported { get { return false; } }
        public override bool LogExceptionsOnly { get { return true; } }

        protected override void ExecuteTask()
        {
            int UserFlagId = (int)this.ExecutionContext.JobDetail.JobDataMap["UserFlagId"];

            using (DiscoDataContext Database = new DiscoDataContext())
            {
                UserFlagService.DeleteUserFlag(Database, UserFlagId, this.Status);
            }
        }

        public static ScheduledTaskStatus ScheduleNow(int UserFlagId)
        {
            JobDataMap taskData = new JobDataMap() { { "UserFlagId", UserFlagId } };

            var instance = new UserFlagDeleteTask();

            return instance.ScheduleTask(taskData);
        }
    }
}