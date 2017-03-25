using System.Collections.Generic;
using System.Linq;

namespace Disco.Services.Logging
{
    public abstract class LogBase
    {
        private Dictionary<int, Models.LogEventType> _EventTypes;

        public LogBase()
        {
            // Cache Event Types
            _EventTypes = LoadEventTypes().ToDictionary(et => et.Id);
        }

        public abstract int ModuleId { get; }
        public abstract string ModuleName { get; }
        public abstract string ModuleDescription { get; }
        protected abstract List<Models.LogEventType> LoadEventTypes();

        public Dictionary<int, Models.LogEventType> EventTypes
        {
            get
            {
                return _EventTypes;
            }
        }
        protected void Log(int EventTypeId, params object[] Args)
        {
            LogContext.Current.Log(ModuleId, EventTypeId, Args);
        }
        public string LiveLogGroupName
        {
            get
            {
                return ModuleName;
            }
        }

    }
}
