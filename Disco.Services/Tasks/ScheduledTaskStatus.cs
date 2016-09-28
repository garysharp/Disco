using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quartz;
using System.Web.Script.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Disco.Services.Tasks
{
    using ChangedItem = KeyValuePair<string, object>;

    public class ScheduledTaskStatus : IScheduledTaskStatus
    {
        #region Backing Fields

        private string _sessionId;
        private string _triggerKey;
        private string _taskName;
        private Type _taskType;
        private TaskCompletionSource<ScheduledTaskStatus> _tcs;
        private bool _isSilent;

        private byte _progress;
        private string _currentProcess;
        private string _currentDescription;

        private Exception _taskException;
        private bool _cancelInitiallySupported;
        private bool _cancelSupported;
        private bool _isCanceling;

        private DateTime? _startedTimestamp;
        private DateTime? _nextScheduledTimestamp;
        private DateTime? _finishedTimestamp;

        private string _finishedMessage;
        private string _finishedUrl;

        private int _statusVersion = 0;

        #endregion

        #region Properties

        public string SessionId { get { return this._sessionId; } }
        public string TriggerKey { get { return this._triggerKey; } }
        public string TaskName { get { return this._taskName; } }
        public Type TaskType { get { return this._taskType; } }
        public bool IsSilent { get { return this._isSilent; } }

        public byte Progress { get { return this._progress; } }
        public string CurrentProcess { get { return this._currentProcess; } }
        public string CurrentDescription { get { return this._currentDescription; } }

        public bool IgnoreCurrentProcessChanges { get; set; }
        public bool IgnoreCurrentDescription { get; set; }
        public double ProgressMultiplier { get; set; }
        public byte ProgressOffset { get; set; }

        public Exception TaskException { get { return this._taskException; } }
        public bool CancelSupported { get { return this._cancelSupported; } }
        public bool IsCanceling { get { return this._isCanceling; } }

        public DateTime? StartedTimestamp { get { return this._startedTimestamp; } }
        public DateTime? FinishedTimestamp { get { return this._finishedTimestamp; } }
        public DateTime? NextScheduledTimestamp { get { return this._nextScheduledTimestamp; } }

        public string FinishedMessage { get { return this._finishedMessage; } }
        public string FinishedUrl { get { return this._finishedUrl; } }

        public int StatusVersion { get { return this._statusVersion; } }

        public bool IsRunning
        {
            get
            {
                return _startedTimestamp.HasValue && !_finishedTimestamp.HasValue;
            }
        }

        public Task CompletionTask { get { return _tcs.Task; } }

        #endregion

        #region Events
        public delegate void UpdatedEvent(ScheduledTaskStatus sender, KeyValuePair<string, object>[] ChangedProperties);
        public delegate void CancelingEvent(ScheduledTaskStatus sender);
        public event UpdatedEvent Updated;
        public event CancelingEvent Canceling;
        #endregion

        public ScheduledTaskStatus(ScheduledTask Task, string SessionId, string TriggerKey, string FinishedUrl = null)
        {
            this._taskName = Task.TaskName;
            this._taskType = Task.GetType();
            this._tcs = new TaskCompletionSource<ScheduledTaskStatus>();

            this._sessionId = SessionId;
            this._triggerKey = TriggerKey;
            this._cancelInitiallySupported = Task.CancelInitiallySupported;
            this._cancelSupported = this._cancelInitiallySupported;

            this._finishedUrl = FinishedUrl;

            this._currentProcess = "Scheduled";
            this._currentDescription = "Scheduled Task for Execution";

            this.ProgressMultiplier = 1;
            this._progress = 0;
        }

        public void LogWarning(string Message)
        {
            ScheduledTasksLog.LogScheduledTaskWarning(TaskName, SessionId, Message);
        }

        public void LogInformation(string Message)
        {
            ScheduledTasksLog.LogScheduledTaskInformation(TaskName, SessionId, Message);
        }

        #region Progress Actions
        private byte CalculateProgressValue(byte Progress)
        {
            return (byte)((Progress * this.ProgressMultiplier) + this.ProgressOffset);
        }

        public void UpdateStatus(byte Progress)
        {
            this._progress = CalculateProgressValue(Progress);
            UpdateTriggered("Progress", this._progress);
        }
        public void UpdateStatus(double Progress)
        {
            UpdateStatus((byte)Progress);
        }
        public void UpdateStatus(string CurrentDescription)
        {
            if (!IgnoreCurrentDescription)
            {
                this._currentDescription = CurrentDescription;
                UpdateTriggered("CurrentDescription", this._currentDescription);
            }
        }
        public void UpdateStatus(byte Progress, string CurrentDescription)
        {
            this._progress = CalculateProgressValue(Progress);

            var changedProperties = new List<ChangedItem>() {
                new ChangedItem("Progress", Progress)
            };

            if (!IgnoreCurrentDescription)
            {
                this._currentDescription = CurrentDescription;
                changedProperties.Add(new ChangedItem("CurrentDescription", CurrentDescription));
            }

            UpdateTriggered(changedProperties.ToArray());
        }
        public void UpdateStatus(double Progress, string CurrentDescription)
        {
            UpdateStatus((byte)Progress, CurrentDescription);
        }
        public void UpdateStatus(byte Progress, string CurrentProcess, string CurrentDescription)
        {
            this._progress = CalculateProgressValue(Progress);

            var changedProperties = new List<ChangedItem>() {
                new ChangedItem("Progress", Progress)
            };

            if (!IgnoreCurrentProcessChanges)
            {
                this._currentProcess = CurrentProcess;
                changedProperties.Add(new ChangedItem("CurrentProcess", CurrentProcess));
            }
            if (!IgnoreCurrentDescription)
            {
                this._currentDescription = CurrentDescription;
                changedProperties.Add(new ChangedItem("CurrentDescription", CurrentDescription));
            }

            UpdateTriggered(changedProperties.ToArray());
        }
        public void UpdateStatus(double Progress, string CurrentProcess, string CurrentDescription)
        {
            UpdateStatus((byte)Progress, CurrentProcess, CurrentDescription);
        }
        #endregion

        #region State Actions
        public bool Canceled()
        {
            if (!this._isCanceling)
            {
                if (_cancelSupported)
                { // Cancelling
                    this._isCanceling = true;
                    UpdateTriggered("IsCancelling", true);
                    if (this.Canceling != null)
                        Canceling(this);
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
        public void SetCancelSupported(bool CancelSupported)
        {
            if (this._cancelSupported != CancelSupported)
            {
                this._cancelSupported = CancelSupported;
                UpdateTriggered("CancelSupported", CancelSupported);
            }
        }
        public void SetTaskException(Exception TaskException)
        {
            if (this._taskException != TaskException)
            {
                this._taskException = TaskException;
                UpdateTriggered("TaskExceptionMessage", (this._taskException == null ? null : this._taskException.Message));
            }
        }
        public void SetIsSilent(bool IsSilent)
        {
            if (this._isSilent != IsSilent)
                this._isSilent = IsSilent;
        }
        public void SetFinishedUrl(string FinishedUrl)
        {
            if (this._finishedUrl != FinishedUrl)
            {
                this._finishedUrl = FinishedUrl;
                UpdateTriggered("FinishedUrl", FinishedUrl);
            }
        }
        public void SetFinishedMessage(string FinishedMessage)
        {
            if (this._finishedMessage != FinishedMessage)
            {
                this._finishedMessage = FinishedMessage;
                UpdateTriggered("FinishedMessage", FinishedMessage);
            }
        }
        public void SetNextScheduledTimestamp(DateTime? NextScheduledTimestamp)
        {
            if (this._nextScheduledTimestamp != NextScheduledTimestamp)
            {
                this._nextScheduledTimestamp = NextScheduledTimestamp;
                UpdateTriggered("NextScheduledTimestamp", NextScheduledTimestamp);
            }
        }
        public void Started()
        {
            var changedProperties = new List<ChangedItem>();

            // Change StartedTimestamp
            this._startedTimestamp = DateTime.Now;
            changedProperties.Add(new ChangedItem("StartedTimestamp", this.StartedTimestamp));

            if (this._finishedTimestamp != null)
            {
                this._finishedTimestamp = null;
                changedProperties.Add(new ChangedItem("FinishedTimestamp", this._finishedTimestamp));
            }

            if (this._nextScheduledTimestamp != null)
            {
                this._nextScheduledTimestamp = null;
                changedProperties.Add(new ChangedItem("NextScheduledTimestamp", this._nextScheduledTimestamp));
            }

            changedProperties.Add(new ChangedItem("IsRunning", this.IsRunning));

            if (this._progress != 0)
            {
                this._progress = 0;
                changedProperties.Add(new ChangedItem("Progress", this._progress));
            }
            if (this._currentProcess != "Starting")
            {
                this._currentProcess = "Starting";
                changedProperties.Add(new ChangedItem("CurrentProcess", this._currentProcess));
            }
            if (this._currentDescription != "Initializing Task for Execution")
            {
                this._currentDescription = "Initializing Task for Execution";
                changedProperties.Add(new ChangedItem("CurrentDescription", this._currentDescription));
            }
            if (this._taskException != null)
            {
                this._taskException = null;
                changedProperties.Add(new ChangedItem("TaskExceptionMessage", (this._taskException == null ? null : this._taskException.Message)));
            }
            if (this._cancelSupported != this._cancelInitiallySupported)
            {
                this._cancelSupported = this._cancelInitiallySupported;
                changedProperties.Add(new ChangedItem("CancelSupported", this._cancelSupported));
            }
            if (this._isCanceling)
            {
                this._isCanceling = false;
                changedProperties.Add(new ChangedItem("IsCanceling", this._isCanceling));
            }

            UpdateTriggered(changedProperties.ToArray());
        }
        public void Finished()
        {
            Finished(this._finishedMessage, this._finishedUrl);
        }
        public void Finished(string FinishedMessage)
        {
            Finished(FinishedMessage, this._finishedUrl);
        }
        public void Finished(string FinishedMessage, string FinishedUrl)
        {
            var changedProperties = new List<ChangedItem>();

            this._finishedTimestamp = DateTime.Now;
            changedProperties.Add(new ChangedItem("FinishedTimestamp", this._finishedTimestamp));
            changedProperties.Add(new ChangedItem("IsRunning", this.IsRunning));

            if (FinishedMessage != this._finishedMessage)
            {
                this._finishedMessage = FinishedMessage;
                changedProperties.Add(new ChangedItem("FinishedMessage", this._finishedMessage));
            }
            if (FinishedUrl != this._finishedUrl)
            {
                this._finishedUrl = FinishedUrl;
                changedProperties.Add(new ChangedItem("FinishedUrl", this._finishedUrl));
            }

            if (this._isCanceling)
            {
                this._isCanceling = false;
                changedProperties.Add(new ChangedItem("IsCanceling", this._isCanceling));
            }

            UpdateTriggered(changedProperties.ToArray());
        }
        internal void Finally()
        {
            this._tcs.SetResult(this);
        }
        public void Reset(DateTime? NextScheduledTimestamp)
        {
            if (this._tcs != null)
                this._tcs.Task.Dispose();
            this._tcs = new TaskCompletionSource<ScheduledTaskStatus>();

            var changedProperties = new List<ChangedItem>();

            if (this._nextScheduledTimestamp != NextScheduledTimestamp)
            {
                this._nextScheduledTimestamp = NextScheduledTimestamp;
                changedProperties.Add(new ChangedItem("NextScheduledTimestamp", this._nextScheduledTimestamp));
            }

            if (this._startedTimestamp != null)
            {
                this._startedTimestamp = null;
                changedProperties.Add(new ChangedItem("StartedTimestamp", this._startedTimestamp));
            }
            if (this._finishedTimestamp != null)
            {
                this._finishedTimestamp = null;
                changedProperties.Add(new ChangedItem("FinishedTimestamp", this._finishedTimestamp));
            }
            if (this._finishedMessage != null)
            {
                this._finishedMessage = null;
                changedProperties.Add(new ChangedItem("FinishedMessage", this._finishedMessage));
            }
            if (this._finishedUrl != null)
            {
                this._finishedUrl = null;
                changedProperties.Add(new ChangedItem("FinishedUrl", this._finishedUrl));
            }
            if (this._progress != 0)
            {
                this._progress = 0;
                changedProperties.Add(new ChangedItem("Progress", this._progress));
            }
            this.ProgressMultiplier = 1;
            this.ProgressOffset = 0;
            this.IgnoreCurrentDescription = false;
            this.IgnoreCurrentProcessChanges = false;

            if (this._currentProcess != "Scheduled")
            {
                this._currentProcess = "Scheduled";
                changedProperties.Add(new ChangedItem("CurrentProcess", this._currentProcess));
            }
            if (this._currentDescription != "Scheduled Task for Execution")
            {
                this._currentDescription = "Scheduled Task for Execution";
                changedProperties.Add(new ChangedItem("CurrentDescription", this._currentDescription));
            }
            if (this._isCanceling)
            {
                this._isCanceling = false;
                changedProperties.Add(new ChangedItem("IsCanceling", this._isCanceling));
            }
            UpdateTriggered(changedProperties.ToArray());
        }
        public bool WaitUntilFinished(TimeSpan Timeout)
        {
            var finished = this._tcs.Task.Wait(Timeout);

            // Return false if task completed, but with an error
            if (finished)
                return this.TaskException == null;
            else
                return false;
        }
        #endregion

        private void UpdateTriggered(string ChangedProperty, object NewValue)
        {
            UpdateTriggered(new ChangedItem[] { new ChangedItem(ChangedProperty, NewValue) });
        }

        private void UpdateTriggered(params ChangedItem[] ChangedProperties)
        {
            this._statusVersion++;

            if (Updated != null)
                Updated(this, ChangedProperties);

            if (!_isSilent)
                ScheduledTaskNotificationsHub.PublishEvent(this.SessionId, ChangedProperties);
        }
    }
}
