using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Services.Tasks
{
    public class ScheduledTaskMockStatus : IScheduledTaskBasicStatus
    {
        private byte progress;
        private string currentProcess;
        private string currentDescription;

        public byte Progress { get { return this.progress; } }
        public string CurrentProcess { get { return this.currentProcess; } }
        public string CurrentDescription { get { return this.currentDescription; } }

        public void UpdateStatus(byte Progress)
        {
            this.progress = Progress;
        }
        public void UpdateStatus(double Progress)
        {
            UpdateStatus((byte)Progress);
        }
        public void UpdateStatus(string CurrentDescription)
        {
            this.currentDescription = CurrentDescription;
        }
        public void UpdateStatus(byte Progress, string CurrentDescription)
        {
            this.progress = Progress;
            this.currentDescription = CurrentDescription;
        }
        public void UpdateStatus(double Progress, string CurrentDescription)
        {
            UpdateStatus((byte)Progress, CurrentDescription);
        }
        public void UpdateStatus(byte Progress, string CurrentProcess, string CurrentDescription)
        {
            this.progress = Progress;
            this.currentProcess = CurrentProcess;
            this.currentDescription = CurrentDescription;
        }
        public void UpdateStatus(double Progress, string CurrentProcess, string CurrentDescription)
        {
            UpdateStatus((byte)Progress, CurrentProcess, CurrentDescription);
        }

        public static ScheduledTaskMockStatus Create()
        {
            return new ScheduledTaskMockStatus();
        }
    }
}
