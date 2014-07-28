using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Disco.Services.Logging;
using Disco.Services.Logging.Models;
using Exceptionless;

namespace Disco.Services.Tasks
{
    public class ScheduledTasksLog : LogBase
    {
        private const int _ModuleId = 20;

        public override string ModuleDescription { get { return "Scheduled Tasks"; } }
        public override int ModuleId { get { return _ModuleId; } }
        public override string ModuleName { get { return "ScheduledTasks"; } }

        public enum EventTypeIds
        {
            InitializingScheduledTasks = 10,
            InitializeException = 15,
            InitializeExceptionWithInner,
            InitializeScheduledTasksException,
            InitializeScheduledTasksExceptionWithInner,
            ScheduledTasksException = 30,
            ScheduledTasksExceptionWithInner,
            ScheduledTaskExecuted = 50,
            ScheduledTaskFinished = 80,
        }
        public static ScheduledTasksLog Current
        {
            get
            {
                return (ScheduledTasksLog)LogContext.LogModules[_ModuleId];
            }
        }
        private static void Log(EventTypeIds EventTypeId, params object[] Args)
        {
            Current.Log((int)EventTypeId, Args);
        }

        public static void LogInitializingScheduledTasks()
        {
            Current.Log((int)EventTypeIds.InitializingScheduledTasks);
        }
        public static void LogScheduledTaskExecuted(string TaskName, string SessionId)
        {
            Current.Log((int)EventTypeIds.ScheduledTaskExecuted, TaskName, SessionId);
        }
        public static void LogScheduledTaskFinished(string TaskName, string SessionId)
        {
            Current.Log((int)EventTypeIds.ScheduledTaskFinished, TaskName, SessionId);
        }

        public static void LogInitializeException(Exception ex)
        {
            ex.ToExceptionless().Submit();

            if (ex.InnerException != null)
            {
                Log(EventTypeIds.InitializeExceptionWithInner, ex.GetType().Name, ex.Message, ex.StackTrace, ex.InnerException.GetType().Name, ex.InnerException.Message, ex.InnerException.StackTrace);
            }
            else
            {
                Log(EventTypeIds.InitializeException, ex.GetType().Name, ex.Message, ex.StackTrace);
            }
        }
        public static void LogInitializeException(Exception ex, Type ScheduledTaskType)
        {
            ex.ToExceptionless().Submit();

            if (ex.InnerException != null)
            {
                Log(EventTypeIds.InitializeScheduledTasksExceptionWithInner, ScheduledTaskType.Name, ScheduledTaskType.Assembly.Location, ex.GetType().Name, ex.Message, ex.StackTrace, ex.InnerException.GetType().Name, ex.InnerException.Message, ex.InnerException.StackTrace);
            }
            else
            {
                Log(EventTypeIds.InitializeScheduledTasksException, ScheduledTaskType.Name, ScheduledTaskType.Assembly.Location, ex.GetType().Name, ex.Message, ex.StackTrace);
            }
        }

        public static void LogScheduledTaskException(string ScheduledTaskName, string SessionId, Type ScheduledTaskType, Exception ex)
        {
            ex.ToExceptionless()
                .AddTags("Scheduled Task")
                .AddObject(ScheduledTaskName, "ScheduledTaskName")
                .AddObject(ScheduledTaskType.Name, "ScheduledTaskTypeName")
                .Submit();

            if (ex.InnerException != null)
            {
                Log(EventTypeIds.ScheduledTasksExceptionWithInner, ScheduledTaskName, SessionId, ScheduledTaskType.Assembly.Location, ex.GetType().Name, ex.Message, ex.StackTrace, ex.InnerException.GetType().Name, ex.InnerException.Message, ex.InnerException.StackTrace);
            }
            else
            {
                Log(EventTypeIds.ScheduledTasksException, ScheduledTaskName, SessionId, ScheduledTaskType.Assembly.Location, ex.GetType().Name, ex.Message, ex.StackTrace);
            }
        }

        protected override List<Logging.Models.LogEventType> LoadEventTypes()
        {
            return new System.Collections.Generic.List<LogEventType>
			{
				new LogEventType
				{
					Id = (int)EventTypeIds.InitializingScheduledTasks, 
					ModuleId = _ModuleId, 
					Name = "Initializing Scheduled Tasks", 
					Format = "Starting Scheduled Task discovery and initialization", 
					Severity =  (int)LogEventType.Severities.Information, 
					UseLive = false, 
					UsePersist = true, 
					UseDisplay = true
				}, 
				new LogEventType
				{
					Id = (int)EventTypeIds.InitializeException, 
					ModuleId = _ModuleId, 
					Name = "Initialize Exception", 
					Format = "Exception: {0}: {1}; {2}", 
					Severity = (int)LogEventType.Severities.Error, 
					UseLive = false, 
					UsePersist = true, 
					UseDisplay = true
				}, 
				new LogEventType
				{
					Id = (int)EventTypeIds.InitializeExceptionWithInner, 
					ModuleId = _ModuleId, 
					Name = "Initialize Exception with Inner Exception", 
					Format = "Exception: {0}: {1}; {2}; Inner: {3}: {4}; {5}", 
					Severity = (int)LogEventType.Severities.Error, 
					UseLive = false, 
					UsePersist = true, 
					UseDisplay = true
				}, 
                new LogEventType
				{
					Id = (int)EventTypeIds.InitializeScheduledTasksException, 
					ModuleId = _ModuleId, 
					Name = "Initialize Task Exception", 
					Format = "[{0}] At '{1}'; Exception: {2}: {3}; {4}", 
					Severity = (int)LogEventType.Severities.Error, 
					UseLive = false, 
					UsePersist = true, 
					UseDisplay = true
				}, 
				new LogEventType
				{
					Id = (int)EventTypeIds.InitializeScheduledTasksExceptionWithInner, 
					ModuleId = _ModuleId, 
					Name = "Initialize Task Exception with Inner Exception", 
					Format = "[{0}] At '{1}'; Exception: {2}: {3}; {4}; Inner: {5}: {6}; {7}", 
					Severity = (int)LogEventType.Severities.Error, 
					UseLive = false, 
					UsePersist = true, 
					UseDisplay = true
				}, 
				new LogEventType
				{
					Id = (int)EventTypeIds.ScheduledTasksException, 
					ModuleId = _ModuleId, 
					Name = "Scheduled Task Exception", 
					Format = "Task Name: {0}; SessionId: {1}; At: '{2}'; Exception: {3}: {4}; {5}", 
					Severity = (int)LogEventType.Severities.Error, 
					UseLive = true, 
					UsePersist = true, 
					UseDisplay = true
				}, 
				new LogEventType
				{
					Id = (int)EventTypeIds.ScheduledTasksExceptionWithInner, 
					ModuleId = _ModuleId, 
					Name = "Scheduled Task Exception with Inner Exception", 
					Format = "Task Name: {0}; SessionId: {1}; At: '{2}'; Exception: {3}: {4}; {5}; Inner: {6}: {7}; {8}", 
					Severity = (int)LogEventType.Severities.Error, 
					UseLive = true, 
					UsePersist = true, 
					UseDisplay = true
				}, 
				new LogEventType
				{
					Id = (int)EventTypeIds.ScheduledTaskExecuted, 
					ModuleId = _ModuleId, 
					Name = "Scheduled Task Started", 
					Format = "Scheduled Task Started: {0}; Session Id: {1}", 
					Severity = (int)LogEventType.Severities.Information, 
					UseLive = true, 
					UsePersist = true, 
					UseDisplay = true
				}, 
				new LogEventType
				{
					Id = (int)EventTypeIds.ScheduledTaskFinished, 
					ModuleId = _ModuleId, 
					Name = "Scheduled Task Finished", 
					Format = "Scheduled Task Finished: {0}; Session Id: {1}", 
					Severity = (int)LogEventType.Severities.Information, 
					UseLive = true, 
					UsePersist = true, 
					UseDisplay = true
				}
			};
        }
    }
}
