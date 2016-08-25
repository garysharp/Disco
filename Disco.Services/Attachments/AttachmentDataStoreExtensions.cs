using Disco.Data.Repository;
using Disco.Models.Repository;
using System;
using System.Drawing;
using System.IO;

namespace Disco.Services
{
    public static class AttachmentDataStoreExtensions
    {

        public static string RepositoryFilename(this IAttachment Attachment, DiscoDataContext Database)
        {
            switch (Attachment.AttachmentType)
            {
                case AttachmentTypes.Device:
                    return Path.Combine(DataStore.CreateLocation(Database, "DeviceAttachments", Attachment.Timestamp),
                        string.Format("{0}_{1}_file", Attachment.Reference, Attachment.Id));
                case AttachmentTypes.Job:
                    return Path.Combine(DataStore.CreateLocation(Database, "JobAttachments", Attachment.Timestamp),
                        string.Format("{0}_{1}_file", Attachment.Reference, Attachment.Id));
                case AttachmentTypes.User:
                    return Path.Combine(DataStore.CreateLocation(Database, "UserAttachments", Attachment.Timestamp),
                        string.Format("{0}_{1}_file", ((string)Attachment.Reference).Replace('\\', '_'), Attachment.Id));
                default:
                    throw new ArgumentException("Unknown Attachment Type", nameof(Attachment));
            }
        }

        public static string RepositoryThumbnailFilename(this IAttachment Attachment, DiscoDataContext Database)
        {
            switch (Attachment.AttachmentType)
            {
                case AttachmentTypes.Device:
                    return Path.Combine(DataStore.CreateLocation(Database, "DeviceAttachments", Attachment.Timestamp),
                        string.Format("{0}_{1}_thumb.jpg", Attachment.Reference, Attachment.Id));
                case AttachmentTypes.Job:
                    return Path.Combine(DataStore.CreateLocation(Database, "JobAttachments", Attachment.Timestamp),
                        string.Format("{0}_{1}_thumb.jpg", Attachment.Reference, Attachment.Id));
                case AttachmentTypes.User:
                    return Path.Combine(DataStore.CreateLocation(Database, "UserAttachments", Attachment.Timestamp),
                        string.Format("{0}_{1}_thumb.jpg", ((string)Attachment.Reference).Replace('\\', '_'), Attachment.Id));
                default:
                    throw new ArgumentException("Unknown Attachment Type", nameof(Attachment));
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
