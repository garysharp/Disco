using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Services.Tasks;
using Quartz;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Disco.Services.Documents.AttachmentImport
{
    public class ThumbnailUpdateTask : ScheduledTask
    {
        public override string TaskName { get { return "Migration: PDF Attachment Thumbnail Update"; } }

        public override bool SingleInstanceTask { get { return true; } }
        public override bool CancelInitiallySupported { get { return false; } }

        #region Required Helpers
        private static string RequiredFilePath(DiscoDataContext Database)
        {
            if (Database.DiscoConfiguration.DataStoreLocation != null)
                return Path.Combine(Database.DiscoConfiguration.DataStoreLocation, "_ThumbnailUpdateRequired.txt");
            else
                return null;
        }

        public static bool IsRequired(DiscoDataContext Database)
        {
            var requiredFilePath = RequiredFilePath(Database);

            if (requiredFilePath == null)
                return false;
            else
                return File.Exists(requiredFilePath);
        }
        public static void SetRequired(DiscoDataContext Database)
        {
            var requiredFilePath = RequiredFilePath(Database);

            if (requiredFilePath != null)
            {
                File.WriteAllText(requiredFilePath, "This file exists to indicate a Thumbnail Update is required. It will automatically be deleted when the update completes.");
                File.SetAttributes(requiredFilePath, FileAttributes.Hidden);
            }
        }
        #endregion

        public override void InitalizeScheduledTask(DiscoDataContext Database)
        {
            if (IsRequired(Database))
            {
                // Schedule in 5mins
                var trigger = TriggerBuilder.Create()
                    .StartAt(DateTimeOffset.Now.AddMinutes(5));

                this.ScheduleTask(trigger);
            }
        }

        public static ScheduledTaskStatus ScheduleImmediately()
        {
            var existingTask = ScheduledTasks.GetTaskStatuses(typeof(ThumbnailUpdateTask)).Where(s => s.IsRunning).FirstOrDefault();
            if (existingTask != null)
                return existingTask;

            var instance = new ThumbnailUpdateTask();
            return instance.ScheduleTask();
        }

        protected override void ExecuteTask()
        {
            using (DiscoDataContext database = new DiscoDataContext())
            {
                Status.UpdateStatus(0, "Updating Attachment PDF Thumbnails", "Reading Attachments");

                // Attachments
                List<IAttachment> attachments = database
                    .DeviceAttachments.Where(da => da.MimeType == "application/pdf").ToList().Cast<IAttachment>()
                    .Concat(database.UserAttachments.Where(da => da.MimeType == "application/pdf").ToList().Cast<IAttachment>())
                    .Concat(database.JobAttachments.Where(da => da.MimeType == "application/pdf").ToList().Cast<IAttachment>())
                    .ToList();

                int failedTotal = 0;
                int notFoundTotal = 0;
                int completedTotal = 0;

                attachments.AsParallel().ForAll(a =>
                {
                    var completed = Interlocked.Increment(ref completedTotal);
                    if ((completed % 10) == 0)
                    {
                        Status.UpdateStatus((100D / attachments.Count) * completed, $"Processing: {a.AttachmentType} Attachment, Id: {a.Id}");
                    }
                    try
                    {
                        var fileName = a.RepositoryFilename(database);
                        if (!File.Exists(fileName))
                        {
                            Interlocked.Increment(ref notFoundTotal);
                            Status.LogWarning($"Attachment not found in the Data Store. [{a.AttachmentType} Attachment, Id {a.Id}] expected at: '{fileName}'");
                        }
                        else
                        {
                            a.GenerateThumbnail(database);
                        }
                    }
                    catch (Exception ex)
                    {
                        Interlocked.Increment(ref failedTotal);
                        Status.LogWarning($"Error when generating thumbnail for [{a.AttachmentType} Attachment, Id {a.Id}]: [{ex.GetType().Name}] {ex.Message}");
                    }
                });

                // Finished - Remove Placeholder File
                var requiredFilePath = RequiredFilePath(database);
                if (requiredFilePath != null && File.Exists(requiredFilePath))
                    File.Delete(requiredFilePath);

                Status.SetFinishedMessage($"Finished updating thumbnails for {attachments.Count:N0}. {notFoundTotal:N0} were not found. {failedTotal:N0} failed.");
                Status.LogInformation(Status.FinishedMessage);
                Status.Finished();
            }
        }

    }
}
