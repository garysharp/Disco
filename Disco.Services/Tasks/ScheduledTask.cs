using Disco.Data.Repository;
using Quartz;
using System;

namespace Disco.Services.Tasks
{
    public abstract class ScheduledTask : IJob
    {
        public virtual void InitalizeScheduledTask(DiscoDataContext Database) { return; }

        internal protected ScheduledTaskStatus Status { get; private set; }
        internal protected IJobExecutionContext ExecutionContext { get; private set; }

        public virtual bool CancelInitiallySupported { get { return true; } }
        public virtual bool SingleInstanceTask { get { return true; } }
        public virtual bool LogExceptionsOnly { get { return false; } }

        public abstract string TaskName { get; }
        protected abstract void ExecuteTask();

        #region Protected Triggers
        /// <summary>
        /// Schedules the Task to Begin Immediately
        /// </summary>
        protected ScheduledTaskStatus ScheduleTask()
        {
            return ScheduleTask(null, null);
        }
        /// <summary>
        /// Schedules the Task to Begin Immediately
        /// </summary>
        /// <param name="DataMap">DataMap passed into the executing Task</param>
        /// <returns></returns>
        protected ScheduledTaskStatus ScheduleTask(JobDataMap DataMap)
        {
            return ScheduleTask(null, DataMap);
        }
        /// <summary>
        /// Schedules the Task to Begin based on the Trigger
        /// </summary>
        /// <param name="Trigger">Trigger for the Task</param>
        protected ScheduledTaskStatus ScheduleTask(TriggerBuilder Trigger)
        {
            return ScheduleTask(Trigger, null);
        }
        /// <summary>
        /// Schedules the Task to Begin based on the Trigger including the DataMap
        /// </summary>
        /// <param name="Trigger">Trigger for the Task</param>
        /// <param name="DataMap">DataMap passed into the executing Task</param>
        /// <returns></returns>
        protected ScheduledTaskStatus ScheduleTask(TriggerBuilder Trigger, JobDataMap DataMap)
        {
            if (Trigger == null)
                Trigger = TriggerBuilder.Create(); // Defaults to Start Immediately

            if (DataMap != null)
                Trigger = Trigger.UsingJobData(DataMap);

            return ScheduledTasks.RegisterTask(this, Trigger);
        }
        #endregion

        public void Execute(IJobExecutionContext context)
        {
            // Task Status
            ExecutionContext = context;
            Status = context.GetDiscoScheduledTaskStatus();
            if (Status == null)
                Status = ScheduledTasks.RegisterTask(this);

            try
            {
                if (!LogExceptionsOnly)
                    ScheduledTasksLog.LogScheduledTaskExecuted(Status.TaskName, Status.SessionId);

                Status.Started();
                ExecuteTask();
            }
            catch (Exception ex)
            {
                ScheduledTasksLog.LogScheduledTaskException(Status.TaskName, Status.SessionId, GetType(), ex);
                Status.SetTaskException(ex);
            }
            finally
            {
                if (!Status.FinishedTimestamp.HasValue) // Scheduled Task Didn't Trigger 'Finished'
                    Status.Finished();

                Status.Finally();

                var nextTriggerTime = context.NextFireTimeUtc;
                if (nextTriggerTime.HasValue)
                { // Continuous Task
                    Status.Reset(nextTriggerTime.Value.LocalDateTime);
                }
                else
                {
                    this.UnregisterTask();
                }

                if (!LogExceptionsOnly)
                    ScheduledTasksLog.LogScheduledTaskFinished(Status.TaskName, Status.SessionId);
            }
        }
    }
}
