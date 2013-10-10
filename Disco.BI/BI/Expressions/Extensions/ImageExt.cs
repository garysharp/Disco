using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Disco.Models.BI.Expressions;
using Disco.BI.Expressions.Extensions.ImageResultImplementations;
using Disco.Models.Repository;
using Disco.BI.Extensions;
using Disco.Data.Repository;
using System.Collections;
using System.IO;
using System.Drawing;

namespace Disco.BI.Expressions.Extensions
{
    public static class ImageExt
    {
        public static FileImageExpressionResult ImageFromFile(string AbsoluteFilePath)
        {
            return new FileImageExpressionResult(AbsoluteFilePath);
        }
        public static FileImageExpressionResult ImageFromDataStoreFile(string RelativeFilePath)
        {
            var configCache = new Disco.Data.Configuration.SystemConfiguration(null);
            string DataStoreLocation = configCache.DataStoreLocation;
            string AbsoluteFilePath = System.IO.Path.Combine(DataStoreLocation, RelativeFilePath);
            return new FileImageExpressionResult(AbsoluteFilePath);
        }
        public static FileImageExpressionResult JobAttachmentFirstImage(Job Job, DiscoDataContext Database)
        {
            var attachment = Job.JobAttachments.FirstOrDefault(ja => ja.MimeType.StartsWith("image/", StringComparison.InvariantCultureIgnoreCase));
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
            var attachment = Job.JobAttachments.LastOrDefault(ja => ja.MimeType.StartsWith("image/", StringComparison.InvariantCultureIgnoreCase));
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
            if (!JobAttachment.MimeType.StartsWith("image/", StringComparison.InvariantCultureIgnoreCase))
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

            var attachments = Job.JobAttachments.Where(a => a.MimeType.StartsWith("image/", StringComparison.InvariantCultureIgnoreCase)).ToList();

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

            var attachments = JobAttachments.Cast<JobAttachment>().Where(a => a.MimeType.StartsWith("image/", StringComparison.InvariantCultureIgnoreCase)).ToList();

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
            //if (DeviceModel.Image == null || DeviceModel.Image.Length == 0)
            //    return null;

            //return ImageFromByteArray(DeviceModel.Image);
        }
        public static BitmapImageExpressionResult OrganisationLogo()
        {
            var configCache = new Disco.Data.Configuration.SystemConfiguration(null);
            BitmapImageExpressionResult result;
            using (var orgLogo = configCache.OrganisationLogo)
            {
                result = ImageFromStream(orgLogo);
            }
            result.LosslessFormat = true;
            return result;
        }

    }
}
