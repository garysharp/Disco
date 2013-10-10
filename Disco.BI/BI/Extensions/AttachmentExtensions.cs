using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Disco.Models.Repository;
using Disco.Data.Repository;
using System.IO;
using Disco.BI.DocumentTemplateBI;
using Disco.Services.Users;

namespace Disco.BI.Extensions
{
    public static class AttachmentExtensions
    {

        public static bool ImportPdfAttachment(this DocumentUniqueIdentifier UniqueIdentifier, DiscoDataContext Database, System.IO.Stream PdfContent, byte[] PdfThumbnail)
        {

            UniqueIdentifier.LoadComponents(Database);
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

            User creatorUser = UserService.GetUser(UniqueIdentifier.CreatorId, Database);
            if (creatorUser == null)
            {
                // No Creator User (or Username invalid)
                creatorUser = UserService.CurrentUser;
            }
            switch (UniqueIdentifier.DataScope)
            {
                case DocumentTemplate.DocumentTemplateScopes.Device:
                    Device d = (Device)UniqueIdentifier.Data;
                    d.CreateAttachment(Database, creatorUser, filename, DocumentTemplate.PdfMimeType, comments, PdfContent, documentTemplate, PdfThumbnail);
                    return true;
                case DocumentTemplate.DocumentTemplateScopes.Job:
                    Job j = (Job)UniqueIdentifier.Data;
                    j.CreateAttachment(Database, creatorUser, filename, DocumentTemplate.PdfMimeType, comments, PdfContent, documentTemplate, PdfThumbnail);
                    return true;
                case DocumentTemplate.DocumentTemplateScopes.User:
                    User u = (User)UniqueIdentifier.Data;
                    u.CreateAttachment(Database, creatorUser, filename, DocumentTemplate.PdfMimeType, comments, PdfContent, documentTemplate, PdfThumbnail);
                    return true;
                default:
                    return false;
            }

        }

        public static string RepositoryFilename(this DeviceAttachment da, DiscoDataContext Database)
        {
            return Path.Combine(DataStore.CreateLocation(Database, "DeviceAttachments", da.Timestamp), string.Format("{0}_{1}_file", da.DeviceSerialNumber, da.Id));
        }
        public static string RepositoryFilename(this JobAttachment ja, DiscoDataContext Database)
        {
            return Path.Combine(DataStore.CreateLocation(Database, "JobAttachments", ja.Timestamp), string.Format("{0}_{1}_file", ja.JobId, ja.Id));
        }
        public static string RepositoryFilename(this UserAttachment ua, DiscoDataContext Database)
        {
            return Path.Combine(DataStore.CreateLocation(Database, "UserAttachments", ua.Timestamp), string.Format("{0}_{1}_file", ua.UserId, ua.Id));
        }

        private static string RepositoryThumbnailFilenameInternal(string DirectoryPath, string Filename)
        {
            return Path.Combine(DirectoryPath, Filename);
        }
        public static string RepositoryThumbnailFilename(this DeviceAttachment da, DiscoDataContext Database)
        {
            return RepositoryThumbnailFilenameInternal(DataStore.CreateLocation(Database, "DeviceAttachments", da.Timestamp), string.Format("{0}_{1}_thumb.jpg", da.DeviceSerialNumber, da.Id));
        }
        public static string RepositoryThumbnailFilename(this JobAttachment ja, DiscoDataContext Database)
        {
            return RepositoryThumbnailFilenameInternal(DataStore.CreateLocation(Database, "JobAttachments", ja.Timestamp), string.Format("{0}_{1}_thumb.jpg", ja.JobId, ja.Id));
        }
        public static string RepositoryThumbnailFilename(this UserAttachment ua, DiscoDataContext Database)
        {
            return RepositoryThumbnailFilenameInternal(DataStore.CreateLocation(Database, "UserAttachments", ua.Timestamp), string.Format("{0}_{1}_thumb.jpg", ua.UserId, ua.Id));
        }

        public static void RepositoryDelete(this DeviceAttachment da, DiscoDataContext Database)
        {
            RepositoryDelete(da.RepositoryFilename(Database), da.RepositoryThumbnailFilename(Database));
        }
        public static void RepositoryDelete(this JobAttachment ja, DiscoDataContext Database)
        {
            RepositoryDelete(ja.RepositoryFilename(Database), ja.RepositoryThumbnailFilename(Database));
        }
        public static void RepositoryDelete(this UserAttachment ua, DiscoDataContext Database)
        {
            RepositoryDelete(ua.RepositoryFilename(Database), ua.RepositoryThumbnailFilename(Database));
        }
        private static void RepositoryDelete(params string[] filePaths)
        {
            foreach (string filePath in filePaths)
            {
                if (File.Exists(filePath))
                    File.Delete(filePath);
            }
        }

        public static string SaveAttachment(this DeviceAttachment da, DiscoDataContext Database, Stream FileContent)
        {
            string filePath = da.RepositoryFilename(Database);
            SaveAttachment(filePath, FileContent);
            return filePath;
        }
        public static string SaveAttachment(this JobAttachment ja, DiscoDataContext Database, Stream FileContent)
        {
            string filePath = ja.RepositoryFilename(Database);
            SaveAttachment(filePath, FileContent);
            return filePath;
        }
        public static string SaveAttachment(this UserAttachment ua, DiscoDataContext Database, Stream FileContent)
        {
            string filePath = ua.RepositoryFilename(Database);
            SaveAttachment(filePath, FileContent);
            return filePath;
        }
        public static string SaveThumbnailAttachment(this DeviceAttachment da, DiscoDataContext Database, byte[] FileContent)
        {
            string filePath = da.RepositoryThumbnailFilename(Database);
            File.WriteAllBytes(filePath, FileContent);
            return filePath;
        }
        public static string SaveThumbnailAttachment(this JobAttachment ja, DiscoDataContext Database, byte[] FileContent)
        {
            string filePath = ja.RepositoryThumbnailFilename(Database);
            File.WriteAllBytes(filePath, FileContent);
            return filePath;
        }
        public static string SaveThumbnailAttachment(this UserAttachment ua, DiscoDataContext Database, byte[] FileContent)
        {
            string filePath = ua.RepositoryThumbnailFilename(Database);
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

        public static string GenerateThumbnail(this DeviceAttachment da, DiscoDataContext Database)
        {
            string filePath = da.RepositoryThumbnailFilename(Database);
            AttachmentBI.Utilities.GenerateThumbnail(da.RepositoryFilename(Database), da.MimeType, filePath);
            return filePath;
        }
        public static string GenerateThumbnail(this JobAttachment ja, DiscoDataContext Database)
        {
            string filePath = ja.RepositoryThumbnailFilename(Database);
            AttachmentBI.Utilities.GenerateThumbnail(ja.RepositoryFilename(Database), ja.MimeType, filePath);
            return filePath;
        }
        public static string GenerateThumbnail(this UserAttachment ua, DiscoDataContext Database)
        {
            string filePath = ua.RepositoryThumbnailFilename(Database);
            AttachmentBI.Utilities.GenerateThumbnail(ua.RepositoryFilename(Database), ua.MimeType, filePath);
            return filePath;
        }
        public static string GenerateThumbnail(this DeviceAttachment da, DiscoDataContext Database, Stream SourceFile)
        {
            string filePath = da.RepositoryThumbnailFilename(Database);
            AttachmentBI.Utilities.GenerateThumbnail(SourceFile, da.MimeType, filePath);
            return filePath;
        }
        public static string GenerateThumbnail(this JobAttachment ja, DiscoDataContext Database, Stream SourceFile)
        {
            string filePath = ja.RepositoryThumbnailFilename(Database);
            AttachmentBI.Utilities.GenerateThumbnail(SourceFile, ja.MimeType, filePath);
            return filePath;
        }
        public static string GenerateThumbnail(this UserAttachment ua, DiscoDataContext Database, Stream SourceFile)
        {
            string filePath = ua.RepositoryThumbnailFilename(Database);
            AttachmentBI.Utilities.GenerateThumbnail(SourceFile, ua.MimeType, filePath);
            return filePath;
        }


    }
}
