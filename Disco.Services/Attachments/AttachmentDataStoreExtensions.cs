using Disco.Data.Repository;
using Disco.Models.Repository;
using System;
using System.Drawing;
using System.IO;

namespace Disco.Services
{
    public static class AttachmentDataStoreExtensions
    {

        public static string RepositoryFilename(this IAttachment attachment, DiscoDataContext database)
        {
            switch (attachment.AttachmentType)
            {
                case AttachmentTypes.Device:
                    return Path.Combine(DataStore.CreateLocation(database, "DeviceAttachments", attachment.Timestamp),
                        $"{attachment.Reference}_{attachment.Id}_file");
                case AttachmentTypes.DeviceBatch:
                    return Path.Combine(DataStore.CreateLocation(database, "DeviceBatchAttachments", attachment.Timestamp),
                        $"{attachment.Reference}_{attachment.Id}_file");
                case AttachmentTypes.Job:
                    return Path.Combine(DataStore.CreateLocation(database, "JobAttachments", attachment.Timestamp),
                        $"{attachment.Reference}_{attachment.Id}_file");
                case AttachmentTypes.User:
                    return Path.Combine(DataStore.CreateLocation(database, "UserAttachments", attachment.Timestamp),
                        $"{((string)attachment.Reference).Replace('\\', '_')}_{attachment.Id}_file");
                default:
                    throw new ArgumentException("Unknown Attachment Type", nameof(attachment));
            }
        }

        public static string RepositoryThumbnailFilename(this IAttachment attachment, DiscoDataContext database)
        {
            switch (attachment.AttachmentType)
            {
                case AttachmentTypes.Device:
                    return Path.Combine(DataStore.CreateLocation(database, "DeviceAttachments", attachment.Timestamp),
                        $"{attachment.Reference}_{attachment.Id}_thumb.jpg");
                case AttachmentTypes.DeviceBatch:
                    return Path.Combine(DataStore.CreateLocation(database, "DeviceBatchAttachments", attachment.Timestamp),
                        $"{attachment.Reference}_{attachment.Id}_thumb.jpg");
                case AttachmentTypes.Job:
                    return Path.Combine(DataStore.CreateLocation(database, "JobAttachments", attachment.Timestamp),
                        $"{attachment.Reference}_{attachment.Id}_thumb.jpg");
                case AttachmentTypes.User:
                    return Path.Combine(DataStore.CreateLocation(database, "UserAttachments", attachment.Timestamp),
                        $"{((string)attachment.Reference).Replace('\\', '_')}_{attachment.Id}_thumb.jpg");
                default:
                    throw new ArgumentException("Unknown Attachment Type", nameof(attachment));
            }
        }

        public static string SaveAttachment(this IAttachment Attachment, DiscoDataContext Database, Stream FileContent)
        {
            string filePath = Attachment.RepositoryFilename(Database);
            DataStore.WriteFile(filePath, FileContent);
            return filePath;
        }

        public static string SaveAttachment(this IAttachment Attachment, DiscoDataContext Database, byte[] FileContent)
        {
            string filePath = Attachment.RepositoryFilename(Database);
            DataStore.WriteFile(filePath, FileContent);
            return filePath;
        }

        public static string SaveThumbnailAttachment(this IAttachment Attachment, DiscoDataContext Database, Image Thumbnail)
        {
            string filePath = Attachment.RepositoryThumbnailFilename(Database);
            Thumbnail.SaveJpg(90, filePath);
            return filePath;
        }

        public static string SaveThumbnailAttachment(this IAttachment Attachment, DiscoDataContext Database, Stream FileContent)
        {
            string filePath = Attachment.RepositoryThumbnailFilename(Database);
            DataStore.WriteFile(filePath, FileContent);
            return filePath;
        }

        public static string SaveThumbnailAttachment(this IAttachment Attachment, DiscoDataContext Database, byte[] FileContent)
        {
            string filePath = Attachment.RepositoryThumbnailFilename(Database);
            DataStore.WriteFile(filePath, FileContent);
            return filePath;
        }

        public static void RepositoryDelete(this IAttachment Attachment, DiscoDataContext Database)
        {
            DataStore.DeleteFiles(Attachment.RepositoryFilename(Database), Attachment.RepositoryThumbnailFilename(Database));
        }

    }
}
