using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Disco.Services.Tasks
{
    using ChangedItem = KeyValuePair<string, object>;

    public class ScheduledTaskStatus : IScheduledTaskStatus
    {
        private const string changedProperty_CurrentDescription = "CurrentDescription";
        private const string changedProperty_Progress = "Progress";
        private const string changedProperty_CurrentProcess = "CurrentProcess";

        #region Backing Fields

        private readonly string sessionId;
        private readonly string triggerKey;
        private readonly string taskName;
        private readonly Type taskType;
        private TaskCompletionSource<ScheduledTaskStatus> tcs;
        private bool isSilent;

        private byte progress;
        private string currentProcess;
        private string currentDescription;

        private Exception taskException;
        private readonly bool cancelInitiallySupported;
        private bool cancelSupported;
        private bool isCanceling;

        private DateTime? startedTimestamp;
        private DateTime? nextScheduledTimestamp;
        private DateTime? finishedTimestamp;

        private string finishedMessage;
        private string finishedUrl;

        private int statusVersion = 0;

        #endregion

        #region Properties

        public string SessionId => sessionId;
        public string TriggerKey => triggerKey;
        public string TaskName => taskName;
        public Type TaskType => taskType;
        public bool IsSilent => isSilent;

        public byte Progress => progress;
        public string CurrentProcess => currentProcess;
        public string CurrentDescription => currentDescription;

        public bool IgnoreCurrentProcessChanges { get; set; }
        public bool IgnoreCurrentDescription { get; set; }
        public double ProgressMultiplier { get; set; }
        public byte ProgressOffset { get; set; }

        public Exception TaskException => taskException;
        public bool CancelSupported => cancelSupported;
        public bool IsCanceling => isCanceling;

        public DateTime? StartedTimestamp => startedTimestamp;
        public DateTime? FinishedTimestamp => finishedTimestamp;
        public DateTime? NextScheduledTimestamp => nextScheduledTimestamp;

        public string FinishedMessage => finishedMessage;
        public string FinishedUrl => finishedUrl;

        public int StatusVersion => statusVersion;

        public bool IsRunning
            => startedTimestamp.HasValue && !finishedTimestamp.HasValue;

        public Task CompletionTask => tcs.Task;

        #endregion

        #region Events
        public delegate void UpdatedEvent(ScheduledTaskStatus sender, ChangedItem[] changedProperties);
        public delegate void CancelingEvent(ScheduledTaskStatus sender);
        public event UpdatedEvent Updated;
        public event CancelingEvent Canceling;
        #endregion

        public ScheduledTaskStatus(ScheduledTask task, string sessionId, string triggerKey, string finishedUrl = null)
        {
            taskName = task.TaskName;
            taskType = task.GetType();
            tcs = new TaskCompletionSource<ScheduledTaskStatus>();

            this.sessionId = sessionId;
            this.triggerKey = triggerKey;
            cancelInitiallySupported = task.CancelInitiallySupported;
            cancelSupported = cancelInitiallySupported;

            this.finishedUrl = finishedUrl;

            currentProcess = "Scheduled";
            currentDescription = "Scheduled Task for Execution";

            ProgressMultiplier = 1;
            progress = 0;
        }

        public void LogWarning(string message)
        {
            ScheduledTasksLog.LogScheduledTaskWarning(TaskName, SessionId, message);
        }

        public void LogInformation(string message)
        {
            ScheduledTasksLog.LogScheduledTaskInformation(TaskName, SessionId, message);
        }

        #region Progress Actions
        private byte CalculateProgressValue(double progress)
        {
            return (byte)((progress * ProgressMultiplier) + ProgressOffset);
        }

        public void UpdateStatus(byte progress)
        {
            this.progress = CalculateProgressValue(progress);
            UpdateTriggered(changedProperty_Progress, this.progress);
        }
        public void UpdateStatus(double progress)
        {
            this.progress = CalculateProgressValue(progress);
            UpdateTriggered(changedProperty_Progress, this.progress);
        }
        public void UpdateStatus(string currentDescription)
        {
            if (!IgnoreCurrentDescription)
            {
                this.currentDescription = currentDescription;
                UpdateTriggered(changedProperty_CurrentDescription, this.currentDescription);
            }
        }
        public void UpdateStatus(int progressIndex, int progressCount, string currentDescription)
        {
            if (progressCount <= 0)
                UpdateStatus(0, currentDescription);
            else
            {
                var p = (byte)((progressIndex / (double)progressCount) * 100);
                UpdateStatus(p, currentDescription);
            }
        }
        public void UpdateStatus(byte progress, string currentDescription)
        {
            this.progress = CalculateProgressValue(progress);

            var changedProperties = new List<ChangedItem>() {
                new ChangedItem(changedProperty_Progress, progress)
            };

            if (!IgnoreCurrentDescription)
            {
                this.currentDescription = currentDescription;
                changedProperties.Add(new ChangedItem(changedProperty_CurrentDescription, currentDescription));
            }

            UpdateTriggered(changedProperties.ToArray());
        }
        public void UpdateStatus(double progress, string currentDescription)
        {
            this.progress = CalculateProgressValue(progress);

            var changedProperties = new List<ChangedItem>() {
                new ChangedItem(changedProperty_Progress, progress)
            };

            if (!IgnoreCurrentDescription)
            {
                this.currentDescription = currentDescription;
                changedProperties.Add(new ChangedItem(changedProperty_CurrentDescription, currentDescription));
            }

            UpdateTriggered(changedProperties.ToArray());
        }
        public void UpdateStatus(byte progress, string currentProcess, string currentDescription)
        {
            this.progress = CalculateProgressValue(progress);

            var changedProperties = new List<ChangedItem>() {
                new ChangedItem(changedProperty_Progress, progress)
            };

            if (!IgnoreCurrentProcessChanges)
            {
                this.currentProcess = currentProcess;
                changedProperties.Add(new ChangedItem(changedProperty_CurrentProcess, currentProcess));
            }
            if (!IgnoreCurrentDescription)
            {
                this.currentDescription = currentDescription;
                changedProperties.Add(new ChangedItem(changedProperty_CurrentDescription, currentDescription));
            }

            UpdateTriggered(changedProperties.ToArray());
        }
        public void UpdateStatus(double progress, string currentProcess, string currentDescription)
        {
            this.progress = CalculateProgressValue(progress);

            var changedProperties = new List<ChangedItem>() {
                new ChangedItem(changedProperty_Progress, progress)
            };

            if (!IgnoreCurrentProcessChanges)
            {
                this.currentProcess = currentProcess;
                changedProperties.Add(new ChangedItem(changedProperty_CurrentProcess, currentProcess));
            }
            if (!IgnoreCurrentDescription)
            {
                this.currentDescription = currentDescription;
                changedProperties.Add(new ChangedItem(changedProperty_CurrentDescription, currentDescription));
            }

            UpdateTriggered(changedProperties.ToArray());
        }
        #endregion

        #region State Actions
        public bool Canceled()
        {
            if (!isCanceling)
            {
                if (cancelSupported)
                { // Cancelling
                    isCanceling = true;
                    UpdateTriggered("IsCancelling", true);
                    Canceling?.Invoke(this);
                    return true;
                }
                else
                { // Cancelling not supported
                    return false;
                }
            }
            else
            { // Already Cancelling
                return true;
            }
        }
        public void SetCancelSupported(bool cancelSupported)
        {
            if (this.cancelSupported != cancelSupported)
            {
                this.cancelSupported = cancelSupported;
                UpdateTriggered("CancelSupported", cancelSupported);
            }
        }
        public void SetTaskException(Exception taskException)
        {
            if (this.taskException != taskException)
            {
                this.taskException = taskException;
                UpdateTriggered("TaskExceptionMessage", this.taskException?.Message);
            }
        }
        public void SetIsSilent(bool IsSilent)
        {
            if (isSilent != IsSilent)
                isSilent = IsSilent;
        }
        public void SetFinishedUrl(string finishedUrl)
        {
            if (this.finishedUrl != finishedUrl)
            {
                this.finishedUrl = finishedUrl;
                UpdateTriggered("FinishedUrl", finishedUrl);
            }
        }
        public void SetFinishedMessage(string finishedMessage)
        {
            if (this.finishedMessage != finishedMessage)
            {
                this.finishedMessage = finishedMessage;
                UpdateTriggered("FinishedMessage", finishedMessage);
            }
        }
        public void SetNextScheduledTimestamp(DateTime? nextScheduledTimestamp)
        {
            if (this.nextScheduledTimestamp != nextScheduledTimestamp)
            {
                this.nextScheduledTimestamp = nextScheduledTimestamp;
                UpdateTriggered("NextScheduledTimestamp", nextScheduledTimestamp);
            }
        }
        public void Started()
        {
            var changedProperties = new List<ChangedItem>();

            // Change StartedTimestamp
            startedTimestamp = DateTime.Now;
            changedProperties.Add(new ChangedItem("StartedTimestamp", StartedTimestamp));

            if (finishedTimestamp != null)
            {
                finishedTimestamp = null;
                changedProperties.Add(new ChangedItem("FinishedTimestamp", finishedTimestamp));
            }

            if (nextScheduledTimestamp != null)
            {
                nextScheduledTimestamp = null;
                changedProperties.Add(new ChangedItem("NextScheduledTimestamp", nextScheduledTimestamp));
            }

            changedProperties.Add(new ChangedItem("IsRunning", IsRunning));

            if (progress != 0)
            {
                progress = 0;
                changedProperties.Add(new ChangedItem(changedProperty_Progress, progress));
            }
            if (currentProcess != "Starting")
            {
                currentProcess = "Starting";
                changedProperties.Add(new ChangedItem(changedProperty_CurrentProcess, currentProcess));
            }
            if (currentDescription != "Initializing Task for Execution")
            {
                currentDescription = "Initializing Task for Execution";
                changedProperties.Add(new ChangedItem(changedProperty_CurrentDescription, currentDescription));
            }
            if (taskException != null)
            {
                taskException = null;
                changedProperties.Add(new ChangedItem("TaskExceptionMessage", taskException?.Message));
            }
            if (cancelSupported != cancelInitiallySupported)
            {
                cancelSupported = cancelInitiallySupported;
                changedProperties.Add(new ChangedItem("CancelSupported", cancelSupported));
            }
            if (isCanceling)
            {
                isCanceling = false;
                changedProperties.Add(new ChangedItem("IsCanceling", isCanceling));
            }

            UpdateTriggered(changedProperties.ToArray());
        }
        public void Finished()
        {
            Finished(finishedMessage, finishedUrl);
        }
        public void Finished(string finishedMessage)
        {
            Finished(finishedMessage, finishedUrl);
        }
        public void Finished(string finishedMessage, string finishedUrl)
        {
            var changedProperties = new List<ChangedItem>();

            finishedTimestamp = DateTime.Now;
            changedProperties.Add(new ChangedItem("FinishedTimestamp", finishedTimestamp));
            changedProperties.Add(new ChangedItem("IsRunning", IsRunning));

            if (finishedMessage != this.finishedMessage)
            {
                this.finishedMessage = finishedMessage;
                changedProperties.Add(new ChangedItem("FinishedMessage", this.finishedMessage));
            }
            if (finishedUrl != this.finishedUrl)
            {
                this.finishedUrl = finishedUrl;
                changedProperties.Add(new ChangedItem("FinishedUrl", this.finishedUrl));
            }

            if (isCanceling)
            {
                isCanceling = false;
                changedProperties.Add(new ChangedItem("IsCanceling", isCanceling));
            }

            UpdateTriggered(changedProperties.ToArray());
        }
        internal void Finally()
        {
            tcs.SetResult(this);
        }
        public void Reset(DateTime? nextScheduledTimestamp)
        {
            tcs?.Task.Dispose();
            tcs = new TaskCompletionSource<ScheduledTaskStatus>();

            var changedProperties = new List<ChangedItem>();

            if (this.nextScheduledTimestamp != nextScheduledTimestamp)
            {
                this.nextScheduledTimestamp = nextScheduledTimestamp;
                changedProperties.Add(new ChangedItem("NextScheduledTimestamp", this.nextScheduledTimestamp));
            }

            if (startedTimestamp != null)
            {
                startedTimestamp = null;
                changedProperties.Add(new ChangedItem("StartedTimestamp", startedTimestamp));
            }
            if (finishedTimestamp != null)
            {
                finishedTimestamp = null;
                changedProperties.Add(new ChangedItem("FinishedTimestamp", finishedTimestamp));
            }
            if (finishedMessage != null)
            {
                finishedMessage = null;
                changedProperties.Add(new ChangedItem("FinishedMessage", finishedMessage));
            }
            if (finishedUrl != null)
            {
                finishedUrl = null;
                changedProperties.Add(new ChangedItem("FinishedUrl", finishedUrl));
            }
            if (progress != 0)
            {
                progress = 0;
                changedProperties.Add(new ChangedItem(changedProperty_Progress, progress));
            }
            ProgressMultiplier = 1;
            ProgressOffset = 0;
            IgnoreCurrentDescription = false;
            IgnoreCurrentProcessChanges = false;

            if (currentProcess != "Scheduled")
            {
                currentProcess = "Scheduled";
                changedProperties.Add(new ChangedItem(changedProperty_CurrentProcess, currentProcess));
            }
            if (currentDescription != "Scheduled Task for Execution")
            {
                currentDescription = "Scheduled Task for Execution";
                changedProperties.Add(new ChangedItem(changedProperty_CurrentDescription, currentDescription));
            }
            if (isCanceling)
            {
                isCanceling = false;
                changedProperties.Add(new ChangedItem("IsCanceling", isCanceling));
            }
            UpdateTriggered(changedProperties.ToArray());
        }
        public bool WaitUntilFinished(TimeSpan timeout)
        {
            var finished = tcs.Task.Wait(timeout);

            // Return false if task completed, but with an error
            if (finished)
                return TaskException == null;
            else
                return false;
        }
        #endregion

        private void UpdateTriggered(string changedProperty, object newValue)
        {
            UpdateTriggered(new ChangedItem(changedProperty, newValue));
        }

        private void UpdateTriggered(params ChangedItem[] changedProperties)
        {
            statusVersion++;

            Updated?.Invoke(this, changedProperties);

            if (!isSilent)
                ScheduledTaskNotificationsHub.PublishEvent(SessionId, changedProperties);
        }
    }
}
