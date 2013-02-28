using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Disco.Models.Repository;
using Disco.Data.Repository;
using System.IO;
using Disco.BI.DocumentTemplateBI;

namespace Disco.BI.Extensions
{
    public static class AttachmentExtensions
    {

        public static bool ImportPdfAttachment(this DocumentUniqueIdentifier UniqueIdentifier, DiscoDataContext dbContext, System.IO.Stream PdfContent, byte[] PdfThumbnail)
        {

            UniqueIdentifier.LoadComponents(dbContext);
            DocumentTemplate documentTemplate = UniqueIdentifier.DocumentTemplate;
            string filename;
            string comments;

            if (documentTemplate == null)
            {
                filename = string.Format("{0}_{1:yyyyMMdd-HHmmss}.pdf", UniqueIdentifier.DataId, UniqueIdentifier.TimeStamp);
                comments = string.Format("Uploaded: {0:s}", UniqueIdentifier.TimeStamp);
            }
            else
            {
                filename = string.Format("{0}_{1:yyyyMMdd-HHmmss}.pdf", UniqueIdentifier.TemplateTypeId, UniqueIdentifier.TimeStamp);
                comments = string.Format("Generated: {0:s}", UniqueIdentifier.TimeStamp);
            }

            User creatorUser = UserBI.UserCache.GetUser(UniqueIdentifier.CreatorId, dbContext);
            if (creatorUser == null)
            {
                // No Creator User (or Username invalid)
                creatorUser = UserBI.UserCache.CurrentUser;
            }
            switch (UniqueIdentifier.DataScope)
            {
                case DocumentTemplate.DocumentTemplateScopes.Device:
                    Device d = (Device)UniqueIdentifier.Data;
                    d.CreateAttachment(dbContext, creatorUser, filename, DocumentTemplate.PdfMimeType, comments, PdfContent, documentTemplate, PdfThumbnail);
                    return true;
                case DocumentTemplate.DocumentTemplateScopes.Job:
                    Job j = (Job)UniqueIdentifier.Data;
                    j.CreateAttachment(dbContext, creatorUser, filename, DocumentTemplate.PdfMimeType, comments, PdfContent, documentTemplate, PdfThumbnail);
                    return true;
                case DocumentTemplate.DocumentTemplateScopes.User:
                    User u = (User)UniqueIdentifier.Data;
                    u.CreateAttachment(dbContext, creatorUser, filename, DocumentTemplate.PdfMimeType, comments, PdfContent, documentTemplate, PdfThumbnail);
                    return true;
                default:
                    return false;
            }

        }

        public static string RepositoryFilename(this DeviceAttachment da, DiscoDataContext dbContext)
        {
            return Path.Combine(DataStore.CreateLocation(dbContext, "DeviceAttachments", da.Timestamp), string.Format("{0}_{1}_file", da.DeviceSerialNumber, da.Id));
        }
        public static string RepositoryFilename(this JobAttachment ja, DiscoDataContext dbContext)
        {
            return Path.Combine(DataStore.CreateLocation(dbContext, "JobAttachments", ja.Timestamp), string.Format("{0}_{1}_file", ja.JobId, ja.Id));
        }
        public static string RepositoryFilename(this UserAttachment ua, DiscoDataContext dbContext)
        {
            return Path.Combine(DataStore.CreateLocation(dbContext, "UserAttachments", ua.Timestamp), string.Format("{0}_{1}_file", ua.UserId, ua.Id));
        }

        private static string RepositoryThumbnailFilenameInternal(string DirectoryPath, string Filename)
        {
            return Path.Combine(DirectoryPath, Filename);
        }
        public static string RepositoryThumbnailFilename(this DeviceAttachment da, DiscoDataContext dbContext)
        {
            return RepositoryThumbnailFilenameInternal(DataStore.CreateLocation(dbContext, "DeviceAttachments", da.Timestamp), string.Format("{0}_{1}_thumb.jpg", da.DeviceSerialNumber, da.Id));
        }
        public static string RepositoryThumbnailFilename(this JobAttachment ja, DiscoDataContext dbContext)
        {
            return RepositoryThumbnailFilenameInternal(DataStore.CreateLocation(dbContext, "JobAttachments", ja.Timestamp), string.Format("{0}_{1}_thumb.jpg", ja.JobId, ja.Id));
        }
        public static string RepositoryThumbnailFilename(this UserAttachment ua, DiscoDataContext dbContext)
        {
            return RepositoryThumbnailFilenameInternal(DataStore.CreateLocation(dbContext, "UserAttachments", ua.Timestamp), string.Format("{0}_{1}_thumb.jpg", ua.UserId, ua.Id));
        }

        public static void RepositoryDelete(this DeviceAttachment da, DiscoDataContext dbContext)
        {
            RepositoryDelete(da.RepositoryFilename(dbContext), da.RepositoryThumbnailFilename(dbContext));
        }
        public static void RepositoryDelete(this JobAttachment ja, DiscoDataContext dbContext)
        {
            RepositoryDelete(ja.RepositoryFilename(dbContext), ja.RepositoryThumbnailFilename(dbContext));
        }
        public static void RepositoryDelete(this UserAttachment ua, DiscoDataContext dbContext)
        {
            RepositoryDelete(ua.RepositoryFilename(dbContext), ua.RepositoryThumbnailFilename(dbContext));
        }
        private static void RepositoryDelete(params string[] filePaths)
        {
            foreach (string filePath in filePaths)
            {
                if (File.Exists(filePath))
                    File.Delete(filePath);
            }
        }

        public static string SaveAttachment(this DeviceAttachment da, DiscoDataContext dbContext, Stream FileContent)
        {
            string filePath = da.RepositoryFilename(dbContext);
            SaveAttachment(filePath, FileContent);
            return filePath;
        }
        public static string SaveAttachment(this JobAttachment ja, DiscoDataContext dbContext, Stream FileContent)
        {
            string filePath = ja.RepositoryFilename(dbContext);
            SaveAttachment(filePath, FileContent);
            return filePath;
        }
        public static string SaveAttachment(this UserAttachment ua, DiscoDataContext dbContext, Stream FileContent)
        {
            string filePath = ua.RepositoryFilename(dbContext);
            SaveAttachment(filePath, FileContent);
            return filePath;
        }
        public static string SaveThumbnailAttachment(this DeviceAttachment da, DiscoDataContext dbContext, byte[] FileContent)
        {
            string filePath = da.RepositoryThumbnailFilename(dbContext);
            File.WriteAllBytes(filePath, FileContent);
            return filePath;
        }
        public static string SaveThumbnailAttachment(this JobAttachment ja, DiscoDataContext dbContext, byte[] FileContent)
        {
            string filePath = ja.RepositoryThumbnailFilename(dbContext);
            File.WriteAllBytes(filePath, FileContent);
            return filePath;
        }
        public static string SaveThumbnailAttachment(this UserAttachment ua, DiscoDataContext dbContext, byte[] FileContent)
        {
            string filePath = ua.RepositoryThumbnailFilename(dbContext);
            File.WriteAllBytes(filePath, FileContent);
            return filePath;
        }
        private static void SaveAttachment(string FilePath, Stream FileContent)
        {
            using (FileStream sw = new FileStream(FilePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                FileContent.CopyTo(sw);
                sw.Flush();
                sw.Close();
            }
        }

        public static string GenerateThumbnail(this DeviceAttachment da, DiscoDataContext dbContext)
        {
            string filePath = da.RepositoryThumbnailFilename(dbContext);
            AttachmentBI.Utilities.GenerateThumbnail(da.RepositoryFilename(dbContext), da.MimeType, filePath);
            return filePath;
        }
        public static string GenerateThumbnail(this JobAttachment ja, DiscoDataContext dbContext)
        {
            string filePath = ja.RepositoryThumbnailFilename(dbContext);
            AttachmentBI.Utilities.GenerateThumbnail(ja.RepositoryFilename(dbContext), ja.MimeType, filePath);
            return filePath;
        }
        public static string GenerateThumbnail(this UserAttachment ua, DiscoDataContext dbContext)
        {
            string filePath = ua.RepositoryThumbnailFilename(dbContext);
            AttachmentBI.Utilities.GenerateThumbnail(ua.RepositoryFilename(dbContext), ua.MimeType, filePath);
            return filePath;
        }
        public static string GenerateThumbnail(this DeviceAttachment da, DiscoDataContext dbContext, Stream SourceFile)
        {
            string filePath = da.RepositoryThumbnailFilename(dbContext);
            AttachmentBI.Utilities.GenerateThumbnail(SourceFile, da.MimeType, filePath);
            return filePath;
        }
        public static string GenerateThumbnail(this JobAttachment ja, DiscoDataContext dbContext, Stream SourceFile)
        {
            string filePath = ja.RepositoryThumbnailFilename(dbContext);
            AttachmentBI.Utilities.GenerateThumbnail(SourceFile, ja.MimeType, filePath);
            return filePath;
        }
        public static string GenerateThumbnail(this UserAttachment ua, DiscoDataContext dbContext, Stream SourceFile)
        {
            string filePath = ua.RepositoryThumbnailFilename(dbContext);
            AttachmentBI.Utilities.GenerateThumbnail(SourceFile, ua.MimeType, filePath);
            return filePath;
        }


    }
}
