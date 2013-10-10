using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Disco.Models.Repository;
using Disco.Data.Repository;
using Disco.Services.Users;
using Disco.Services.Authorization;

namespace Disco.BI.Extensions
{
    public static class AttachmentActionExtensions
    {

        #region Delete
        public static bool CanDelete(this DeviceAttachment da)
        {
            if (UserService.CurrentAuthorization.Has(Claims.Device.Actions.RemoveAnyAttachments))
                return true;

            if (UserService.CurrentAuthorization.Has(Claims.Device.Actions.RemoveOwnAttachments)
                && da.TechUserId == UserService.CurrentUserId)
                return true;

            return false;
        }
        public static void OnDelete(this DeviceAttachment da, DiscoDataContext Database)
        {
            if (!da.CanDelete())
                throw new InvalidOperationException("Deletion of Attachment is Denied");

            da.RepositoryDelete(Database);
            Database.DeviceAttachments.Remove(da);
        }
        public static bool CanDelete(this JobAttachment ja)
        {
            if (UserService.CurrentAuthorization.Has(Claims.Job.Actions.RemoveAnyAttachments))
                return true;

            if (UserService.CurrentAuthorization.Has(Claims.Job.Actions.RemoveOwnAttachments)
                && ja.TechUserId == UserService.CurrentUserId)
                return true;

            return false;
        }
        public static void OnDelete(this JobAttachment ja, DiscoDataContext Database)
        {
            if (!ja.CanDelete())
                throw new InvalidOperationException("Deletion of Attachment is Denied");

            ja.RepositoryDelete(Database);
            Database.JobAttachments.Remove(ja);
        }
        public static bool CanDelete(this UserAttachment ua)
        {
            if (UserService.CurrentAuthorization.Has(Claims.User.Actions.RemoveAnyAttachments))
                return true;

            if (UserService.CurrentAuthorization.Has(Claims.User.Actions.RemoveOwnAttachments)
                && ua.TechUserId == UserService.CurrentUserId)
                return true;

            return false;
        }
        public static void OnDelete(this UserAttachment ua, DiscoDataContext Database)
        {
            if (!ua.CanDelete())
                throw new InvalidOperationException("Deletion of Attachment is Denied");

            ua.RepositoryDelete(Database);
            Database.UserAttachments.Remove(ua);
        }
        #endregion

    }
}
