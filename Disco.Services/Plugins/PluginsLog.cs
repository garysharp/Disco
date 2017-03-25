using System;
using System.Collections.Generic;
using Disco.Services.Logging;
using Disco.Services.Logging.Models;
using System.Reflection;
using Exceptionless;

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
            PluginWarning = 30,
            PluginMessage = 40,
            PluginReferenceAssemblyLoaded = 50,
            PluginConfigurationLoaded = 100,
            PluginConfigurationSaved = 104,
            PluginWebControllerAccessed = 200,

            Installing = 500,
            Installed = 550,
            BeforeUpdate = 600,
            AfterUpdate = 700,
            Uninstalling = 800,
            Uninstalled = 850
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

        public static void LogInstalling(PluginManifest Manifest)
        {
            Current.Log((int)EventTypeIds.Installing, Manifest.Id, Manifest.Version.ToString(4), Manifest.Name);
        }
        public static void LogInstalled(PluginManifest Manifest)
        {
            Current.Log((int)EventTypeIds.Installing, Manifest.Id, Manifest.Version.ToString(4), Manifest.Name, Manifest.PluginLocation);
        }
        public static void LogBeforeUpdate(PluginManifest ExistingManifest, PluginManifest UpdateManifest)
        {
            Current.Log((int)EventTypeIds.BeforeUpdate, ExistingManifest.Id, ExistingManifest.Name, ExistingManifest.PluginLocation, ExistingManifest.Version.ToString(4), UpdateManifest.Version.ToString(4));
        }
        public static void LogAfterUpdate(PluginManifest ExistingManifest, PluginManifest UpdateManifest)
        {
            Current.Log((int)EventTypeIds.AfterUpdate, UpdateManifest.Id, UpdateManifest.Name, UpdateManifest.PluginLocation, ExistingManifest.Version.ToString(4), UpdateManifest.Version.ToString(4));
        }
        public static void LogUninstalling(PluginManifest Manifest, bool UninstallData)
        {
            Current.Log((int)EventTypeIds.Uninstalling, Manifest.Id, Manifest.Name, Manifest.PluginLocation, Manifest.Version.ToString(4), UninstallData);
        }
        public static void LogUninstalled(PluginManifest Manifest, bool UninstalledData)
        {
            Current.Log((int)EventTypeIds.Uninstalled, Manifest.Id, Manifest.Name, Manifest.PluginLocation, Manifest.Version.ToString(4), UninstalledData);
        }

        public static void LogInitializeException(string PluginFilename, Exception ex)
        {
            ex.ToExceptionless().AddObject(PluginFilename, "PluginFilename").Submit();

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
            ex.ToExceptionless().AddObject(Component, "Component").Submit();

            if (ex.InnerException != null)
            {
                Log(EventTypeIds.PluginExceptionWithInner, Component, ex.GetType().Name, ex.Message, ex.StackTrace, ex.InnerException.GetType().Name, ex.InnerException.Message, ex.InnerException.StackTrace);
            }
            else
            {
                Log(EventTypeIds.PluginException, Component, ex.GetType().Name, ex.Message, ex.StackTrace);
            }
        }
        public static void LogPluginWarning(PluginManifest Manifest, string Message, params object[] ExportData)
        {
            LogPluginWarningOrMessage(EventTypeIds.PluginWarning, Manifest, Message, ExportData);
        }
        public static void LogPluginMessage(PluginManifest Manifest, string Message, params object[] ExportData)
        {
            LogPluginWarningOrMessage(EventTypeIds.PluginMessage, Manifest, Message, ExportData);
        }
        private static void LogPluginWarningOrMessage(EventTypeIds WarningOrMessage, PluginManifest Manifest, string Message, object[] ExportData)
        {
            if (WarningOrMessage != EventTypeIds.PluginMessage && WarningOrMessage != EventTypeIds.PluginWarning)
                throw new ArgumentException("Only PluginMessage/PluginWarning is allowed", "WarningOrMessage");

            object[] LogData;

            if (ExportData == null || ExportData.Length == 0)
            {
                LogData = new object[3];
            }
            else
            {
                LogData = new object[4 + ExportData.Length];
                for (int i = 0; i < ExportData.Length; i++)
                    LogData[4 + i] = ExportData[i];
            }

            LogData[0] = Manifest.Name;
            LogData[1] = Manifest.Id;
            LogData[2] = Manifest.VersionFormatted;
            LogData[3] = Message;

            Log(WarningOrMessage, LogData);
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
					Id = (int)EventTypeIds.PluginWarning, 
					ModuleId = _ModuleId, 
					Name = "Plugin Warning", 
					Format = "{0} [{1} v{2}]: {3}", 
					Severity = (int)LogEventType.Severities.Warning, 
					UseLive = true, 
					UsePersist = true, 
					UseDisplay = true
				}, 
                new LogEventType
				{
					Id = (int)EventTypeIds.PluginMessage, 
					ModuleId = _ModuleId, 
					Name = "Plugin Message", 
					Format = "{0} [{1} v{2}]: {3}", 
					Severity = (int)LogEventType.Severities.Information, 
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
				}, 
				new LogEventType
				{
					Id = (int)EventTypeIds.Installing, 
					ModuleId = _ModuleId, 
					Name = "Installing Plugin", 
					Format = "Installing Plugin: {2} [{0} v{1}]", 
					Severity = (int)LogEventType.Severities.Information, 
					UseLive = true, 
					UsePersist = true, 
					UseDisplay = true
				}, 
				new LogEventType
				{
					Id = (int)EventTypeIds.Installed, 
					ModuleId = _ModuleId, 
					Name = "Plugin Installed", 
					Format = "Plugin Installed: {2} [{0} v{1}], Location: {3}", 
					Severity = (int)LogEventType.Severities.Information, 
					UseLive = true, 
					UsePersist = true, 
					UseDisplay = true
				}, 
				new LogEventType
				{
					Id = (int)EventTypeIds.BeforeUpdate, 
					ModuleId = _ModuleId, 
					Name = "Updating Plugin", 
					Format = "Updating Plugin: {1} [{0}], v{3} -> v{4}, Location: {2}", 
					Severity = (int)LogEventType.Severities.Information, 
					UseLive = true, 
					UsePersist = true, 
					UseDisplay = true
				}, 
				new LogEventType
				{
					Id = (int)EventTypeIds.AfterUpdate, 
					ModuleId = _ModuleId, 
					Name = "Plugin Updated", 
					Format = "Plugin Updated: {1} [{0}], v{3} -> v{4}, Location: {2}", 
					Severity = (int)LogEventType.Severities.Information, 
					UseLive = false, 
					UsePersist = true, 
					UseDisplay = true
				}, 
				new LogEventType
				{
					Id = (int)EventTypeIds.Uninstalling, 
					ModuleId = _ModuleId, 
					Name = "Uninstalling Plugin", 
					Format = "Uninstalling Plugin: {1} [{0} v{3}], Location: {2}, UninstallData: {4}", 
					Severity = (int)LogEventType.Severities.Information, 
					UseLive = true, 
					UsePersist = true, 
					UseDisplay = true
				}, 
				new LogEventType
				{
					Id = (int)EventTypeIds.Uninstalled, 
					ModuleId = _ModuleId, 
					Name = "Plugin Uninstalled", 
					Format = "Plugin Uninstalled: {1} [{0} v{3}], Location: {2}, UninstallData: {4}", 
					Severity = (int)LogEventType.Severities.Information, 
					UseLive = false, 
					UsePersist = true, 
					UseDisplay = true
				}
			};
        }
    }
}
