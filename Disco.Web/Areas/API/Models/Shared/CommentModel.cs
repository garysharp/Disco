using Disco.Models.Repository;
using System;

namespace Disco.Web.Areas.API.Models.Shared
{
    public class CommentModel
    {
        public int Id { get; set; }
        public AttachmentTypes TargetType { get; set; }
        public string TargetId { get; set; }
        public string AuthorId { get; set; }
        public string Author { get; set; }
        public DateTime Timestamp { get; set; }
        public string Comments { get; set; }
        public string HtmlComments { get; set; }
        public long TimestampUnixEpoc => Timestamp.ToUnixEpoc();
        public string TimestampFull => Timestamp.ToFullDateTime();

        public static CommentModel FromEntity(JobLog log)
        {
            return new CommentModel
            {
                Id = log.Id,
                TargetType = AttachmentTypes.Job,
                TargetId = log.JobId.ToString(),
                AuthorId = log.TechUserId,
                Author = log.TechUser.ToString(),
                Timestamp = log.Timestamp,
                Comments = log.Comments,
                HtmlComments = log.Comments.ToHtmlComment().ToString()
            };
        }

        public static CommentModel FromEntity(UserComment comment)
        {
            return new CommentModel
            {
                Id = comment.Id,
                TargetType = AttachmentTypes.User,
                TargetId = comment.UserId,
                AuthorId = comment.TechUserId,
                Author = comment.TechUser.ToString(),
                Timestamp = comment.Timestamp,
                Comments = comment.Comments,
                HtmlComments = comment.Comments.ToHtmlComment().ToString()
            };
        }

        public static CommentModel FromEntity(DeviceComment comment)
        {
            return new CommentModel
            {
                Id = comment.Id,
                TargetType = AttachmentTypes.Device,
                TargetId = comment.DeviceSerialNumber,
                AuthorId = comment.TechUserId,
                Author = comment.TechUser.ToString(),
                Timestamp = comment.Timestamp,
                Comments = comment.Comments,
                HtmlComments = comment.Comments.ToHtmlComment().ToString()
            };
        }
    }
}