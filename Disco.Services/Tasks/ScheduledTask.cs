using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quartz;
using Disco.Data.Repository;

namespace Disco.Services.Tasks
{
    public abstract class ScheduledTask : IJob
    {
        public virtual void InitalizeScheduledTask(DiscoDataContext Database) { return; }

        internal protected ScheduledTaskStatus Status { get; private set; }
        internal protected IJobExecutionContext ExecutionContext { get; private set; }

        public virtual bool CancelInitiallySupported { get { return true; } }
        public virtual bool SingleInstanceTask { get { return true; } }
        public virtual bool IsSilent { get { return false; } }
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
            this.ExecutionContext = context;
            this.Status = context.GetDiscoScheduledTaskStatus();
            if (this.Status == null)
                this.Status = ScheduledTasks.RegisterTask(this);

            try
            {
                if (!this.LogExceptionsOnly)
                    ScheduledTasksLog.LogScheduledTaskExecuted(this.Status.TaskName, this.Status.SessionId);

                this.Status.Started();
                this.ExecuteTask();
            }
            catch (Exception ex)
            {
                ScheduledTasksLog.LogScheduledTaskException(this.Status.TaskName, this.Status.SessionId, this.GetType(), ex);
                this.Status.SetTaskException(ex);
            }
            finally
            {
                if (!this.Status.FinishedTimestamp.HasValue) // Scheduled Task Didn't Trigger 'Finished'
                    this.Status.Finished();

                var nextTriggerTime = context.NextFireTimeUtc;
                if (nextTriggerTime.HasValue)
                { // Continuous Task
                    this.Status.Reset(nextTriggerTime.Value.LocalDateTime);
                }
                else
                {
                    this.UnregisterTask();
                }

                if (!this.LogExceptionsOnly)
                    ScheduledTasksLog.LogScheduledTaskFinished(this.Status.TaskName, this.Status.SessionId);
            }
        }
    }
}
