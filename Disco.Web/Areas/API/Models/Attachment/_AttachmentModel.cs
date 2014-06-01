using Disco.Services;
using System;

namespace Disco.Web.Areas.API.Models.Attachment
{
    public class _AttachmentModel
    {
        public string ParentId { get; set; }
        public int Id { get; set; }
        public string Author { get; set; }
        public string AuthorId { get; set; }
        public DateTime Timestamp { get; set; }
        public string Comments { get; set; }
        public string Filename { get; set; }
        public string MimeType { get; set; }
        public long TimestampUnixEpoc { get { return Timestamp.ToUnixEpoc(); } }
        public string TimestampFull { get { return Timestamp.ToFullDateTime(); } }

        public static _AttachmentModel FromAttachment(Disco.Models.Repository.UserAttachment ua)
        {
            return new _AttachmentModel
            {
                ParentId = ua.UserId,
                Id = ua.Id,
                AuthorId = ua.TechUserId,
                Author = ua.TechUser.ToStringFriendly(),
                Timestamp = ua.Timestamp,
                Comments = ua.Comments,
                Filename = ua.Filename,
                MimeType = ua.MimeType
            };
        }
        public static _AttachmentModel FromAttachment(Disco.Models.Repository.JobAttachment ja)
        {
            return new _AttachmentModel
            {
                ParentId = ja.JobId.ToString(),
                Id = ja.Id,
                AuthorId = ja.TechUserId,
                Author = ja.TechUser.ToStringFriendly(),
                Timestamp = ja.Timestamp,
                Comments = ja.Comments,
                Filename = ja.Filename,
                MimeType = ja.MimeType
            };
        }
        public static _AttachmentModel FromAttachment(Disco.Models.Repository.DeviceAttachment da)
        {
            return new _AttachmentModel
            {
                ParentId = da.DeviceSerialNumber,
                Id = da.Id,
                AuthorId = da.TechUserId,
                Author = da.TechUser.ToStringFriendly(),
                Timestamp = da.Timestamp,
                Comments = da.Comments,
                Filename = da.Filename,
                MimeType = da.MimeType
            };
        }

    }
}