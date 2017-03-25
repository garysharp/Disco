using Quartz;
using Quartz.Impl;
using Quartz.Impl.Triggers;
using System;
using System.IO;

namespace Disco.Services.Documents.AttachmentImport
{
    public class ImportDirectoryMonitor : IDisposable
    {
        private FileSystemWatcher watcher;

        public const string WatcherFilter = "*.pdf";
        public string MonitorLocation { get; private set; }
        public IScheduler Scheduler { get; private set; }
        public int ImportDelay { get; private set; }

        public ImportDirectoryMonitor(string MonitorLocation, IScheduler Scheduler, int ImportDelay)
        {
            if (MonitorLocation == null)
                throw new ArgumentNullException(nameof(MonitorLocation));
            if (Scheduler == null)
                throw new ArgumentNullException(nameof(Scheduler));

            this.MonitorLocation = MonitorLocation.EndsWith(@"\") ? MonitorLocation : $@"{MonitorLocation}\";
            this.Scheduler = Scheduler;
            this.ImportDelay = Math.Max(0, ImportDelay);
        }

        public void Start()
        {
            if (watcher == null)
            {
                if (!Directory.Exists(MonitorLocation))
                {
                    Directory.CreateDirectory(MonitorLocation);
                }

                watcher = new FileSystemWatcher(MonitorLocation, WatcherFilter);
                watcher.Created += OnFileCreated;
            }

            watcher.EnableRaisingEvents = true;
        }

        public void Stop()
        {
            if (watcher != null)
            {
                watcher.EnableRaisingEvents = false;
            }
        }

        private void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            if (!e.ChangeType.HasFlag(WatcherChangeTypes.Deleted))
            {
                ScheduleImport(e.FullPath, ImportDelay);
            }
        }

        private void ScheduleImport(string Filename, int ImportDelay)
        {
            var startTime = new DateTimeOffset(DateTime.Now.AddMilliseconds(ImportDelay));
            var jobTrigger = new SimpleTriggerImpl(Guid.NewGuid().ToString(), startTime);

            var jobDetails = new JobDetailImpl(Guid.NewGuid().ToString(), typeof(ImporterJob));
            jobDetails.JobDataMap.Add("Filename", Filename);
            jobDetails.JobDataMap.Add("RetryCount", 0);

            Scheduler.ScheduleJob(jobDetails, jobTrigger);
        }

        public void ScheduleCurrentFiles(int ImportDelay)
        {
            foreach (var filename in Directory.GetFiles(MonitorLocation, "*.pdf"))
            {
                ScheduleImport(filename, ImportDelay);
            }
        }

        public void Dispose()
        {
            if (watcher != null)
            {
                watcher.EnableRaisingEvents = false;
                watcher.Dispose();
                watcher = null;
            }
        }
    }
}
