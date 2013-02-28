using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Disco.Logging.Models;

namespace Disco.Logging
{
    public class SystemLog : LogBase
    {
        private const int _ModuleId = 0;
        public enum EventTypeIds : int
        {
            Information = 0,
            Warning = 1,
            Error = 2,
            Exception = 10,
            ExceptionWithInner = 11,
            LogInitialized = 100
        }
        public static SystemLog Current
        {
            get
            {
                return (SystemLog)LogContext.LogModules[_ModuleId];
            }
        }
        private static void Log(EventTypeIds EventTypeId, params object[] Args)
        {
            Current.Log((int)EventTypeId, Args);
        }
        public static void LogInformation(params object[] Messages)
        {
            Log(EventTypeIds.Information, Messages);
        }
        public static void LogWarning(params object[] Messages)
        {
            Log(EventTypeIds.Warning, Messages);
        }
        public static void LogError(params object[] Messages)
        {
            Log(EventTypeIds.Error, Messages);
        }
        public static void LogException(string Component, Exception ex)
        {
            if (ex.InnerException != null)
            {
                Log(EventTypeIds.ExceptionWithInner, Component, ex.GetType().Name, ex.Message, ex.StackTrace, ex.InnerException.GetType().Name, ex.InnerException.Message, ex.InnerException.StackTrace);
            }
            else
            {
                Log(EventTypeIds.Exception, Component, ex.GetType().Name, ex.Message, ex.StackTrace);
            }
        }

        public static void LogLogInitialized(string PersistantStorePath)
        {
            Log(EventTypeIds.LogInitialized, PersistantStorePath);
        }

        public override int ModuleId
        {
            get { return _ModuleId; }
        }

        public override string ModuleName
        {
            get { return "System"; }
        }

        public override string ModuleDescription
        {
            get { return "Core System Log"; }
        }

        protected override List<LogEventType> LoadEventTypes()
        {
            List<LogEventType> eventTypes = new List<LogEventType>() {
                 new LogEventType() {
                     Id = (int)EventTypeIds.Information,
                     ModuleId = _ModuleId,
                     Name = "Information",
                     Format = null,
                     Severity = (int)LogEventType.Severities.Information,
                     UseLive = true, UsePersist = true, UseDisplay = true },
                new LogEventType() {
                     Id = (int)EventTypeIds.Warning,
                     ModuleId = _ModuleId,
                     Name = "Warning",
                     Format = null,
                     Severity = (int)LogEventType.Severities.Warning,
                     UseLive = true, UsePersist = true, UseDisplay = true },
                new LogEventType() {
                     Id = (int)EventTypeIds.Error,
                     ModuleId = _ModuleId,
                     Name = "Error",
                     Format = null,
                     Severity = (int)LogEventType.Severities.Error,
                     UseLive = true, UsePersist = true, UseDisplay = true },
                new LogEventType() {
                     Id = (int)EventTypeIds.Exception,
                     ModuleId = _ModuleId,
                     Name = "Exception",
                     Format = "{0}; {1}: {2}; {3}",
                     Severity = (int)LogEventType.Severities.Error,
                     UseLive = true, UsePersist = true, UseDisplay = true },
                new LogEventType() {
                     Id = (int)EventTypeIds.ExceptionWithInner,
                     ModuleId = _ModuleId,
                     Name = "Exception with Inner Exception",
                     Format = "{0}; {1}: {2}; {3}; {4}: {5}; {6}",
                     Severity = (int)LogEventType.Severities.Error,
                     UseLive = true, UsePersist = true, UseDisplay = true },
                new LogEventType() {
                     Id = (int)EventTypeIds.LogInitialized,
                     ModuleId = _ModuleId,
                     Name = "Log Initialized",
                     Format = "Log Initialized to '{0}'",
                     Severity = (int)LogEventType.Severities.Information,
                     UseLive = false, UsePersist = true, UseDisplay = true }
            };

            return eventTypes;
        }
    }
}
