using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Services.Tasks
{
    public interface IScheduledTaskBasicStatus
    {
        byte Progress { get; }
        string CurrentProcess { get; }
        string CurrentDescription { get; }

        void UpdateStatus(byte Progress);
        void UpdateStatus(double Progress);
        void UpdateStatus(string CurrentDescription);
        void UpdateStatus(byte Progress, string CurrentDescription);
        void UpdateStatus(double Progress, string CurrentDescription);
        void UpdateStatus(byte Progress, string CurrentProcess, string CurrentDescription);
        void UpdateStatus(double Progress, string CurrentProcess, string CurrentDescription);
    }
}
