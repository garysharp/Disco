using System;
using System.Collections.Generic;
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

        public string SessionId { get { return _sessionId; } }
        public string TriggerKey { get { return _triggerKey; } }
        public string TaskName { get { return _taskName; } }
        public Type TaskType { get { return _taskType; } }
        public bool IsSilent { get { return _isSilent; } }

        public byte Progress { get { return _progress; } }
        public string CurrentProcess { get { return _currentProcess; } }
        public string CurrentDescription { get { return _currentDescription; } }

        public bool IgnoreCurrentProcessChanges { get; set; }
        public bool IgnoreCurrentDescription { get; set; }
        public double ProgressMultiplier { get; set; }
        public byte ProgressOffset { get; set; }

        public Exception TaskException { get { return _taskException; } }
        public bool CancelSupported { get { return _cancelSupported; } }
        public bool IsCanceling { get { return _isCanceling; } }

        public DateTime? StartedTimestamp { get { return _startedTimestamp; } }
        public DateTime? FinishedTimestamp { get { return _finishedTimestamp; } }
        public DateTime? NextScheduledTimestamp { get { return _nextScheduledTimestamp; } }

        public string FinishedMessage { get { return _finishedMessage; } }
        public string FinishedUrl { get { return _finishedUrl; } }

        public int StatusVersion { get { return _statusVersion; } }

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
        public delegate void UpdatedEvent(ScheduledTaskStatus sender, ChangedItem[] ChangedProperties);
        public delegate void CancelingEvent(ScheduledTaskStatus sender);
        public event UpdatedEvent Updated;
        public event CancelingEvent Canceling;
        #endregion

        public ScheduledTaskStatus(ScheduledTask Task, string SessionId, string TriggerKey, string FinishedUrl = null)
        {
            _taskName = Task.TaskName;
            _taskType = Task.GetType();
            _tcs = new TaskCompletionSource<ScheduledTaskStatus>();

            _sessionId = SessionId;
            _triggerKey = TriggerKey;
            _cancelInitiallySupported = Task.CancelInitiallySupported;
            _cancelSupported = _cancelInitiallySupported;

            _finishedUrl = FinishedUrl;

            _currentProcess = "Scheduled";
            _currentDescription = "Scheduled Task for Execution";

            ProgressMultiplier = 1;
            _progress = 0;
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
        private byte CalculateProgressValue(double Progress)
        {
            return (byte)((Progress * ProgressMultiplier) + ProgressOffset);
        }

        public void UpdateStatus(byte Progress)
        {
            _progress = CalculateProgressValue(Progress);
            UpdateTriggered("Progress", _progress);
        }
        public void UpdateStatus(double Progress)
        {
            _progress = CalculateProgressValue(Progress);
            UpdateTriggered("Progress", _progress);
        }
        public void UpdateStatus(string CurrentDescription)
        {
            if (!IgnoreCurrentDescription)
            {
                _currentDescription = CurrentDescription;
                UpdateTriggered("CurrentDescription", _currentDescription);
            }
        }
        public void UpdateStatus(byte Progress, string CurrentDescription)
        {
            _progress = CalculateProgressValue(Progress);

            var changedProperties = new List<ChangedItem>() {
                new ChangedItem("Progress", Progress)
            };

            if (!IgnoreCurrentDescription)
            {
                _currentDescription = CurrentDescription;
                changedProperties.Add(new ChangedItem("CurrentDescription", CurrentDescription));
            }

            UpdateTriggered(changedProperties.ToArray());
        }
        public void UpdateStatus(double Progress, string CurrentDescription)
        {
            _progress = CalculateProgressValue(Progress);

            var changedProperties = new List<ChangedItem>() {
                new ChangedItem("Progress", Progress)
            };

            if (!IgnoreCurrentDescription)
            {
                _currentDescription = CurrentDescription;
                changedProperties.Add(new ChangedItem("CurrentDescription", CurrentDescription));
            }

            UpdateTriggered(changedProperties.ToArray());
        }
        public void UpdateStatus(byte Progress, string CurrentProcess, string CurrentDescription)
        {
            _progress = CalculateProgressValue(Progress);

            var changedProperties = new List<ChangedItem>() {
                new ChangedItem("Progress", Progress)
            };

            if (!IgnoreCurrentProcessChanges)
            {
                _currentProcess = CurrentProcess;
                changedProperties.Add(new ChangedItem("CurrentProcess", CurrentProcess));
            }
            if (!IgnoreCurrentDescription)
            {
                _currentDescription = CurrentDescription;
                changedProperties.Add(new ChangedItem("CurrentDescription", CurrentDescription));
            }

            UpdateTriggered(changedProperties.ToArray());
        }
        public void UpdateStatus(double Progress, string CurrentProcess, string CurrentDescription)
        {
            _progress = CalculateProgressValue(Progress);

            var changedProperties = new List<ChangedItem>() {
                new ChangedItem("Progress", Progress)
            };

            if (!IgnoreCurrentProcessChanges)
            {
                _currentProcess = CurrentProcess;
                changedProperties.Add(new ChangedItem("CurrentProcess", CurrentProcess));
            }
            if (!IgnoreCurrentDescription)
            {
                _currentDescription = CurrentDescription;
                changedProperties.Add(new ChangedItem("CurrentDescription", CurrentDescription));
            }

            UpdateTriggered(changedProperties.ToArray());
        }
        #endregion

        #region State Actions
        public bool Canceled()
        {
            if (!_isCanceling)
            {
                if (_cancelSupported)
                { // Cancelling
                    _isCanceling = true;
                    UpdateTriggered("IsCancelling", true);
                    if (Canceling != null)
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
            if (_cancelSupported != CancelSupported)
            {
                _cancelSupported = CancelSupported;
                UpdateTriggered("CancelSupported", CancelSupported);
            }
        }
        public void SetTaskException(Exception TaskException)
        {
            if (_taskException != TaskException)
            {
                _taskException = TaskException;
                UpdateTriggered("TaskExceptionMessage", (_taskException == null ? null : _taskException.Message));
            }
        }
        public void SetIsSilent(bool IsSilent)
        {
            if (_isSilent != IsSilent)
                _isSilent = IsSilent;
        }
        public void SetFinishedUrl(string FinishedUrl)
        {
            if (_finishedUrl != FinishedUrl)
            {
                _finishedUrl = FinishedUrl;
                UpdateTriggered("FinishedUrl", FinishedUrl);
            }
        }
        public void SetFinishedMessage(string FinishedMessage)
        {
            if (_finishedMessage != FinishedMessage)
            {
                _finishedMessage = FinishedMessage;
                UpdateTriggered("FinishedMessage", FinishedMessage);
            }
        }
        public void SetNextScheduledTimestamp(DateTime? NextScheduledTimestamp)
        {
            if (_nextScheduledTimestamp != NextScheduledTimestamp)
            {
                _nextScheduledTimestamp = NextScheduledTimestamp;
                UpdateTriggered("NextScheduledTimestamp", NextScheduledTimestamp);
            }
        }
        public void Started()
        {
            var changedProperties = new List<ChangedItem>();

            // Change StartedTimestamp
            _startedTimestamp = DateTime.Now;
            changedProperties.Add(new ChangedItem("StartedTimestamp", StartedTimestamp));

            if (_finishedTimestamp != null)
            {
                _finishedTimestamp = null;
                changedProperties.Add(new ChangedItem("FinishedTimestamp", _finishedTimestamp));
            }

            if (_nextScheduledTimestamp != null)
            {
                _nextScheduledTimestamp = null;
                changedProperties.Add(new ChangedItem("NextScheduledTimestamp", _nextScheduledTimestamp));
            }

            changedProperties.Add(new ChangedItem("IsRunning", IsRunning));

            if (_progress != 0)
            {
                _progress = 0;
                changedProperties.Add(new ChangedItem("Progress", _progress));
            }
            if (_currentProcess != "Starting")
            {
                _currentProcess = "Starting";
                changedProperties.Add(new ChangedItem("CurrentProcess", _currentProcess));
            }
            if (_currentDescription != "Initializing Task for Execution")
            {
                _currentDescription = "Initializing Task for Execution";
                changedProperties.Add(new ChangedItem("CurrentDescription", _currentDescription));
            }
            if (_taskException != null)
            {
                _taskException = null;
                changedProperties.Add(new ChangedItem("TaskExceptionMessage", (_taskException == null ? null : _taskException.Message)));
            }
            if (_cancelSupported != _cancelInitiallySupported)
            {
                _cancelSupported = _cancelInitiallySupported;
                changedProperties.Add(new ChangedItem("CancelSupported", _cancelSupported));
            }
            if (_isCanceling)
            {
                _isCanceling = false;
                changedProperties.Add(new ChangedItem("IsCanceling", _isCanceling));
            }

            UpdateTriggered(changedProperties.ToArray());
        }
        public void Finished()
        {
            Finished(_finishedMessage, _finishedUrl);
        }
        public void Finished(string FinishedMessage)
        {
            Finished(FinishedMessage, _finishedUrl);
        }
        public void Finished(string FinishedMessage, string FinishedUrl)
        {
            var changedProperties = new List<ChangedItem>();

            _finishedTimestamp = DateTime.Now;
            changedProperties.Add(new ChangedItem("FinishedTimestamp", _finishedTimestamp));
            changedProperties.Add(new ChangedItem("IsRunning", IsRunning));

            if (FinishedMessage != _finishedMessage)
            {
                _finishedMessage = FinishedMessage;
                changedProperties.Add(new ChangedItem("FinishedMessage", _finishedMessage));
            }
            if (FinishedUrl != _finishedUrl)
            {
                _finishedUrl = FinishedUrl;
                changedProperties.Add(new ChangedItem("FinishedUrl", _finishedUrl));
            }

            if (_isCanceling)
            {
                _isCanceling = false;
                changedProperties.Add(new ChangedItem("IsCanceling", _isCanceling));
            }

            UpdateTriggered(changedProperties.ToArray());
        }
        internal void Finally()
        {
            _tcs.SetResult(this);
        }
        public void Reset(DateTime? NextScheduledTimestamp)
        {
            if (_tcs != null)
                _tcs.Task.Dispose();
            _tcs = new TaskCompletionSource<ScheduledTaskStatus>();

            var changedProperties = new List<ChangedItem>();

            if (_nextScheduledTimestamp != NextScheduledTimestamp)
            {
                _nextScheduledTimestamp = NextScheduledTimestamp;
                changedProperties.Add(new ChangedItem("NextScheduledTimestamp", _nextScheduledTimestamp));
            }

            if (_startedTimestamp != null)
            {
                _startedTimestamp = null;
                changedProperties.Add(new ChangedItem("StartedTimestamp", _startedTimestamp));
            }
            if (_finishedTimestamp != null)
            {
                _finishedTimestamp = null;
                changedProperties.Add(new ChangedItem("FinishedTimestamp", _finishedTimestamp));
            }
            if (_finishedMessage != null)
            {
                _finishedMessage = null;
                changedProperties.Add(new ChangedItem("FinishedMessage", _finishedMessage));
            }
            if (_finishedUrl != null)
            {
                _finishedUrl = null;
                changedProperties.Add(new ChangedItem("FinishedUrl", _finishedUrl));
            }
            if (_progress != 0)
            {
                _progress = 0;
                changedProperties.Add(new ChangedItem("Progress", _progress));
            }
            ProgressMultiplier = 1;
            ProgressOffset = 0;
            IgnoreCurrentDescription = false;
            IgnoreCurrentProcessChanges = false;

            if (_currentProcess != "Scheduled")
            {
                _currentProcess = "Scheduled";
                changedProperties.Add(new ChangedItem("CurrentProcess", _currentProcess));
            }
            if (_currentDescription != "Scheduled Task for Execution")
            {
                _currentDescription = "Scheduled Task for Execution";
                changedProperties.Add(new ChangedItem("CurrentDescription", _currentDescription));
            }
            if (_isCanceling)
            {
                _isCanceling = false;
                changedProperties.Add(new ChangedItem("IsCanceling", _isCanceling));
            }
            UpdateTriggered(changedProperties.ToArray());
        }
        public bool WaitUntilFinished(TimeSpan Timeout)
        {
            var finished = _tcs.Task.Wait(Timeout);

            // Return false if task completed, but with an error
            if (finished)
                return TaskException == null;
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
            _statusVersion++;

            if (Updated != null)
                Updated(this, ChangedProperties);

            if (!_isSilent)
                ScheduledTaskNotificationsHub.PublishEvent(SessionId, ChangedProperties);
        }
    }
}
