using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Disco.Web.Areas.API.Models.Job
{
    public class CommentPostModel
    {
        public string Result { get; set; }
        public _CommentModel Comment { get; set; }
    }
}