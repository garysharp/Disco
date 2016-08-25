using Disco.Data.Repository;
using Disco.Models.Repository;
using Exceptionless;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Services
{
    public static class AttachmentActionExtensions
    {
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
                            Thumbnail = sourceImage.ResizeImage(48, 48);
                            using (Image mimeTypeIcon = Disco.Services.Properties.Resources.MimeType_img16)
                            {
                                Thumbnail.EmbedIconOverlay(mimeTypeIcon);
                            }
                            return true;
                        }
                    }
                    catch (Exception ex)
                    {
                        ex.ToExceptionless().Submit();
                    }

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

                                Thumbnail = pdfiumDocument.Render(0, (int)size.Width, (int)size.Height, 72, 72, true);
                                return true;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ex.ToExceptionless().Submit();
                    }
                }
            }
            Thumbnail = null;
            return false;
        }
    }
}
