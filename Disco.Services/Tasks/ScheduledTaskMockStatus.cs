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

        public ScheduledTaskMockStatus(string taskName)
        {
            TaskName = taskName;
        }

        private byte CalculateProgressValue(byte progress)
        {
            return (byte)((progress * ProgressMultiplier) + ProgressOffset);
        }

        public void UpdateStatus(byte progress)
        {
            Progress = CalculateProgressValue(progress);
        }
        public void UpdateStatus(double progress)
        {
            UpdateStatus((byte)progress);
        }
        public void UpdateStatus(string currentDescription)
        {
            if (!IgnoreCurrentDescription)
                CurrentDescription = currentDescription;
        }
        public void UpdateStatus(int progressIndex, int progressCount, string currentDescription)
        {
            if (progressCount <= 0)
                UpdateStatus(0, currentDescription);
            else
            {
                var progress = (byte)((progressIndex / (double)progressCount) * 100);
                UpdateStatus(progress, currentDescription);
            }
        }
        public void UpdateStatus(byte progress, string currentDescription)
        {
            Progress = CalculateProgressValue(progress);
            if (!IgnoreCurrentDescription)
                CurrentDescription = currentDescription;
        }
        public void UpdateStatus(double progress, string currentDescription)
        {
            UpdateStatus((byte)progress, currentDescription);
        }
        public void UpdateStatus(byte progress, string currentProcess, string currentDescription)
        {
            Progress = CalculateProgressValue(progress);
            if (!IgnoreCurrentProcessChanges)
                CurrentProcess = currentProcess;
            if (!IgnoreCurrentDescription)
                CurrentDescription = currentDescription;
        }
        public void UpdateStatus(double progress, string currentProcess, string currentDescription)
        {
            UpdateStatus((byte)progress, currentProcess, currentDescription);
        }

        public void SetFinishedUrl(string finishedUrl)
        {
            FinishedUrl = finishedUrl;
        }
        public void SetFinishedMessage(string finishedMessage)
        {
            FinishedMessage = finishedMessage;
        }
        public void Finished()
        {
            Finished(FinishedMessage, FinishedUrl);
        }
        public void Finished(string finishedMessage)
        {
            Finished(finishedMessage, FinishedUrl);
        }
        public void Finished(string finishedMessage, string finishedUrl)
        {
            FinishedMessage = finishedMessage;
            FinishedUrl = finishedUrl;
        }

        public void SetTaskException(Exception taskException)
        {
            TaskException = taskException;
        }

        public void LogWarning(string message)
        {
            ScheduledTasksLog.LogScheduledTaskWarning(TaskName, null, message);
        }

        public void LogInformation(string message)
        {
            ScheduledTasksLog.LogScheduledTaskInformation(TaskName, null, message);
        }

        [Obsolete("Use ScheduledTaskMockStatus.Create(TaskName) instead")]
        public static ScheduledTaskMockStatus Create()
        {
            return new ScheduledTaskMockStatus();
        }

        public static ScheduledTaskMockStatus Create(string taskName)
        {
            return new ScheduledTaskMockStatus(taskName);
        }
    }
}
