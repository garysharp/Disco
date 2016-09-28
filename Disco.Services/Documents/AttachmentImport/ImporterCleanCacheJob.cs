using Disco.Data.Repository;
using Disco.Services.Logging;
using Disco.Services.Tasks;
using Quartz;
using System;
using System.IO;

namespace Disco.Services.Documents.AttachmentImport
{
    public class ImporterCleanCacheJob : ScheduledTask
    {
        public override string TaskName { get { return "Document Importer - Clean Cache Task"; } }

        public override bool SingleInstanceTask { get { return true; } }
        public override bool CancelInitiallySupported { get { return false; } }
        public override bool LogExceptionsOnly { get { return true; } }

        public override void InitalizeScheduledTask(DiscoDataContext Database)
        {
            // Trigger Daily @ 12:30am
            TriggerBuilder triggerBuilder = TriggerBuilder.Create().WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(0, 30));

            this.ScheduleTask(triggerBuilder);
        }

        protected override void ExecuteTask()
        {
            string dataStoreLocation;
            using (DiscoDataContext database = new DiscoDataContext())
            {
                dataStoreLocation = DataStore.CreateLocation(database, @"Cache\DocumentDropBox_SessionPages");
            }

            int deleteCount = 0;
            int errorCount = 0;

            var dataStoreInfo = new DirectoryInfo(dataStoreLocation);

            foreach (var file in dataStoreInfo.GetFiles())
            {
                try
                {
                    if (file.CreationTime < DateTime.Today)
                    {
                        file.Delete();
                        deleteCount++;
                    }
                }
                catch
                {
                    errorCount++;
                }
            }

            Status.LogInformation($"Cleared DocumentDropBox_SessionPages Cache, Deleted {deleteCount} File/s, with {errorCount} Error/s");
        }
    }
}
