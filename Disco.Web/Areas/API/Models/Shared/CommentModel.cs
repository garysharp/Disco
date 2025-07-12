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

        public static CommentModel FromJobLog(JobLog jl)
        {
            return new CommentModel
            {
                Id = jl.Id,
                TargetType = AttachmentTypes.Job,
                TargetId = jl.JobId.ToString(),
                AuthorId = jl.TechUserId,
                Author = jl.TechUser.ToString(),
                Timestamp = jl.Timestamp,
                Comments = jl.Comments,
                HtmlComments = jl.Comments.ToHtmlComment().ToString()
            };
        }

    }
}