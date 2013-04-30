using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Disco.Services.Logging;
using Disco.Services.Logging.Models;
using Disco.Models.UI.Config.Logging;

namespace Disco.Web.Areas.Config.Models.Logging
{
    public class IndexModel : ConfigLoggingIndexModel
    {
        public Dictionary<LogBase, List<LogEventType>> LogModules { get; set; }
    }
}