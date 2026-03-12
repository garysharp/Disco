using Quartz;
using System;
using System.Threading;
using System.Web;

namespace Disco.Services.Tasks
{
    public class RestartAppScheduledTask : ScheduledTask
    {
        public override string TaskName { get; } = "Restarting Disco ICT";

        public override bool LogExceptionsOnly { get; } = true;
        public override bool CancelInitiallySupported { get; } = false;
        public override bool SingleInstanceTask { get; } = true;

        public static ScheduledTaskStatus ScheduleNow(TimeSpan delay)
        {
            var taskData = new JobDataMap() { { nameof(delay), delay } };

            var task = new RestartAppScheduledTask();
            return task.ScheduleTask(taskData);
        }

        protected override void ExecuteTask()
        {
            var delay = (TimeSpan)ExecutionContext.JobDetail.JobDataMap["delay"];

            RestartApp(delay);
        }

        private static readonly object _restartTimerLock = new object();
        private static Timer _restartTimer;
        internal static void RestartApp(TimeSpan delay)
        {
            lock (_restartTimerLock)
            {
                if (_restartTimer != null)
                {
                    _restartTimer.Dispose();
                }

                if (delay == TimeSpan.Zero)
                    HttpRuntime.UnloadAppDomain();
                else
                {
                    _restartTimer = new Timer((state) =>
                    {
                        HttpRuntime.UnloadAppDomain();
                    }, null, (int)delay.TotalMilliseconds, Timeout.Infinite);
                }
            }
        }

    }
}
