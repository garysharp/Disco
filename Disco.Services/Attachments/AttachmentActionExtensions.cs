using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Services.Authorization;
using Disco.Services.Documents.ManagedGroups;
using Disco.Services.Users;
using System;
using System.Drawing;
using System.IO;
using System.Threading;

namespace Disco.Services
{
    public static class AttachmentActionExtensions
    {
        #region Delete
        public static bool CanDelete(this DeviceAttachment da)
        {
            if (UserService.CurrentAuthorization.Has(Claims.Device.Actions.RemoveAnyAttachments))
                return true;

            if (UserService.CurrentAuthorization.Has(Claims.Device.Actions.RemoveOwnAttachments)
                && da.TechUserId.Equals(UserService.CurrentUserId, StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        }
        public static void OnDelete(this DeviceAttachment da, DiscoDataContext Database)
        {
            if (!da.CanDelete())
                throw new InvalidOperationException("Deletion of Attachment is Denied");

            var attachmentId = da.Id;
            var documentTemplateId = da.DocumentTemplateId;
            var deviceSerialNumber = da.DeviceSerialNumber;

            da.RepositoryDelete(Database);
            Database.DeviceAttachments.Remove(da);

            DocumentTemplateManagedGroups.TriggerDeviceAttachmentDeleted(Database, attachmentId, documentTemplateId, deviceSerialNumber);
        }
        public static bool CanDelete(this DeviceBatchAttachment attachment)
        {
            if (UserService.CurrentAuthorization.Has(Claims.Config.DeviceBatch.Configure))
                return true;

            return false;
        }
        public static void OnDelete(this DeviceBatchAttachment attachment, DiscoDataContext Database)
        {
            if (!attachment.CanDelete())
                throw new InvalidOperationException("Deletion of Attachment is Denied");

            attachment.RepositoryDelete(Database);
            Database.DeviceBatchAttachments.Remove(attachment);
        }
        public static bool CanDelete(this JobAttachment ja)
        {
            if (UserService.CurrentAuthorization.Has(Claims.Job.Actions.RemoveAnyAttachments))
                return true;

            if (UserService.CurrentAuthorization.Has(Claims.Job.Actions.RemoveOwnAttachments)
                && ja.TechUserId.Equals(UserService.CurrentUserId, StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        }
        public static void OnDelete(this JobAttachment ja, DiscoDataContext Database)
        {
            if (!ja.CanDelete())
                throw new InvalidOperationException("Deletion of Attachment is Denied");

            var attachmentId = ja.Id;
            var documentTemplateId = ja.DocumentTemplateId;
            var jobId = ja.JobId;

            ja.RepositoryDelete(Database);
            Database.JobAttachments.Remove(ja);

            DocumentTemplateManagedGroups.TriggerJobAttachmentDeleted(Database, attachmentId, documentTemplateId, jobId);
        }
        public static bool CanDelete(this UserAttachment ua)
        {
            if (UserService.CurrentAuthorization.Has(Claims.User.Actions.RemoveAnyAttachments))
                return true;

            if (UserService.CurrentAuthorization.Has(Claims.User.Actions.RemoveOwnAttachments)
                && ua.TechUserId.Equals(UserService.CurrentUserId, StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        }
        public static void OnDelete(this UserAttachment ua, DiscoDataContext Database)
        {
            if (!ua.CanDelete())
                throw new InvalidOperationException("Deletion of Attachment is Denied");

            var attachmentId = ua.Id;
            var documentTemplateId = ua.DocumentTemplateId;
            var userId = ua.UserId;

            ua.RepositoryDelete(Database);
            Database.UserAttachments.Remove(ua);

            DocumentTemplateManagedGroups.TriggerUserAttachmentDeleted(Database, attachmentId, documentTemplateId, userId);
        }
        #endregion

        public static DeviceAttachment CreateAttachment(this Device Device, DiscoDataContext Database, User CreatorUser, string Filename, string MimeType, string Comments, Stream Content, DocumentTemplate DocumentTemplate = null, Image PdfThumbnail = null)
            => Device.CreateAttachment(Database, CreatorUser, Filename, DateTime.Now, MimeType, Comments, Content, DocumentTemplate, PdfThumbnail);

        public static DeviceAttachment CreateAttachment(this Device Device, DiscoDataContext Database, User CreatorUser, string Filename, DateTime timestamp, string MimeType, string Comments, Stream Content, DocumentTemplate DocumentTemplate = null, Image PdfThumbnail = null, string HandlerId = null, string HandlerReferenceId = null, string HandlerData = null)
        {
            if (string.IsNullOrEmpty(MimeType) || MimeType.Equals("unknown/unknown", StringComparison.OrdinalIgnoreCase))
                MimeType = Interop.MimeTypes.ResolveMimeType(Filename);

            DeviceAttachment da = new DeviceAttachment()
            {
                DeviceSerialNumber = Device.SerialNumber,
                TechUserId = CreatorUser.UserId,
                Filename = Filename,
                MimeType = MimeType,
                Timestamp = timestamp,
                Comments = Comments,
                HandlerId = HandlerId,
                HandlerReferenceId = HandlerReferenceId,
                HandlerData = HandlerData,
            };

            if (DocumentTemplate != null)
                da.DocumentTemplateId = DocumentTemplate.Id;

            Database.DeviceAttachments.Add(da);
            Database.SaveChanges();

            da.SaveAttachment(Database, Content);
            Content.Position = 0;
            if (PdfThumbnail == null)
                da.GenerateThumbnail(Database, Content);
            else
                da.SaveThumbnailAttachment(Database, PdfThumbnail);

            return da;
        }

        public static JobAttachment CreateAttachment(this Job Job, DiscoDataContext Database, User CreatorUser, string Filename, string MimeType, string Comments, Stream Content, DocumentTemplate DocumentTemplate = null, Image PdfThumbnail = null)
            => Job.CreateAttachment(Database, CreatorUser, Filename, DateTime.Now, MimeType, Comments, Content, DocumentTemplate, PdfThumbnail);

        public static JobAttachment CreateAttachment(this Job Job, DiscoDataContext Database, User CreatorUser, string Filename, DateTime Timestamp, string MimeType, string Comments, Stream Content, DocumentTemplate DocumentTemplate = null, Image PdfThumbnail = null, string HandlerId = null, string HandlerReferenceId = null, string HandlerData = null)
        {
            if (string.IsNullOrEmpty(MimeType) || MimeType.Equals("unknown/unknown", StringComparison.OrdinalIgnoreCase))
                MimeType = Interop.MimeTypes.ResolveMimeType(Filename);

            JobAttachment ja = new JobAttachment()
            {
                JobId = Job.Id,
                TechUserId = CreatorUser.UserId,
                Filename = Filename,
                MimeType = MimeType,
                Timestamp = Timestamp,
                Comments = Comments,
                HandlerId = HandlerId,
                HandlerReferenceId = HandlerReferenceId,
                HandlerData = HandlerData,
            };

            if (DocumentTemplate != null)
                ja.DocumentTemplateId = DocumentTemplate.Id;

            Database.JobAttachments.Add(ja);
            Database.SaveChanges();

            ja.SaveAttachment(Database, Content);
            Content.Position = 0;
            if (PdfThumbnail == null)
                ja.GenerateThumbnail(Database, Content);
            else
                ja.SaveThumbnailAttachment(Database, PdfThumbnail);

            return ja;
        }

        public static UserAttachment CreateAttachment(this User User, DiscoDataContext Database, User CreatorUser, string Filename, string MimeType, string Comments, Stream Content, DocumentTemplate DocumentTemplate = null, Image PdfThumbnail = null)
            => User.CreateAttachment(Database, CreatorUser, Filename, DateTime.Now, MimeType, Comments, Content, DocumentTemplate, PdfThumbnail);

        public static UserAttachment CreateAttachment(this User User, DiscoDataContext Database, User CreatorUser, string Filename, DateTime Timestamp, string MimeType, string Comments, Stream Content, DocumentTemplate DocumentTemplate = null, Image PdfThumbnail = null, string HandlerId = null, string HandlerReferenceId = null, string HandlerData = null)
        {
            if (string.IsNullOrEmpty(MimeType) || MimeType.Equals("unknown/unknown", StringComparison.OrdinalIgnoreCase))
                MimeType = Interop.MimeTypes.ResolveMimeType(Filename);

            UserAttachment ua = new UserAttachment()
            {
                UserId = User.UserId,
                TechUserId = CreatorUser.UserId,
                Filename = Filename,
                MimeType = MimeType,
                Timestamp = Timestamp,
                Comments = Comments,
                HandlerId = HandlerId,
                HandlerReferenceId = HandlerReferenceId,
                HandlerData = HandlerData,
            };

            if (DocumentTemplate != null)
                ua.DocumentTemplateId = DocumentTemplate.Id;

            Database.UserAttachments.Add(ua);
            Database.SaveChanges();

            ua.SaveAttachment(Database, Content);
            Content.Position = 0;
            if (PdfThumbnail == null)
                ua.GenerateThumbnail(Database, Content);
            else
                ua.SaveThumbnailAttachment(Database, PdfThumbnail);

            return ua;
        }

        public static bool WaitForThumbnailGeneration(this IAttachment attachment, DiscoDataContext database, out string thumbnailPath, out string mimeType)
        {
            thumbnailPath = attachment.RepositoryThumbnailFilename(database);
            if (thumbnailPath.EndsWith(".png"))
                mimeType = "image/png";
            else
                mimeType = "image/jpeg";

            if (File.Exists(thumbnailPath))
                return true;

            // recently created attachments may not have a thumbnail yet
            var timestamp = attachment.Timestamp;
            if (timestamp > DateTime.Now.AddSeconds(-5) && attachment.SupportsThumbnailGeneration(out _, out _))
            {
                while (!File.Exists(thumbnailPath) && timestamp > DateTime.Now.AddSeconds(-5))
                    Thread.Sleep(250);

                if (File.Exists(thumbnailPath))
                    return true;
            }

            return false;
        }

        public static string GenerateThumbnail(this IAttachment attachment, DiscoDataContext Database, Stream AttachmentStream)
        {
            string thumbnailFilePath = attachment.RepositoryThumbnailFilename(Database);

            Image thumbnail;
            if (attachment.GenerateThumbnail(AttachmentStream, out thumbnail))
            {
                thumbnail.SaveJpg(90, thumbnailFilePath);
            }

            return thumbnailFilePath;
        }

        public static string GenerateThumbnail(this IAttachment attachment, DiscoDataContext Database)
        {
            string thumbnailFilePath = attachment.RepositoryThumbnailFilename(Database);

            using (var attachmentStream = File.OpenRead(attachment.RepositoryFilename(Database)))
            {
                Image thumbnail;
                if (attachment.GenerateThumbnail(attachmentStream, out thumbnail))
                {
                    thumbnail.SaveJpg(90, thumbnailFilePath);
                }
            }

            return thumbnailFilePath;
        }


        private const string pdfMimeType = "application/pdf";
        private const string pdfExtension = "pdf";
        private static readonly string[] imageMimeTypes = new string[] { "image/jpeg", "image/png", "image/gif", "image/bmp" };
        private static readonly string[] imageExtensions = new string[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
        public static (bool supported, bool isImage, bool isPdf) SupportsThumbnailGeneration(string mimeType, string fileName)
        {
            if (!string.IsNullOrEmpty(mimeType))
            {
                if (pdfMimeType.Equals(mimeType, StringComparison.OrdinalIgnoreCase))
                    return (true, false, true);
                foreach (var imageMimeType in imageMimeTypes)
                    if (mimeType.Equals(imageMimeType, StringComparison.OrdinalIgnoreCase))
                        return (true, true, false);
            }

            if (!string.IsNullOrEmpty(fileName))
            {
                if (fileName.EndsWith(pdfExtension, StringComparison.OrdinalIgnoreCase))
                    return (true, false, true);
                foreach (var imageExtension in imageExtensions)
                    if (fileName.EndsWith(imageExtension, StringComparison.OrdinalIgnoreCase))
                        return (true, true, false);
            }

            return (false, false, false);
        }

        public static bool SupportsThumbnailGeneration(this IAttachment attachment, out bool isImage, out bool isPdf)
        {
            var result = SupportsThumbnailGeneration(attachment.MimeType, attachment.Filename);

            isImage = result.isImage;
            isPdf = result.isPdf;

            return result.supported;
        }

        public static bool GenerateThumbnail(this IAttachment attachment, Stream source, out Image thumbnail)
        {
            if (source != null)
            {
                var (supported, isImage, isPdf) = SupportsThumbnailGeneration(attachment.MimeType, attachment.Filename);

                if (supported)
                {
                    // GDI+ (jpg, png, gif, bmp)
                    if (isImage)
                    {
                        try
                        {
                            using (Image sourceImage = Image.FromStream(source))
                            {
                                thumbnail = sourceImage.ResizeImage(48, 48, Brushes.Black);
                                using (Image mimeTypeIcon = Properties.Resources.MimeType_img16)
                                {
                                    thumbnail.EmbedIconOverlay(mimeTypeIcon);
                                }
                                return true;
                            }
                        }
                        catch (Exception) { }

                    }

                    // PDF
                    if (isPdf)
                    {
                        try
                        {
                            using (var pdfiumDocument = PdfiumViewer.PdfDocument.Load(source))
                            {

                                if (pdfiumDocument.PageCount > 0)
                                {
                                    var pageSize = pdfiumDocument.PageSizes[0];
                                    var size = ImagingExtensions.CalculateResize((int)pageSize.Width, (int)pageSize.Height, 48, 48);

                                    using (var sourceImage = pdfiumDocument.Render(0, (int)size.Width, (int)size.Height, 72, 72, true))
                                    {
                                        thumbnail = sourceImage.ResizeImage(48, 48, Brushes.White);
                                        using (Image mimeTypeIcon = Properties.Resources.MimeType_pdf16)
                                        {
                                            thumbnail.EmbedIconOverlay(mimeTypeIcon);
                                        }
                                        return true;
                                    }
                                }
                            }
                        }
                        catch (Exception) { }
                    }
                }
            }
            thumbnail = null;
            return false;
        }
    }
}
