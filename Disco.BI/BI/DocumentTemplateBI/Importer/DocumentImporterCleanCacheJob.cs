using Disco.Data.Repository;
using Disco.Services.Logging;
using Quartz;
using Quartz.Impl;
using Disco.Services.Tasks;

namespace Disco.BI.DocumentTemplateBI.Importer
{
    public class DocumentImporterCleanCacheJob : ScheduledTask
    {
        public override string TaskName { get { return "Document Importer - Clean Cache Task"; } }
        
        public override bool SingleInstanceTask { get { return true; } }
        public override bool CancelInitiallySupported { get { return false; } }

        public override void InitalizeScheduledTask(DiscoDataContext dbContext)
        {
            // Trigger Daily @ 12:30am
            TriggerBuilder triggerBuilder = TriggerBuilder.Create().WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(0, 30));

            this.ScheduleTask(triggerBuilder);
        }

        protected override void ExecuteTask()
        {
            string dataStoreLocation;
            using (DiscoDataContext dbContext = new DiscoDataContext())
            {
                dataStoreLocation = DataStore.CreateLocation(dbContext, "Cache\\DocumentDropBox_SessionPages");
            }

            int deleteCount = 0;
            int errorCount = 0;

            System.IO.DirectoryInfo dataStoreInfo = new System.IO.DirectoryInfo(dataStoreLocation);

            System.DateTime today = System.DateTime.Today;

            foreach (System.IO.FileInfo file in dataStoreInfo.GetFiles())
            {
                try
                {
                    if (file.CreationTime < today)
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

            SystemLog.LogInformation(
                string.Format("Cleared DocumentDropBox_SessionPages Cache, Deleted {0} File/s, with {1} Error/s", deleteCount, errorCount),
                deleteCount,
                errorCount
                );
        }

    }
}
