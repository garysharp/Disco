using System.Collections.Generic;

namespace Disco.Web.Areas.API.Models.Job
{
    public class CommentsModel
    {
        public string Result { get; set; }
        public List<_CommentModel> Comments { get; set; }
    }
}