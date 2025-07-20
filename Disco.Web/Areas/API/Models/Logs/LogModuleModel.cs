using Disco.Services.Logging;
using System.Collections.Generic;
using System.Linq;

namespace Disco.Web.Areas.API.Models.Logs
{
    public class LogModuleModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public List<LogEventTypeModel> EventTypes { get; set; }

        public static LogModuleModel FromLogModule(LogBase LogModule)
        {
            return new LogModuleModel()
            {
                Id = LogModule.ModuleId,
                Name = LogModule.ModuleName,
                Description = LogModule.ModuleDescription,
                EventTypes = LogModule.EventTypes.Values.Select(et => LogEventTypeModel.FromLogEventType(et)).ToList()
            };
        }
    }
}