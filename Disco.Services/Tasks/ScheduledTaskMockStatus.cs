using System;

namespace Disco.Services.Tasks
{
    public class ScheduledTaskMockStatus : IScheduledTaskStatus
    {
        public string TaskName { get; private set; }

        public byte Progress { get; set; }
        public string CurrentProcess { get; set; }
        public string CurrentDescription { get; set; }

        public bool IgnoreCurrentProcessChanges { get; set; }
        public bool IgnoreCurrentDescription { get; set; }
        public double ProgressMultiplier { get; set; }
        public byte ProgressOffset { get; set; }

        public DateTime? StartedTimestamp { get; } = DateTime.Now;

        public string FinishedMessage { get; set; }
        public string FinishedUrl { get; set; }

        public Exception TaskException { get; set; }

        [Obsolete("Use ScheduledTaskMockStatus.Create(TaskName) instead")]
        public ScheduledTaskMockStatus() : this("Unknown Task")
        { }

        public ScheduledTaskMockStatus(string TaskName)
        {
            this.TaskName = TaskName;
        }

        private byte CalculateProgressValue(byte Progress)
        {
            return (byte)((Progress * ProgressMultiplier) + ProgressOffset);
        }

        public void UpdateStatus(byte Progress)
        {
            this.Progress = CalculateProgressValue(Progress);
        }
        public void UpdateStatus(double Progress)
        {
            UpdateStatus((byte)Progress);
        }
        public void UpdateStatus(string CurrentDescription)
        {
            if (!IgnoreCurrentDescription)
                this.CurrentDescription = CurrentDescription;
        }
        public void UpdateStatus(byte Progress, string CurrentDescription)
        {
            this.Progress = CalculateProgressValue(Progress);
            if (!IgnoreCurrentDescription)
                this.CurrentDescription = CurrentDescription;
        }
        public void UpdateStatus(double Progress, string CurrentDescription)
        {
            UpdateStatus((byte)Progress, CurrentDescription);
        }
        public void UpdateStatus(byte Progress, string CurrentProcess, string CurrentDescription)
        {
            this.Progress = CalculateProgressValue(Progress);
            if (!IgnoreCurrentProcessChanges)
                this.CurrentProcess = CurrentProcess;
            if (!IgnoreCurrentDescription)
                this.CurrentDescription = CurrentDescription;
        }
        public void UpdateStatus(double Progress, string CurrentProcess, string CurrentDescription)
        {
            UpdateStatus((byte)Progress, CurrentProcess, CurrentDescription);
        }

        public void SetFinishedUrl(string FinishedUrl)
        {
            this.FinishedUrl = FinishedUrl;
        }
        public void SetFinishedMessage(string FinishedMessage)
        {
            this.FinishedMessage = FinishedMessage;
        }
        public void Finished()
        {
            Finished(FinishedMessage, FinishedUrl);
        }
        public void Finished(string FinishedMessage)
        {
            Finished(FinishedMessage, FinishedUrl);
        }
        public void Finished(string FinishedMessage, string FinishedUrl)
        {
            this.FinishedMessage = FinishedMessage;
            this.FinishedUrl = FinishedUrl;
        }

        public void SetTaskException(Exception TaskException)
        {
            this.TaskException = TaskException;
        }

        public void LogWarning(string Message)
        {
            ScheduledTasksLog.LogScheduledTaskWarning(TaskName, null, Message);
        }

        public void LogInformation(string Message)
        {
            ScheduledTasksLog.LogScheduledTaskInformation(TaskName, null, Message);
        }

        [Obsolete("Use ScheduledTaskMockStatus.Create(TaskName) instead")]
        public static ScheduledTaskMockStatus Create()
        {
            return new ScheduledTaskMockStatus();
        }

        public static ScheduledTaskMockStatus Create(string TaskName)
        {
            return new ScheduledTaskMockStatus(TaskName);
        }
    }
}
