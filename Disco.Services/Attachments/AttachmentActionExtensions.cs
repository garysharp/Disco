using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Services.Authorization;
using Disco.Services.Documents.ManagedGroups;
using Disco.Services.Users;
using System;
using System.Drawing;
using System.IO;

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
        {
            if (string.IsNullOrEmpty(MimeType) || MimeType.Equals("unknown/unknown", StringComparison.OrdinalIgnoreCase))
                MimeType = Interop.MimeTypes.ResolveMimeType(Filename);

            DeviceAttachment da = new DeviceAttachment()
            {
                DeviceSerialNumber = Device.SerialNumber,
                TechUserId = CreatorUser.UserId,
                Filename = Filename,
                MimeType = MimeType,
                Timestamp = DateTime.Now,
                Comments = Comments
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
        {
            if (string.IsNullOrEmpty(MimeType) || MimeType.Equals("unknown/unknown", StringComparison.OrdinalIgnoreCase))
                MimeType = Interop.MimeTypes.ResolveMimeType(Filename);

            JobAttachment ja = new JobAttachment()
            {
                JobId = Job.Id,
                TechUserId = CreatorUser.UserId,
                Filename = Filename,
                MimeType = MimeType,
                Timestamp = DateTime.Now,
                Comments = Comments
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
        {
            if (string.IsNullOrEmpty(MimeType) || MimeType.Equals("unknown/unknown", StringComparison.OrdinalIgnoreCase))
                MimeType = Interop.MimeTypes.ResolveMimeType(Filename);

            UserAttachment ua = new UserAttachment()
            {
                UserId = User.UserId,
                TechUserId = CreatorUser.UserId,
                Filename = Filename,
                MimeType = MimeType,
                Timestamp = DateTime.Now,
                Comments = Comments
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

        public static string GenerateThumbnail(this IAttachment attachment, DiscoDataContext Database, Stream AttachmentStream)
        {
            string thumbnailFilePath = attachment.RepositoryThumbnailFilename(Database);

            Image thumbnail;
            if (GenerateThumbnail(AttachmentStream, attachment.MimeType, out thumbnail))
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
                if (GenerateThumbnail(attachmentStream, attachment.MimeType, out thumbnail))
                {
                    thumbnail.SaveJpg(90, thumbnailFilePath);
                }
            }

            return thumbnailFilePath;
        }

        public static bool GenerateThumbnail(Stream Source, string SourceMimeType, out Image Thumbnail)
        {
            if (Source != null)
            {
                // GDI+ (jpg, png, gif, bmp)
                if (SourceMimeType.Equals("image/jpeg", StringComparison.OrdinalIgnoreCase) || SourceMimeType.Contains("jpg") ||
                    SourceMimeType.Equals("image/png", StringComparison.OrdinalIgnoreCase) || SourceMimeType.Contains("png") ||
                    SourceMimeType.Equals("image/gif", StringComparison.OrdinalIgnoreCase) || SourceMimeType.Contains("gif") ||
                    SourceMimeType.Equals("image/bmp", StringComparison.OrdinalIgnoreCase) || SourceMimeType.Contains("bmp"))
                {
                    try
                    {
                        using (Image sourceImage = Image.FromStream(Source))
                        {
                            Thumbnail = sourceImage.ResizeImage(48, 48, Brushes.Black);
                            using (Image mimeTypeIcon = Properties.Resources.MimeType_img16)
                            {
                                Thumbnail.EmbedIconOverlay(mimeTypeIcon);
                            }
                            return true;
                        }
                    }
                    catch (Exception) { }

                }

                // PDF
                if (SourceMimeType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase) || SourceMimeType.Contains("pdf"))
                {
                    try
                    {
                        using (var pdfiumDocument = PdfiumViewer.PdfDocument.Load(Source))
                        {

                            if (pdfiumDocument.PageCount > 0)
                            {
                                var pageSize = pdfiumDocument.PageSizes[0];
                                var size = ImagingExtensions.CalculateResize((int)pageSize.Width, (int)pageSize.Height, 48, 48);

                                using (var sourceImage = pdfiumDocument.Render(0, (int)size.Width, (int)size.Height, 72, 72, true))
                                {
                                    Thumbnail = sourceImage.ResizeImage(48, 48, Brushes.White);
                                    using (Image mimeTypeIcon = Properties.Resources.MimeType_pdf16)
                                    {
                                        Thumbnail.EmbedIconOverlay(mimeTypeIcon);
                                    }
                                    return true;
                                }
                            }
                        }
                    }
                    catch (Exception) { }
                }
            }
            Thumbnail = null;
            return false;
        }
    }
}
