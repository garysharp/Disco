using Disco.Models.UI.Config.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Disco.Web.Areas.Config.Models.Shared
{
    public class TaskStatusModel : ConfigSharedTaskStatusModel
    {
        public string SessionId { get; set; }
    }
}