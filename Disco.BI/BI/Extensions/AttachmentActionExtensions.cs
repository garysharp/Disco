using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Disco.Models.Repository;
using Disco.Data.Repository;

namespace Disco.BI.Extensions
{
    public static class AttachmentActionExtensions
    {

        #region Delete
        public static bool CanDelete(this DeviceAttachment da)
        {
            return true; // Placeholder - Currently Can Always Delete;
        }
        public static void OnDelete(this DeviceAttachment da, DiscoDataContext dbContext)
        {
            if (!da.CanDelete())
                throw new InvalidOperationException("Deletion of Attachment is Denied");

            da.RepositoryDelete(dbContext);
            dbContext.DeviceAttachments.Remove(da);
        }
        public static bool CanDelete(this JobAttachment ja)
        {
            return true; // Placeholder - Currently Can Always Delete;
        }
        public static void OnDelete(this JobAttachment ja, DiscoDataContext dbContext)
        {
            if (!ja.CanDelete())
                throw new InvalidOperationException("Deletion of Attachment is Denied");

            ja.RepositoryDelete(dbContext);
            dbContext.JobAttachments.Remove(ja);
        }
        public static bool CanDelete(this UserAttachment ua)
        {
            return true; // Placeholder - Currently Can Always Delete;
        }
        public static void OnDelete(this UserAttachment ua, DiscoDataContext dbContext)
        {
            if (!ua.CanDelete())
                throw new InvalidOperationException("Deletion of Attachment is Denied");

            ua.RepositoryDelete(dbContext);
            dbContext.UserAttachments.Remove(ua);
        }
        #endregion

    }
}
