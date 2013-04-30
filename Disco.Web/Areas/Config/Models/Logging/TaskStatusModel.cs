using Disco.Models.UI.Config.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Disco.Web.Areas.Config.Models.Logging
{
    public class TaskStatusModel : ConfigLoggingTaskStatusModel
    {
        public string SessionId { get; set; }
    }
}