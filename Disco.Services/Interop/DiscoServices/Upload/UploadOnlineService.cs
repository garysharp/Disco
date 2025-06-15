using Disco.Models.Repository;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Disco.Services.Interop.DiscoServices.Upload
{
    public static class UploadOnlineService
    {
        private static readonly UploadOnlineClient client;

        static UploadOnlineService()
        {
            client = new UploadOnlineClient();
        }

        public static Task<(Uri sessionUri, DateTime sessionExpiration)> CreateSession(User techUser, IAttachmentTarget attachmentTarget)
        {
            return client.CreateSession(techUser, attachmentTarget);
        }

        internal static MemoryStream SyncUploads(string lastFileId, string hintFileId)
        {
            return client.SyncUploads(lastFileId, hintFileId);
        }
    }
}
