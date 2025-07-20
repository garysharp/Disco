﻿using Disco.Data.Repository;
using Disco.Services.Tasks;
using Quartz;

namespace Disco.Services.Jobs.JobQueues
{
    public class JobQueueDeleteTask : ScheduledTask
    {
        public override string TaskName { get { return "Job Queues - Delete Queue"; } }

        public override bool SingleInstanceTask { get { return false; } }
        public override bool CancelInitiallySupported { get { return false; } }
        public override bool LogExceptionsOnly { get { return true; } }

        protected override void ExecuteTask()
        {
            int jobQueueId = (int)ExecutionContext.JobDetail.JobDataMap["JobQueueId"];

            using (DiscoDataContext Database = new DiscoDataContext())
            {
                JobQueueService.DeleteJobQueue(Database, jobQueueId, Status);
            }
        }

        public static ScheduledTaskStatus ScheduleNow(int JobQueueId)
        {
            JobDataMap taskData = new JobDataMap() { { "JobQueueId", JobQueueId } };

            var instance = new JobQueueDeleteTask();

            return instance.ScheduleTask(taskData);
        }
    }
}
