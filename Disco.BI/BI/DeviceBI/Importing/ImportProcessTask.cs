using Disco.Data.Repository;
using Disco.Models.BI.Device;
using Disco.Services.Tasks;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Disco.BI.DeviceBI.Importing
{
    public class ImportProcessTask : ScheduledTask
    {
        public override string TaskName { get { return "Import Devices - Processing Changes"; } }

        public override bool SingleInstanceTask { get { return false; } }
        public override bool CancelInitiallySupported { get { return false; } }

        protected override void ExecuteTask()
        {
            string parseKey = (string)this.ExecutionContext.JobDetail.JobDataMap["ParseKey"];

            if (string.IsNullOrWhiteSpace(parseKey))
                throw new ArgumentNullException("ParseKey");

            parseKey = string.Format(ImportParseTask.ImportParseCacheKey, parseKey);

            List<ImportDevice> records = (List<ImportDevice>)HttpRuntime.Cache.Get(parseKey);

            if (records == null)
                throw new InvalidOperationException("The session timed out (60 minutes), try importing again");

            this.Status.UpdateStatus(0, "Processing Device Import", "Importing Devices");

            using (DiscoDataContext dbContext = new DiscoDataContext())
            {
                var populateReferences = Import.GetPopulateRecordReferences(dbContext);

                DateTime lastUpdate = DateTime.Now;
                foreach (var record in records)
                {
                    record.ImportRecord(dbContext, populateReferences);

                    if (DateTime.Now.Subtract(lastUpdate).TotalSeconds > 1)
                    {
                        // Update every second
                        this.Status.UpdateStatus((int)Math.Floor((((double)(records.IndexOf(record) + 1) / records.Count) * 100)), string.Format("Importing: {0} ({1} of {2})", record.SerialNumber, records.IndexOf(record) + 1, records.Count));
                        lastUpdate = DateTime.Now;
                    }
                }
            }

        }

        public static ScheduledTaskStatus Run(string ParseTaskSessionKey)
        {
            if (string.IsNullOrWhiteSpace(ParseTaskSessionKey))
                throw new ArgumentNullException("ParseTaskSessionKey");

            var task = new ImportProcessTask();
            JobDataMap taskData = new JobDataMap() { { "ParseKey", ParseTaskSessionKey } };
            return task.ScheduleTask(taskData);
        }
    }
}
