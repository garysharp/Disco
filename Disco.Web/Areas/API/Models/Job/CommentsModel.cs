using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Disco.Web.Areas.API.Models.Job
{
    public class CommentsModel
    {
        public string Result { get; set; }
        public List<_CommentModel> Comments { get; set; }
    }
}