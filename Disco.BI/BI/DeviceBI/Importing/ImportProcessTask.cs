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
            string importParseTaskId = (string)this.ExecutionContext.JobDetail.JobDataMap["ImportParseTaskId"];

            if (string.IsNullOrWhiteSpace(importParseTaskId))
                throw new ArgumentNullException("ImportParseTaskId");

            ImportDeviceSession session = Import.GetSession(importParseTaskId);

            if (session == null)
                throw new InvalidOperationException("The session timed out (60 minutes), try importing again");

            List<ImportDevice> records = session.ImportDevices;
            int recordsImported = 0;

            this.Status.UpdateStatus(0, "Processing Device Import", "Importing Devices");

            using (DiscoDataContext dbContext = new DiscoDataContext())
            {
                var populateReferences = Import.GetPopulateRecordReferences(dbContext);

                DateTime lastUpdate = DateTime.Now;
                foreach (var record in records)
                {
                    if (record.ImportRecord(dbContext, populateReferences))
                        recordsImported++;

                    if (DateTime.Now.Subtract(lastUpdate).TotalSeconds > 1)
                    {
                        // Update every second
                        this.Status.UpdateStatus((int)Math.Floor((((double)(records.IndexOf(record) + 1) / records.Count) * 100)), string.Format("Importing: {0} ({1} of {2})", record.SerialNumber, records.IndexOf(record) + 1, records.Count));
                        lastUpdate = DateTime.Now;
                    }
                }
            }

            this.Status.SetFinishedMessage(string.Format("Imported {0} of {1} Devices", recordsImported, records.Count));
        }

        public static ScheduledTaskStatus Run(string ImportParseTaskId)
        {
            if (string.IsNullOrWhiteSpace(ImportParseTaskId))
                throw new ArgumentNullException("ImportParseTaskId");

            var task = new ImportProcessTask();
            JobDataMap taskData = new JobDataMap() { { "ImportParseTaskId", ImportParseTaskId } };
            return task.ScheduleTask(taskData);
        }
    }
}
