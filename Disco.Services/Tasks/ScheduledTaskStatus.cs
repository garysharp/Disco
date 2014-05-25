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
    public class ScheduledTaskStatus : IScheduledTaskBasicStatus
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

        public Task CompletionTask
        {
            get
            {
                return _tcs.Task;
            }
        }

        #endregion

        #region Events
        public delegate void UpdatedBroadcastEvent(ScheduledTaskStatusLive SessionStatus);
        public delegate void UpdatedEvent(ScheduledTaskStatus sender, string[] ChangedProperties);
        public delegate void CancelingEvent(ScheduledTaskStatus sender);
        public static event UpdatedBroadcastEvent UpdatedBroadcast;
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

            this._progress = 0;
        }

        #region Progress Actions
        public void UpdateStatus(byte Progress)
        {
            this._progress = Progress;
            UpdateTriggered(new string[] { "Progress" });
        }
        public void UpdateStatus(double Progress)
        {
            UpdateStatus((byte)Progress);
        }
        public void UpdateStatus(string CurrentDescription)
        {
            this._currentDescription = CurrentDescription;
            UpdateTriggered(new string[] { "CurrentDescription" });
        }
        public void UpdateStatus(byte Progress, string CurrentDescription)
        {
            this._progress = Progress;
            this._currentDescription = CurrentDescription;
            UpdateTriggered(new string[] { "Progress", "CurrentDescription" });
        }
        public void UpdateStatus(double Progress, string CurrentDescription)
        {
            UpdateStatus((byte)Progress, CurrentDescription);
        }
        public void UpdateStatus(byte Progress, string CurrentProcess, string CurrentDescription)
        {
            this._progress = Progress;
            this._currentProcess = CurrentProcess;
            this._currentDescription = CurrentDescription;
            UpdateTriggered(new string[] { "Progress", "CurrentProcess", "CurrentDescription" });
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
                    UpdateTriggered(new string[] { "IsCancelling" });
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
                UpdateTriggered(new string[] { "CancelSupported" });
            }
        }
        public void SetTaskException(Exception TaskException)
        {
            if (this._taskException != TaskException)
            {
                this._taskException = TaskException;
                UpdateTriggered(new string[] { "TaskException" });
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
                UpdateTriggered(new string[] { "FinishedUrl" });
            }
        }
        public void SetFinishedMessage(string FinishedMessage)
        {
            if (this._finishedMessage != FinishedMessage)
            {
                this._finishedMessage = FinishedMessage;
                UpdateTriggered(new string[] { "FinishedMessage" });
            }
        }
        public void SetNextScheduledTimestamp(DateTime? NextScheduledTimestamp)
        {
            if (this._nextScheduledTimestamp != NextScheduledTimestamp)
            {
                this._nextScheduledTimestamp = NextScheduledTimestamp;
                UpdateTriggered(new string[] { "NextScheduledTimestamp" });
            }
        }
        public void Started()
        {
            List<string> changedProperties = new List<string>() { "IsRunning", "StartedTimestamp" };

            this._startedTimestamp = DateTime.Now;

            if (this._nextScheduledTimestamp != null)
            {
                this._nextScheduledTimestamp = null;
                changedProperties.Add("NextScheduledTimestamp");
            }
            if (this._finishedTimestamp != null)
            {
                this._finishedTimestamp = null;
                changedProperties.Add("FinishedTimestamp");
            }
            if (this._progress != 0)
            {
                this._progress = 0;
                changedProperties.Add("Progress");
            }
            if (this._currentProcess != "Starting")
            {
                this._currentProcess = "Starting";
                changedProperties.Add("CurrentProcess");
            }
            if (this._currentDescription != "Initializing Task for Execution")
            {
                this._currentDescription = "Initializing Task for Execution";
                changedProperties.Add("CurrentDescription");
            }
            if (this._taskException != null)
            {
                this._taskException = null;
                changedProperties.Add("TaskException");
            }
            if (this._cancelSupported != this._cancelInitiallySupported)
            {
                this._cancelSupported = this._cancelInitiallySupported;
                changedProperties.Add("CancelSupported");
            }
            {
                this._isCanceling = false;
                changedProperties.Add("IsCanceling");
            }
            if (this._isCanceling)
            {
                this._isCanceling = false;
                changedProperties.Add("IsCanceling");
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
            List<string> changedProperties = new List<string>() { "IsRunning", "FinishedTimestamp" };

            this._finishedTimestamp = DateTime.Now;

            if (FinishedMessage != this._finishedMessage)
            {
                this._finishedMessage = FinishedMessage;
                changedProperties.Add("FinishedMessage");
            }
            if (FinishedUrl != this._finishedUrl)
            {
                this._finishedUrl = FinishedUrl;
                changedProperties.Add("FinishedUrl");
            }

            if (this._isCanceling)
            {
                this._isCanceling = false;
                changedProperties.Add("IsCanceling");
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

            List<string> changedProperties = new List<string>();

            if (this._nextScheduledTimestamp != NextScheduledTimestamp)
            {
                this._nextScheduledTimestamp = NextScheduledTimestamp;
                changedProperties.Add("NextScheduledTimestamp");
            }

            if (this._startedTimestamp != null)
            {
                this._startedTimestamp = null;
                changedProperties.Add("StartedTimestamp");
            }
            if (this._finishedTimestamp != null)
            {
                this._finishedTimestamp = null;
                changedProperties.Add("FinishedTimestamp");
            }
            if (this._finishedMessage != null)
            {
                this._finishedMessage = null;
                changedProperties.Add("FinishedMessage");
            }
            if (this._finishedUrl != null)
            {
                this._finishedUrl = null;
                changedProperties.Add("FinishedUrl");
            }
            if (this._progress != 0)
            {
                this._progress = 0;
                changedProperties.Add("Progress");
            }
            if (this._currentProcess != "Scheduled")
            {
                this._currentProcess = "Scheduled";
                changedProperties.Add("CurrentProcess");
            }
            if (this._currentDescription != "Scheduled Task for Execution")
            {
                this._currentDescription = "Scheduled Task for Execution";
                changedProperties.Add("CurrentDescription");
            }
            if (this._isCanceling)
            {
                this._isCanceling = false;
                changedProperties.Add("IsCanceling");
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

        private void UpdateTriggered(string[] ChangedProperties)
        {
            this._statusVersion++;

            if (Updated != null)
                Updated(this, ChangedProperties);

            if (!_isSilent && UpdatedBroadcast != null)
                UpdatedBroadcast.Invoke(ScheduledTaskStatusLive.FromScheduledTaskStatus(this, ChangedProperties));
        }
    }
}
