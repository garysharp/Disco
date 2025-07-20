using Disco.Models.UI.Config.Logging;
using Disco.Services.Logging;
using Disco.Services.Logging.Models;
using System.Collections.Generic;

namespace Disco.Web.Areas.Config.Models.Logging
{
    public class IndexModel : ConfigLoggingIndexModel
    {
        public Dictionary<LogBase, List<LogEventType>> LogModules { get; set; }
    }
}