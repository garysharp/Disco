using Disco.Data.Repository;
using Disco.Models.BI.Device;
using Disco.Models.Repository;
using Disco.Services.Tasks;
using LumenWorks.Framework.IO.Csv;
using Quartz;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Caching;

namespace Disco.BI.DeviceBI.Importing
{
    public class ImportParseTask : ScheduledTask
    {
        public override string TaskName { get { return "Import Devices - Parsing"; } }

        public override bool SingleInstanceTask { get { return false; } }
        public override bool CancelInitiallySupported { get { return false; } }

        internal const string ImportParseCacheKey = "ImportParseResults_{0}";

        protected override void ExecuteTask()
        {
            MemoryStream csvStream = (MemoryStream)this.ExecutionContext.JobDetail.JobDataMap["CsvImport"];

            this.Status.UpdateStatus(0, "Parsing CSV File", "Loading Records");

            List<ImportDevice> records;

            using (TextReader csvTextReader = new StreamReader(csvStream))
            {
                using (CsvReader csvReader = new CsvReader(csvTextReader, true))
                {
                    csvReader.DefaultParseErrorAction = ParseErrorAction.ThrowException;
                    csvReader.MissingFieldAction = MissingFieldAction.ReplaceByNull;

                    records = csvReader.Select(record => record.ParseRecord()).ToList();
                }
            }
            csvStream.Dispose();

            this.Status.UpdateStatus(20, "Parsing CSV File", string.Format("Linking {0} Records", records.Count));

            using (DiscoDataContext dbContext = new DiscoDataContext())
            {
                var populateReferences = Import.GetPopulateRecordReferences(dbContext);

                DateTime lastUpdate = DateTime.Now;
                foreach (var record in records)
                {
                    record.PopulateRecord(dbContext, populateReferences);

                    if (DateTime.Now.Subtract(lastUpdate).TotalSeconds > 1)
                    {
                        // Update every second
                        this.Status.UpdateStatus((int)Math.Floor((((double)(records.IndexOf(record) + 1) / records.Count) * 80)));
                        lastUpdate = DateTime.Now;
                    }
                }
            }

            // Set Results to Cache
            string key = string.Format(ImportParseCacheKey, this.Status.SessionId);
            HttpRuntime.Cache.Insert(key, records, null, Cache.NoAbsoluteExpiration, TimeSpan.FromMinutes(60), CacheItemPriority.NotRemovable, null);
        }

        public static ScheduledTaskStatus Run(Stream CsvImport)
        {

            MemoryStream csvStream = new MemoryStream();
            CsvImport.CopyTo(csvStream);
            csvStream.Position = 0;

            var task = new ImportParseTask();
            JobDataMap taskData = new JobDataMap() { { "CsvImport", csvStream } };
            return task.ScheduleTask(taskData);
        }
    }
}
