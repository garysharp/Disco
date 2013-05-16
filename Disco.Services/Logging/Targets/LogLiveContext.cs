using Disco.Services.Logging.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Services.Logging.Targets
{
    public static class LogLiveContext
    {
        public delegate void LogBroadcastEvent(LogBase logModule, LogEventType eventType, DateTime Timestamp, params object[] Arguments);
        public static event LogBroadcastEvent LogBroadcast;

        internal static void Broadcast(LogBase logModule, LogEventType eventType, DateTime Timestamp, params object[] Arguments)
        {
            if (LogBroadcast != null)
                LogBroadcast.Invoke(logModule, eventType, Timestamp, Arguments);
        }
    }
}
