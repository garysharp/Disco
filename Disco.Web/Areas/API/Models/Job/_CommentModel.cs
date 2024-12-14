using System;

namespace Disco.Web.Areas.API.Models.Job
{
    public class _CommentModel
    {
        public int Id { get; set; }
        public int JobId { get; set; }
        public string AuthorId { get; set; }
        public string Author { get; set; }
        public DateTime Timestamp { get; set; }
        public string Comments { get; set; }
        public string HtmlComments { get; set; }
        public long TimestampUnixEpoc { get { return this.Timestamp.ToUnixEpoc(); } }
        public string TimestampFull { get { return Timestamp.ToFullDateTime(); } }

        public static _CommentModel FromJobLog(Disco.Models.Repository.JobLog jl)
        {
            return new _CommentModel
            {
                Id = jl.Id,
                JobId = jl.JobId,
                AuthorId = jl.TechUserId,
                Author = jl.TechUser.ToString(),
                Timestamp = jl.Timestamp,
                Comments = jl.Comments,
                HtmlComments = jl.Comments.ToHtmlComment().ToString()
            };
        }

    }
}