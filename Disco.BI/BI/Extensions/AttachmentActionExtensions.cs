using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Disco.Models.Repository;
using Disco.Data.Repository;
using Disco.Services.Users;
using Disco.Services.Authorization;
using Disco.BI.DocumentTemplateBI.ManagedGroups;
using Disco.Services;

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
                && da.TechUserId.Equals(UserService.CurrentUserId, StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        }
        public static void OnDelete(this DeviceAttachment da, DiscoDataContext Database)
        {
            if (!da.CanDelete())
                throw new InvalidOperationException("Deletion of Attachment is Denied");

            var attachmentId = da.Id;
            var documentTemplateId = da.DocumentTemplateId;
            var deviceSerialNumber = da.DeviceSerialNumber;

            da.RepositoryDelete(Database);
            Database.DeviceAttachments.Remove(da);

            DocumentTemplateManagedGroups.TriggerDeviceAttachmentDeleted(Database, attachmentId, documentTemplateId, deviceSerialNumber);
        }
        public static bool CanDelete(this JobAttachment ja)
        {
            if (UserService.CurrentAuthorization.Has(Claims.Job.Actions.RemoveAnyAttachments))
                return true;

            if (UserService.CurrentAuthorization.Has(Claims.Job.Actions.RemoveOwnAttachments)
                && ja.TechUserId.Equals(UserService.CurrentUserId, StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        }
        public static void OnDelete(this JobAttachment ja, DiscoDataContext Database)
        {
            if (!ja.CanDelete())
                throw new InvalidOperationException("Deletion of Attachment is Denied");

            var attachmentId = ja.Id;
            var documentTemplateId = ja.DocumentTemplateId;
            var jobId = ja.JobId;

            ja.RepositoryDelete(Database);
            Database.JobAttachments.Remove(ja);

            DocumentTemplateManagedGroups.TriggerJobAttachmentDeleted(Database, attachmentId, documentTemplateId, jobId);
        }
        public static bool CanDelete(this UserAttachment ua)
        {
            if (UserService.CurrentAuthorization.Has(Claims.User.Actions.RemoveAnyAttachments))
                return true;

            if (UserService.CurrentAuthorization.Has(Claims.User.Actions.RemoveOwnAttachments)
                && ua.TechUserId.Equals(UserService.CurrentUserId, StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        }
        public static void OnDelete(this UserAttachment ua, DiscoDataContext Database)
        {
            if (!ua.CanDelete())
                throw new InvalidOperationException("Deletion of Attachment is Denied");

            var attachmentId = ua.Id;
            var documentTemplateId = ua.DocumentTemplateId;
            var userId = ua.UserId;

            ua.RepositoryDelete(Database);
            Database.UserAttachments.Remove(ua);

            DocumentTemplateManagedGroups.TriggerUserAttachmentDeleted(Database, attachmentId, documentTemplateId, userId);
        }
        #endregion

    }
}
