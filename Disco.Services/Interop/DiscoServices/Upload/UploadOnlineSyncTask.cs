using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Models.Services.Interop.DiscoServices;
using Disco.Services.Tasks;
using Disco.Services.Users;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Quartz;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Disco.Services.Interop.DiscoServices.Upload
{
    public class UploadOnlineSyncTask : ScheduledTask
    {
        private readonly static object runSerialLock = new object();
        private const int connectNotificationType = 1628986937;
        private const string AttachmentHandlerId = "Upload";

        public override string TaskName { get { return "Upload Online - Sync"; } }
        public override bool SingleInstanceTask { get; } = false;

        public static ScheduledTaskStatus RunningStatus
            => ScheduledTasks.GetTaskStatuses(typeof(UploadOnlineSyncTask)).Where(ts => ts.IsRunning).FirstOrDefault();

        public override void InitalizeScheduledTask(DiscoDataContext Database)
        {

            OnlineServicesConnect.SubscribeToNotifications(HandleConnectNotification, connectNotificationType);
        }

        public static ScheduledTaskStatus ScheduleInOneHour()
        {
            var instance = new UploadOnlineSyncTask();
            var trigger = TriggerBuilder.Create()
                .StartAt(DateTimeOffset.Now.AddHours(1));
            return instance.ScheduleTask(trigger);
        }

        private static ScheduledTaskStatus ScheduleNow(string hintFileId)
        {
            var taskState = new JobDataMap() { { "hintFileId", hintFileId } };
            var instance = new UploadOnlineSyncTask();
            return instance.ScheduleTask(taskState);
        }

        private static void HandleConnectNotification(IConnectNotification notification)
        {
            if (notification.Version == 1 && notification.Type == connectNotificationType && Guid.TryParse(notification.Content, out _))
            {
                ScheduleNow(notification.Content);
            }
        }

        protected override void ExecuteTask()
        {
            var hintFileId = (string)(ExecutionContext.JobDetail?.JobDataMap?["hintFileId"]);

            using (var database = new DiscoDataContext())
            {
                if (hintFileId != null && UploadAttachmentExists(database, hintFileId))
                {
                    Status.Finished("Hinted attachment has already been downloaded");
                    return;
                }

                lock (runSerialLock)
                {
                    if (hintFileId != null && UploadAttachmentExists(database, hintFileId))
                    {
                        Status.Finished("Hinted attachment has already been downloaded");
                        return;
                    }

                    var lastFileId = LastUploadFileId(database);

                    Status.UpdateStatus(10, "Fetching attachments from Online Services");
                    var archiveStream = UploadOnlineService.SyncUploads(lastFileId, hintFileId);

                    if (archiveStream == null)
                    {
                        Status.Finished("No new uploads found");
                        return;
                    }

                    using (var archive = new ZipArchive(archiveStream, ZipArchiveMode.Read))
                    {
                        Status.UpdateStatus(45, "Reading manifest");

                        List<SyncUploadModel> manifest;
                        var manifestEntry = archive.GetEntry("manifest.json");
                        using (var manifestStream = manifestEntry.Open())
                        {
                            using (var manifestReader = new StreamReader(manifestStream))
                            {
                                using (var jsonReader = new JsonTextReader(manifestReader))
                                {
                                    var serializerSettings = new JsonSerializerSettings()
                                    {
                                        ContractResolver = new CamelCasePropertyNamesContractResolver(),
                                    };
                                    var serializer = JsonSerializer.Create(serializerSettings);
                                    manifest = serializer.Deserialize<List<SyncUploadModel>>(jsonReader);
                                }
                            }
                        }

                        if (manifest == null || manifest.Count == 0)
                        {
                            Status.Finished("No uploads found in the archive manifest");
                            return;
                        }

                        Status.UpdateStatus(50, $"Importing {manifest.Count} attachments");
                        var attachmentStream = new MemoryStream();
                        foreach (var upload in manifest)
                        {
                            var archiveEntry = archive.GetEntry(upload.Id);
                            if (archiveEntry == null)
                                continue;

                            if (!UserService.TryGetUser(upload.CreatedBy, database, false, out var createdBy))
                                continue;

                            var createdOn = DateTimeOffset.FromUnixTimeMilliseconds(upload.CreatedOn).ToLocalTime().DateTime;

                            using (var uploadStream = archiveEntry.Open())
                            {
                                uploadStream.CopyTo(attachmentStream);
                                attachmentStream.Position = 0;
                                switch (upload.TargetType)
                                {
                                    case AttachmentTypes.Device:
                                        var device = database.Devices.Find(upload.TargetId);
                                        if (device == null)
                                            continue;
                                        if (database.DeviceAttachments.Any(da => da.DeviceSerialNumber == device.SerialNumber && da.HandlerId == AttachmentHandlerId && da.HandlerReferenceId == upload.Id))
                                            continue;
                                        device.CreateAttachment(database, createdBy, upload.FileName, createdOn, upload.MimeType, upload.Comments, attachmentStream, DocumentTemplate: null, PdfThumbnail: null, HandlerId: AttachmentHandlerId, HandlerReferenceId: upload.Id, HandlerData: null);
                                        break;
                                    case AttachmentTypes.Job:
                                        var jobId = int.Parse(upload.TargetId);
                                        var job = database.Jobs.Find(jobId);
                                        if (job == null)
                                            continue;
                                        if (database.JobAttachments.Any(ja => ja.JobId == jobId && ja.HandlerId == AttachmentHandlerId && ja.HandlerReferenceId == upload.Id))
                                            continue;
                                        job.CreateAttachment(database, createdBy, upload.FileName, createdOn, upload.MimeType, upload.Comments, attachmentStream, DocumentTemplate: null, PdfThumbnail: null, HandlerId: AttachmentHandlerId, HandlerReferenceId: upload.Id, HandlerData: null);
                                        break;
                                    case AttachmentTypes.User:
                                        if (UserService.TryGetUser(upload.TargetId, database, true, out var targetUser))
                                        {
                                            if (database.UserAttachments.Any(ua => ua.UserId == targetUser.UserId && ua.HandlerId == AttachmentHandlerId && ua.HandlerReferenceId == upload.Id))
                                                continue;
                                            targetUser.CreateAttachment(database, createdBy, upload.FileName, createdOn, upload.MimeType, upload.Comments, attachmentStream, DocumentTemplate: null, PdfThumbnail: null, HandlerId: AttachmentHandlerId, HandlerReferenceId: upload.Id, HandlerData: null);
                                        }
                                        break;
                                }
                                attachmentStream.SetLength(0);
                            }
                        }

                        Status.Finished("Sync completed successfully");
                    }
                }
            }
        }

        private static bool UploadAttachmentExists(DiscoDataContext database, string fileId)
        {
            return database.JobAttachments
                    .Any(ja => ja.HandlerId == AttachmentHandlerId && ja.HandlerReferenceId == fileId) ||
                database.DeviceAttachments
                    .Any(da => da.HandlerId == AttachmentHandlerId && da.HandlerReferenceId == fileId) ||
                database.UserAttachments
                    .Any(ua => ua.HandlerId == AttachmentHandlerId && ua.HandlerReferenceId == fileId);
        }

        private static string LastUploadFileId(DiscoDataContext database)
        {
            var ids = new List<string>(3)
            {
                database.JobAttachments.Where(ja => ja.HandlerId == AttachmentHandlerId).Max(ja => ja.HandlerReferenceId),
                database.DeviceAttachments.Where(ja => ja.HandlerId == AttachmentHandlerId).Max(ja => ja.HandlerReferenceId),
                database.UserAttachments.Where(ja => ja.HandlerId == AttachmentHandlerId).Max(ja => ja.HandlerReferenceId),
            };
            ids.Sort(StringComparer.Ordinal);
            if (ids[2] == null)
                return null;
            else
                return ids[2];
        }

        private class SyncUploadModel
        {
            public string Id { get; set; }
            public string CreatedBy { get; set; }
            public long CreatedOn { get; set; }
            public AttachmentTypes TargetType { get; set; }
            public string TargetId { get; set; }
            public string FileName { get; set; }
            public string MimeType { get; set; }
            public string Comments { get; set; }
        }
    }
}
