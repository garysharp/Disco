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

        void UpdateStatus(byte progress);
        void UpdateStatus(double progress);
        void UpdateStatus(int progressIndex, int progressCount, string currentDescription);
        void UpdateStatus(string currentDescription);
        void UpdateStatus(byte progress, string currentDescription);
        void UpdateStatus(double progress, string currentDescription);
        void UpdateStatus(byte progress, string currentProcess, string currentDescription);
        void UpdateStatus(double progress, string currentProcess, string currentDescription);

        void SetFinishedUrl(string finishedUrl);
        void SetFinishedMessage(string finishedMessage);
        void Finished();
        void Finished(string finishedMessage);
        void Finished(string finishedMessage, string finishedUrl);

        void SetTaskException(Exception taskException);

        void LogWarning(string message);
        void LogInformation(string message);
    }
}
