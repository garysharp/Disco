using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Services.Tasks
{
    public class ScheduledTaskMockStatus : IScheduledTaskStatus
    {
        public byte Progress { get; set; }
        public string CurrentProcess { get; set; }
        public string CurrentDescription { get; set; }

        public bool IgnoreCurrentProcessChanges { get; set; }
        public bool IgnoreCurrentDescription { get; set; }
        public double ProgressMultiplier { get; set; }
        public byte ProgressOffset { get; set; }

        public string FinishedMessage { get; set; }
        public string FinishedUrl { get; set; }

        public Exception TaskException { get; set; }

        private byte CalculateProgressValue(byte Progress)
        {
            return (byte)((Progress * this.ProgressMultiplier) + this.ProgressOffset);
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
            Finished(this.FinishedMessage, this.FinishedUrl);
        }
        public void Finished(string FinishedMessage)
        {
            Finished(FinishedMessage, this.FinishedUrl);
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



        public static ScheduledTaskMockStatus Create()
        {
            return new ScheduledTaskMockStatus();
        }
    }
}
