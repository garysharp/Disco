using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Services.Tasks;
using Quartz;
using System;
using System.Collections.Generic;
using System.IO;

namespace Disco.Services.Documents
{
    public class DocumentBulkGenerateTask : ScheduledTask
    {
        private const string JobDataMapContext = "Context";

        public override string TaskName { get; } = "Document Template - Bulk Generate";
        public override bool SingleInstanceTask { get; } = false;
        public override bool CancelInitiallySupported { get; } = false;

        public override void InitalizeScheduledTask(DiscoDataContext Database)
        {
            // clear cache
            var cachePath = GetCachePath(Database);

            if (Directory.Exists(cachePath))
                Directory.Delete(cachePath, true);
        }

        public static Stream GetCached(DiscoDataContext database, string id)
        {
            var cachePath = GetCachePath(database);
            var path = Path.Combine(cachePath, $"{id}.pdf");
            if (File.Exists(path))
                return File.OpenRead(path);
            else
                throw new FileNotFoundException();
        }

        public static ScheduledTaskStatus ScheduleNow(Func<DocumentTemplate, DiscoDataContext, User, DateTime, bool, List<string>, IScheduledTaskStatus, Stream> generateDelegate, DocumentTemplate documentTemplate, User creatorUser, DateTime timestamp, bool insertBlankPages, List<string> dataObjectsIds)
        {
            var context = new DocumentBulkGenerateContext()
            {
                GenerateDelegate = generateDelegate,
                DocumentTemplate = documentTemplate,
                CreatorUser = creatorUser,
                Timestamp = timestamp,
                InsertBlankPages = insertBlankPages,
                DataObjectsIds = dataObjectsIds,
            };

            // Build Data Map
            var task = new DocumentBulkGenerateTask();
            JobDataMap taskData = new JobDataMap() { { JobDataMapContext, context } };

            // Schedule Task
            var status = task.ScheduleTask(taskData);
            context.Id = status.SessionId;
            return status;
        }

        protected override void ExecuteTask()
        {
            var context = (DocumentBulkGenerateContext)ExecutionContext.JobDetail.JobDataMap[JobDataMapContext];

            using (var database = new DiscoDataContext())
            {
                var cachePath = GetCachePath(database);
                if (!Directory.Exists(cachePath))
                    Directory.CreateDirectory(cachePath);

                var filePath = Path.Combine(cachePath, $"{Status.SessionId}.pdf");

                var stream = context.GenerateDelegate(context.DocumentTemplate, database, context.CreatorUser, context.Timestamp, context.InsertBlankPages, context.DataObjectsIds, Status);

                database.SaveChanges();

                using (var cacheStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
                    stream.CopyTo(cacheStream);

            }
            Status.UpdateStatus(100);
            Status.Finished("Generated Document");
        }

        private static string GetCachePath(DiscoDataContext database)
            => Path.Combine(database.DiscoConfiguration.DataStoreLocation, @"DocumentTemplates\BulkGenerateCache");

        private class DocumentBulkGenerateContext
        {
            public Func<DocumentTemplate, DiscoDataContext, User, DateTime, bool, List<string>, IScheduledTaskStatus, Stream> GenerateDelegate { get; set; }
            public string Id { get; set; }
            public DocumentTemplate DocumentTemplate { get; set; }
            public User CreatorUser { get; set; }
            public DateTime Timestamp { get; set; }
            public bool InsertBlankPages { get; set; }
            public List<string> DataObjectsIds { get; set; }
        }
    }
}
