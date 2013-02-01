using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Disco.Services.Logging;
using Disco.Services.Logging.Models;
using System.Reflection;

namespace Disco.Services.Plugins
{
    public class PluginsLog : LogBase
    {
        private const int _ModuleId = 10;

        public override string ModuleDescription { get { return "Plugins"; } }
        public override int ModuleId { get { return _ModuleId; } }
        public override string ModuleName { get { return "Plugins"; } }

        public enum EventTypeIds
        {
            InitializingPlugins = 10,
            InitializingPluginAssembly,
            InitializedPlugin,
            InitializedPluginFeature,
            InitializeWarning = 15,
            InitializeError,
            InitializeException,
            InitializeExceptionWithInner,
            PluginException = 20,
            PluginExceptionWithInner,
            PluginReferenceAssemblyLoaded = 50,
            PluginConfigurationLoaded = 100,
            PluginConfigurationSaved = 104,
            PluginWebControllerAccessed = 200
        }

        public static PluginsLog Current
        {
            get
            {
                return (PluginsLog)LogContext.LogModules[_ModuleId];
            }
        }
        private static void Log(EventTypeIds EventTypeId, params object[] Args)
        {
            Current.Log((int)EventTypeId, Args);
        }

        public static void LogInitializingPlugins(string PluginDirectory)
        {
            Current.Log((int)EventTypeIds.InitializingPlugins, PluginDirectory);
        }
        public static void LogInitializingPluginAssembly(Assembly PluginAssembly)
        {
            Current.Log((int)EventTypeIds.InitializingPluginAssembly, PluginAssembly.FullName, PluginAssembly.Location);
        }
        public static void LogInitializedPlugin(PluginManifest Menifest)
        {
            Current.Log((int)EventTypeIds.InitializedPlugin, Menifest.Id, Menifest.Version.ToString(3), Menifest.Type.Name, Menifest.Type.Assembly.Location);
        }
        public static void LogInitializedPluginFeature(PluginManifest PluginMenifest, PluginFeatureManifest FeatureManifest)
        {
            Current.Log((int)EventTypeIds.InitializedPluginFeature, PluginMenifest.Id, FeatureManifest.Type.Name);
        }
        public static void LogInitializeWarning(string Warning)
        {
            Current.Log((int)EventTypeIds.InitializeWarning, Warning);
        }
        public static void LogInitializeError(string Error)
        {
            Current.Log((int)EventTypeIds.InitializeError, Error);
        }
        public static void LogPluginReferenceAssemblyLoaded(string AssemblyFullName, string AssemblyPath, string RequestedBy)
        {
            Current.Log((int)EventTypeIds.PluginReferenceAssemblyLoaded, AssemblyFullName, AssemblyPath, RequestedBy);
        }
        public static void LogPluginConfigurationLoaded(string PluginId, string UserId)
        {
            Current.Log((int)EventTypeIds.PluginConfigurationLoaded, PluginId, UserId);
        }
        public static void LogPluginConfigurationSaved(string PluginId, string UserId)
        {
            Current.Log((int)EventTypeIds.PluginConfigurationSaved, PluginId, UserId);
        }
        public static void LogPluginWebControllerAccessed(string PluginId, string PluginAction, string UserId)
        {
            Current.Log((int)EventTypeIds.PluginWebControllerAccessed, PluginId, PluginAction, UserId);
        }

        public static void LogInitializeException(string PluginFilename, Exception ex)
        {
            if (ex.InnerException != null)
            {
                Log(EventTypeIds.InitializeExceptionWithInner, PluginFilename, ex.GetType().Name, ex.Message, ex.StackTrace, ex.InnerException.GetType().Name, ex.InnerException.Message, ex.InnerException.StackTrace);
            }
            else
            {
                Log(EventTypeIds.InitializeException, PluginFilename, ex.GetType().Name, ex.Message, ex.StackTrace);
            }
        }

        public static void LogPluginException(string Component, Exception ex)
        {
            if (ex.InnerException != null)
            {
                Log(EventTypeIds.PluginExceptionWithInner, Component, ex.GetType().Name, ex.Message, ex.StackTrace, ex.InnerException.GetType().Name, ex.InnerException.Message, ex.InnerException.StackTrace);
            }
            else
            {
                Log(EventTypeIds.PluginException, Component, ex.GetType().Name, ex.Message, ex.StackTrace);
            }
        }

        protected override List<Logging.Models.LogEventType> LoadEventTypes()
        {
            return new System.Collections.Generic.List<LogEventType>
			{
				new LogEventType
				{
					Id = (int)EventTypeIds.InitializingPlugins, 
					ModuleId = _ModuleId, 
					Name = "Initializing Plugins", 
					Format = "Starting plugin discovery and initialization from: {0}", 
					Severity =  (int)LogEventType.Severities.Information, 
					UseLive = false, 
					UsePersist = true, 
					UseDisplay = true
				}, 
				new LogEventType
				{
					Id = (int)EventTypeIds.InitializingPluginAssembly, 
					ModuleId = _ModuleId, 
					Name = "Initializing Plugin Assembly", 
					Format = "Initializing Plugin Assembly: [{0}] From '{1}'", 
					Severity = (int)LogEventType.Severities.Information, 
					UseLive = false, 
					UsePersist = true, 
					UseDisplay = true
				}, 
				new LogEventType
				{
					Id = (int)EventTypeIds.InitializedPlugin, 
					ModuleId = _ModuleId, 
					Name = "Initialized Plugin", 
					Format = "Initialized Plugin: '{0} (v{1})' [{2}] From '{3}'", 
					Severity = (int)LogEventType.Severities.Information, 
					UseLive = false, 
					UsePersist = true, 
					UseDisplay = true
				}, 
                new LogEventType
				{
					Id = (int)EventTypeIds.InitializedPluginFeature, 
					ModuleId = _ModuleId, 
					Name = "Initialized Plugin Feature", 
					Format = "Initialized Plugin Feature: '{1}' From '{0}'", 
					Severity = (int)LogEventType.Severities.Information, 
					UseLive = false, 
					UsePersist = true, 
					UseDisplay = true
				}, 
				new LogEventType
				{
					Id = (int)EventTypeIds.InitializeWarning, 
					ModuleId = _ModuleId, 
					Name = "Initialize Warning", 
					Format = "Initialize Warning: {0}", 
					Severity = (int)LogEventType.Severities.Warning, 
					UseLive = false, 
					UsePersist = true, 
					UseDisplay = true
				}, 
				new LogEventType
				{
					Id = (int)EventTypeIds.InitializeError, 
					ModuleId = _ModuleId, 
					Name = "Initialize Error", 
					Format = "Initialize Error: {0}", 
					Severity = (int)LogEventType.Severities.Error, 
					UseLive = false, 
					UsePersist = true, 
					UseDisplay = true
				}, 
				new LogEventType
				{
					Id = (int)EventTypeIds.InitializeException, 
					ModuleId = _ModuleId, 
					Name = "Initialize Exception", 
					Format = "Exception: {0}; {1}: {2}; {3}", 
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
					Format = "Exception: {0}; {1}: {2}; {3}; Inner: {4}: {5}; {6}", 
					Severity = (int)LogEventType.Severities.Error, 
					UseLive = false, 
					UsePersist = true, 
					UseDisplay = true
				}, 
				new LogEventType
				{
					Id = (int)EventTypeIds.PluginException, 
					ModuleId = _ModuleId, 
					Name = "Plugin Exception", 
					Format = "Exception: {0}; {1}: {2}; {3}", 
					Severity = (int)LogEventType.Severities.Error, 
					UseLive = true, 
					UsePersist = true, 
					UseDisplay = true
				}, 
				new LogEventType
				{
					Id = (int)EventTypeIds.PluginExceptionWithInner, 
					ModuleId = _ModuleId, 
					Name = "Plugin Exception with Inner Exception", 
					Format = "Exception: {0}; {1}: {2}; {3}; Inner: {4}: {5}; {6}", 
					Severity = (int)LogEventType.Severities.Error, 
					UseLive = true, 
					UsePersist = true, 
					UseDisplay = true
				}, 
				new LogEventType
				{
					Id = (int)EventTypeIds.PluginReferenceAssemblyLoaded, 
					ModuleId = _ModuleId, 
					Name = "Plugin Reference Assembly Loaded", 
					Format = "Loaded Plugin Reference Assembly: [{0}] From: '{1}'; Requested by: [{2}]", 
					Severity = (int)LogEventType.Severities.Information, 
					UseLive = true, 
					UsePersist = true, 
					UseDisplay = true
				}, 
				new LogEventType
				{
					Id = (int)EventTypeIds.PluginConfigurationLoaded, 
					ModuleId = _ModuleId, 
					Name = "Plugin Configuration Loaded", 
					Format = "Plugin Configuration Loaded: [{0}] by [{1}]", 
					Severity = (int)LogEventType.Severities.Information, 
					UseLive = true, 
					UsePersist = true, 
					UseDisplay = true
				}, 
				new LogEventType
				{
					Id = (int)EventTypeIds.PluginConfigurationSaved, 
					ModuleId = _ModuleId, 
					Name = "Plugin Configuration Saved", 
					Format = "Plugin Configuration Saved: [{0}] by [{1}]", 
					Severity = (int)LogEventType.Severities.Information, 
					UseLive = true, 
					UsePersist = true, 
					UseDisplay = true
				}, 
				new LogEventType
				{
					Id = (int)EventTypeIds.PluginWebControllerAccessed, 
					ModuleId = _ModuleId, 
					Name = "Plugin Web Controller Accessed", 
					Format = "Plugin Web Controller Accessed: Plugin [{0}], Action [{1}], By [{2}]", 
					Severity = (int)LogEventType.Severities.Information, 
					UseLive = true, 
					UsePersist = true, 
					UseDisplay = true
				}
			};
        }
    }
}
