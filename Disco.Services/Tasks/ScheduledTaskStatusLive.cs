using System;

namespace Disco.Services.Tasks
{
    public class ScheduledTaskStatusLive
    {
        public string TaskName { get; set; }
        public string SessionId { get; set; }

        public byte Progress { get; set; }
        public string CurrentProcess { get; set; }
        public string CurrentDescription { get; set; }

        public string TaskExceptionMessage { get; set; }
        public bool CancelSupported { get; set; }
        public bool IsCancelling { get; set; }

        public bool IsRunning { get; set; }
        public DateTime? StartedTimestamp { get; set; }
        public DateTime? FinishedTimestamp { get; set; }
        public DateTime? NextScheduledTimestamp { get; set; }

        public string FinishedMessage { get; set; }
        public string FinishedUrl { get; set; }

        public int StatusVersion { get; set; }

        public string[] ChangedProperties { get; set; }

        public static ScheduledTaskStatusLive FromScheduledTaskStatus(ScheduledTaskStatus Status, string[] ChangedProperties)
        {
            return new ScheduledTaskStatusLive()
            {
                TaskName = Status.TaskName,
                SessionId = Status.SessionId,
                Progress = Status.Progress,
                CurrentProcess = Status.CurrentProcess,
                CurrentDescription = Status.CurrentDescription,
                CancelSupported = Status.CancelSupported,
                TaskExceptionMessage = (Status.TaskException == null ? null : Status.TaskException.Message),
                IsCancelling = Status.IsCanceling,
                IsRunning = Status.IsRunning,
                StartedTimestamp = Status.StartedTimestamp,
                FinishedTimestamp = Status.FinishedTimestamp,
                NextScheduledTimestamp = Status.NextScheduledTimestamp,
                FinishedMessage = Status.FinishedMessage,
                FinishedUrl = Status.FinishedUrl,
                StatusVersion = Status.StatusVersion,
                ChangedProperties = ChangedProperties
            };
        }
    }
}
