using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Disco.Services.Logging;
using Disco.Services.Logging.Models;

namespace Disco.Web.Areas.Config.Models.Logging
{
    public class IndexModel
    {
        public Dictionary<LogBase, List<LogEventType>> LogModules { get; set; }
    }
}