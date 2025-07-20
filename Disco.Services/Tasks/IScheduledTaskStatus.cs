using System;

namespace Disco.Services.Tasks
{
    public interface IScheduledTaskStatus
    {
        byte Progress { get; }
        string CurrentProcess { get; }
        string CurrentDescription { get; }

        bool IgnoreCurrentProcessChanges { get; set; }
        bool IgnoreCurrentDescription { get; set; }
        double ProgressMultiplier { get; set; }
        byte ProgressOffset { get; set; }

        DateTime? StartedTimestamp { get; }

        string FinishedMessage { get; }
        string FinishedUrl { get; }

        Exception TaskException { get; }

        void UpdateStatus(byte Progress);
        void UpdateStatus(double Progress);
        void UpdateStatus(string CurrentDescription);
        void UpdateStatus(byte Progress, string CurrentDescription);
        void UpdateStatus(double Progress, string CurrentDescription);
        void UpdateStatus(byte Progress, string CurrentProcess, string CurrentDescription);
        void UpdateStatus(double Progress, string CurrentProcess, string CurrentDescription);

        void SetFinishedUrl(string FinishedUrl);
        void SetFinishedMessage(string FinishedMessage);
        void Finished();
        void Finished(string FinishedMessage);
        void Finished(string FinishedMessage, string FinishedUrl);

        void SetTaskException(Exception TaskException);

        void LogWarning(string Message);
        void LogInformation(string Message);
    }
}
