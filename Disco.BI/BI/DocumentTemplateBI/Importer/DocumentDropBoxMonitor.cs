using System;
using System.IO;
using System.Web.Caching;
using Disco.Data.Repository;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Triggers;

namespace Disco.BI.DocumentTemplateBI.Importer
{
    public class DocumentDropBoxMonitor : System.IDisposable
    {
        private IScheduler _scheduler;
        private FileSystemWatcher _fsw;
        private Cache _httpCache;

        public const string WatcherFilter = "*.pdf";
        public string DropBoxLocation { get; private set; }

        public DocumentDropBoxMonitor(DiscoDataContext Database, ISchedulerFactory SchedulerFactory, Cache HttpCache)
        {
            if (Database == null)
                throw new System.ArgumentNullException("Context");
            
            this._httpCache = HttpCache;

            var location = DataStore.CreateLocation(Database, "DocumentDropBox");
            this.DropBoxLocation = location.EndsWith(@"\") ? location : string.Concat(location, @"\");
            
            this._scheduler = SchedulerFactory.GetScheduler();
            this._scheduler.Start();
        }
        public void ScheduleCurrentFiles(int Delay)
        {
            foreach (var filename in System.IO.Directory.GetFiles(this.DropBoxLocation, "*.pdf"))
            {
                this.ScheduleFile(filename, Delay);
            }
        }
        public void StartWatching()
        {
            if (this._fsw == null)
            {
                this._fsw = new FileSystemWatcher(this.DropBoxLocation, "*.pdf");
                this._fsw.Created += new FileSystemEventHandler(this.FSW_Created);
            }
            this._fsw.EnableRaisingEvents = true;
        }
        public void StopWatching()
        {
            if (this._fsw != null)
            {
                this._fsw.EnableRaisingEvents = false;
            }
        }
        public void ScheduleFile(string Filename, int Delay)
        {
            System.Guid guid = System.Guid.NewGuid();
            JobDetailImpl jd = new JobDetailImpl(guid.ToString(), typeof(DocumentImporterJob));
            jd.JobDataMap.Add("Filename", Filename);
            jd.JobDataMap.Add("RetryCount", 0);
            jd.JobDataMap.Add("HttpCache", this._httpCache);
            guid = System.Guid.NewGuid();
            
            System.DateTimeOffset startTimeUtc = new System.DateTimeOffset(DateTime.Now.AddSeconds((double)Delay));
            SimpleTriggerImpl trig = new SimpleTriggerImpl(guid.ToString(), startTimeUtc);
            
            this._scheduler.ScheduleJob(jd, trig);
        }
        private void FSW_Created(object sender, FileSystemEventArgs e)
        {
            if ((e.ChangeType & WatcherChangeTypes.Deleted) != WatcherChangeTypes.Deleted)
                this.ScheduleFile(e.FullPath, 5);
        }

        public void Dispose()
        {
            this.StopWatching();
            if (this._fsw != null)
                this._fsw.Dispose();
            if (this._scheduler != null)
                this._scheduler.Shutdown(false);
        }
    }
}
