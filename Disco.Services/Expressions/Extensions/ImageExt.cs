using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Services.Expressions.Extensions.ImageResultImplementations;
using System;
using System.Collections;
using System.Drawing;
using System.IO;
using System.Linq;

namespace Disco.Services.Expressions.Extensions
{
    public static class ImageExt
    {
        public static FileImageExpressionResult ImageFromFile(string AbsoluteFilePath)
        {
            return new FileImageExpressionResult(AbsoluteFilePath);
        }
        public static FileImageExpressionResult ImageFromDataStoreFile(string RelativeFilePath)
        {
            var configCache = new Data.Configuration.SystemConfiguration(null);
            string DataStoreLocation = configCache.DataStoreLocation;
            string AbsoluteFilePath = System.IO.Path.Combine(DataStoreLocation, RelativeFilePath);
            return new FileImageExpressionResult(AbsoluteFilePath);
        }
        public static FileImageExpressionResult JobAttachmentFirstImage(Job Job, DiscoDataContext Database)
        {
            var attachment = Job.JobAttachments.FirstOrDefault(ja => ja.MimeType.StartsWith("image/", StringComparison.OrdinalIgnoreCase));
            if (attachment != null)
            {
                var filename = attachment.RepositoryFilename(Database);
                return new FileImageExpressionResult(filename);
            }
            else
                return null;
        }
        public static FileImageExpressionResult JobAttachmentLastImage(Job Job, DiscoDataContext Database)
        {
            var attachment = Job.JobAttachments.LastOrDefault(ja => ja.MimeType.StartsWith("image/", StringComparison.OrdinalIgnoreCase));
            if (attachment != null)
            {
                var filename = attachment.RepositoryFilename(Database);
                return new FileImageExpressionResult(filename);
            }
            else
                return null;
        }
        public static FileImageExpressionResult JobAttachmentImage(JobAttachment JobAttachment, DiscoDataContext Database)
        {
            if (JobAttachment == null)
                throw new ArgumentNullException("JobAttachment");
            if (!JobAttachment.MimeType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Invalid Image MimeType for Attachment");

            var filename = JobAttachment.RepositoryFilename(Database);
            return new FileImageExpressionResult(filename);
        }
        public static FileMontageImageExpressionResult JobAttachmentImageMontage(Job Job, DiscoDataContext Database)
        {
            if (Job == null)
                throw new ArgumentNullException("Job");
            if (Job.JobAttachments == null)
                throw new ArgumentException("Job.JobAttachments is null", "Job");

            var attachments = Job.JobAttachments.Where(a => a.MimeType.StartsWith("image/", StringComparison.OrdinalIgnoreCase)).ToList();

            if (attachments.Count > 0)
            {
                var attachmentFilepaths = attachments.Select(a => a.RepositoryFilename(Database)).ToList();

                return new FileMontageImageExpressionResult(attachmentFilepaths);
            }
            else
                return null;
        }
        public static FileMontageImageExpressionResult JobAttachmentsImageMontage(ArrayList JobAttachments, DiscoDataContext Database)
        {
            if (JobAttachments == null)
                throw new ArgumentNullException("JobAttachments");

            var attachments = JobAttachments.Cast<JobAttachment>().Where(a => a.MimeType.StartsWith("image/", StringComparison.OrdinalIgnoreCase)).ToList();

            if (attachments.Count > 0)
            {
                var attachmentFilepaths = attachments.Select(a => a.RepositoryFilename(Database)).ToList();

                return new FileMontageImageExpressionResult(attachmentFilepaths);
            }
            else
                return null;
        }

        public static BitmapImageExpressionResult ImageFromStream(Stream ImageStream)
        {
            if (ImageStream == null)
                throw new ArgumentNullException("ImageStream");

            return new BitmapImageExpressionResult(Bitmap.FromStream(ImageStream));
        }
        public static BitmapImageExpressionResult ImageFromByteArray(byte[] ImageByteArray)
        {
            if (ImageByteArray == null)
                throw new ArgumentNullException("ImageByteArray");

            return new BitmapImageExpressionResult(Bitmap.FromStream(new MemoryStream(ImageByteArray)));
        }
        public static BitmapImageExpressionResult DeviceModelImage(DeviceModel DeviceModel)
        {
            if (DeviceModel == null)
                throw new ArgumentNullException("DeviceModel");

            using (Stream deviceModelImage = DeviceModel.Image())
            {
                if (deviceModelImage == null)
                    return null;
                else
                    return ImageFromStream(deviceModelImage);
            }
        }
        public static BitmapImageExpressionResult OrganisationLogo()
        {
            var configCache = new Data.Configuration.SystemConfiguration(null);
            BitmapImageExpressionResult result;
            using (var orgLogo = configCache.OrganisationLogo)
            {
                result = ImageFromStream(orgLogo);
            }
            result.Format = Models.Services.Expressions.Extensions.ImageExpressionFormat.Png;
            return result;
        }

        public static QrCodeImageExpressionResult QrCode(string content)
        {
            return new QrCodeImageExpressionResult(content, 'M');
        }

        public static QrCodeImageExpressionResult QrCode(string content, string errorCorrectionLevel)
        {
            if (string.IsNullOrWhiteSpace(errorCorrectionLevel))
                errorCorrectionLevel = "M";

            return new QrCodeImageExpressionResult(content, errorCorrectionLevel[0]);
        }

    }
}
