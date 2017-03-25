using System;
using System.Collections.Generic;
using System.Linq;
using Quartz;
using Quartz.Impl;
using Disco.Data.Repository;

namespace Disco.Services.Tasks
{
    public static class ScheduledTasks
    {
        internal const string SchedulerGroupName = "DiscoScheduledTasks";
        private static IScheduler _TaskScheduler;

        private static object _RunningTasksLock = new object();
        private static List<ScheduledTaskStatus> _RunningTasks = new List<ScheduledTaskStatus>();

        public static void InitalizeScheduledTasks(DiscoDataContext database, ISchedulerFactory SchedulerFactory, bool InitiallySchedule)
        {
            ScheduledTasksLog.LogInitializingScheduledTasks();

            try
            {
                _TaskScheduler = SchedulerFactory.GetScheduler();
                _TaskScheduler.Start();

                // Scheduled Cleanup
                ScheduledTaskCleanup.Schedule(_TaskScheduler);

                if (InitiallySchedule)
                {
                    // Discover DiscoScheduledTask
                    var appDomain = AppDomain.CurrentDomain;
                    var servicesAssemblyName = typeof(ScheduledTask).Assembly.GetName().Name;

                    var scheduledTaskTypes = (from a in appDomain.GetAssemblies()
                                              where !a.GlobalAssemblyCache &&
                                                !a.IsDynamic &&
                                                (a.GetName().Name == servicesAssemblyName || a.GetReferencedAssemblies().Any(ra => ra.Name == servicesAssemblyName))
                                              from type in a.GetTypes()
                                              where typeof(ScheduledTask).IsAssignableFrom(type) && !type.IsAbstract
                                              select type);
                    foreach (Type scheduledTaskType in scheduledTaskTypes)
                    {
                        ScheduledTask instance = (ScheduledTask)Activator.CreateInstance(scheduledTaskType);
                        try
                        {
                            instance.InitalizeScheduledTask(database);
                        }
                        catch (Exception ex)
                        {
                            ScheduledTasksLog.LogInitializeException(ex, scheduledTaskType);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ScheduledTasksLog.LogInitializeException(ex);
            }

        }

        public static ScheduledTaskStatus GetTaskStatus(string TaskSessionId)
        {
            return _RunningTasks.Where(t => t.SessionId == TaskSessionId).FirstOrDefault();
        }
        public static List<ScheduledTaskStatus> GetTaskStatuses(Type TaskType)
        {
            return _RunningTasks.Where(t => t.TaskType == TaskType).ToList();
        }
        public static List<ScheduledTaskStatus> GetTaskStatuses()
        {
            return _RunningTasks.ToList();
        }

        public static ScheduledTaskStatus RegisterTask(ScheduledTask Task)
        {
            return RegisterTask(Task, null);
        }
        public static ScheduledTaskStatus RegisterTask(ScheduledTask Task, TriggerBuilder TaskBuilder)
        {
            var sessionId = Guid.NewGuid().ToString("D");
            var triggerKey = GenerateTriggerKey();
            var taskType = Task.GetType();

            var taskStatus = new ScheduledTaskStatus(Task, sessionId, triggerKey.Name);

            lock (_RunningTasksLock)
            {
                if (Task.SingleInstanceTask)
                {
                    var existingGuid = _RunningTasks.Where(t => t.IsRunning && t.TaskType == taskType).Select(t => t.SessionId).FirstOrDefault();
                    if (existingGuid != null)
                        throw new InvalidOperationException(string.Format("This Single-Instance Task is already running, SessionId: {0}", existingGuid));
                }
                _RunningTasks.Add(taskStatus);
            }

            if (TaskBuilder != null)
            {
                ITrigger trigger = TaskBuilder.WithIdentity(triggerKey).Build();
                IJobDetail jobDetails = new JobDetailImpl(sessionId, taskType)
                {
                    Description = Task.TaskName,
                    JobDataMap = trigger.JobDataMap
                };

                _TaskScheduler.ScheduleJob(jobDetails, trigger);

                var nextTriggerTime = trigger.GetNextFireTimeUtc();
                if (nextTriggerTime.HasValue)
                    taskStatus.SetNextScheduledTimestamp(nextTriggerTime.Value.LocalDateTime);
            }

            return taskStatus;
        }
        public static bool UnregisterTask(this ScheduledTask Task)
        {
            lock (_RunningTasksLock)
            {
                if (_RunningTasks.Contains(Task.Status))
                {
                    //_RunningTasks.Remove(Task.Status);
                    _TaskScheduler.UnscheduleJob(Task.ExecutionContext.Trigger.Key);
                    return true;
                }
            }
            return false;
        }
        public static bool UnregisterTask(this ScheduledTaskStatus TaskStatus)
        {
            lock (_RunningTasksLock)
            {
                if (_RunningTasks.Contains(TaskStatus))
                {
                    //_RunningTasks.Remove(Task.Status);
                    _TaskScheduler.UnscheduleJob(new TriggerKey(TaskStatus.TriggerKey, SchedulerGroupName));
                    return true;
                }
            }
            return false;
        }

        public static TriggerKey GenerateTriggerKey()
        {
            return new TriggerKey(Guid.NewGuid().ToString("D"), SchedulerGroupName);
        }

        public static ScheduledTaskStatus GetDiscoScheduledTaskStatus(this IJobExecutionContext context)
        {
            return GetTaskStatus(context.JobDetail.Key.Name);
        }

        private class ScheduledTaskCleanup : IJob
        {
            public void Execute(IJobExecutionContext context)
            {
                lock (ScheduledTasks._RunningTasksLock)
                {
                    // Lifetime = 5mins
                    var expiredTime = DateTime.Now.AddMinutes(-1);
                    var expiredTasks = ScheduledTasks._RunningTasks.Where(
                            t => !t.IsRunning &&
                                 !t.NextScheduledTimestamp.HasValue &&
                                 t.FinishedTimestamp < expiredTime
                        ).ToArray();

                    foreach (var expiredTask in expiredTasks)
                        ScheduledTasks._RunningTasks.Remove(expiredTask);
                }
            }
            public static void Schedule(IScheduler TaskScheduler)
            {
                // Next 10min interval
                DateTime now = DateTime.Now;
                int mins = (10 - (now.Minute % 10));
                if (mins < 2)
                    mins += 10;
                DateTimeOffset startAt = new DateTimeOffset(now).AddMinutes(mins).AddSeconds(now.Second * -1).AddMilliseconds(now.Millisecond * -1);
                ITrigger trigger = TriggerBuilder.Create()
                    .StartAt(startAt)
                    .WithSchedule(SimpleScheduleBuilder.RepeatMinutelyForever(10))
                    .WithIdentity("ScheduledTaskCleanupTrigger", ScheduledTasks.SchedulerGroupName + "_System")
                    .Build();

                IJobDetail job = JobBuilder.Create<ScheduledTaskCleanup>()
                        .WithIdentity("ScheduledTaskCleanupJob", ScheduledTasks.SchedulerGroupName + "_System")
                        .Build();

                _TaskScheduler.ScheduleJob(job, trigger);
            }
        }
    }
}
