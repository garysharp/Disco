using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Disco.Data.Repository;
using System.IO;
using System.Management;
using System.Diagnostics;
using System.Data.SqlServerCe;
using System.Data.EntityClient;
using System.Data.Entity;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Triggers;
using Newtonsoft.Json;

namespace Disco.Services.Logging
{
    public class LogContext
    {
        public static Dictionary<int, LogBase> LogModules { get; private set; }
        private static object _LogModulesLock = new object();

        private static LogContext _Current;
        private static object _CurrentLock = new Object();
        public static LogContext Current
        {
            get
            {
                lock (_CurrentLock)
                {
                    if (_Current == null)
                        throw new InvalidOperationException("Logging Context has not been Initialized");
                    return _Current;
                }
            }
            private set
            {
                lock (_CurrentLock)
                {
                    _Current = value;
                }
            }
        }

        private static void InitalizeModules()
        {
            if (LogModules == null)
            {
                lock (_LogModulesLock)
                {
                    if (LogModules == null)
                    {
                        LogModules = new Dictionary<int, LogBase>();
                        // Load all LogModules (Only from Disco Assemblies)
                        var appDomain = AppDomain.CurrentDomain;

                        var logModuleTypes = (from a in appDomain.GetAssemblies()
                                              where !a.GlobalAssemblyCache && !a.IsDynamic && a.FullName.StartsWith("Disco.", StringComparison.InvariantCultureIgnoreCase)
                                              from type in a.GetTypes()
                                              where typeof(LogBase).IsAssignableFrom(type) && !type.IsAbstract
                                              select type);
                        foreach (var logModuleType in logModuleTypes)
                        {
                            var instance = (LogBase)Activator.CreateInstance(logModuleType);
                            LogModules[instance.ModuleId] = instance;
                        }
                    }
                }
            }
        }

        private static void InitalizeDatabase(Targets.LogPersistContext logDbContext)
        {
            // Add Modules
            var existingModules = logDbContext.Modules.Include("EventTypes").ToDictionary(m => m.Id);
            foreach (var module in LogModules)
            {
                // Update/Insert Module
                Models.LogModule dbModule;
                if (existingModules.TryGetValue(module.Key, out dbModule))
                {
                    // Update
                    if (dbModule.Name != module.Value.ModuleName)
                        dbModule.Name = module.Value.ModuleName;
                    if (dbModule.Description != module.Value.ModuleDescription)
                        dbModule.Description = module.Value.ModuleDescription;
                }
                else
                {
                    // Insert
                    dbModule = new Models.LogModule()
                    {
                        Id = module.Key,
                        Name = module.Value.ModuleName,
                        Description = module.Value.ModuleDescription
                    };
                    logDbContext.Modules.Add(dbModule);
                }
                // Update/Insert Event Types
                Dictionary<int, Models.LogEventType> existingEventTypes = (dbModule.EventTypes == null) ? new Dictionary<int, Models.LogEventType>() : dbModule.EventTypes.ToDictionary(et => et.Id);
                foreach (var eventType in module.Value.EventTypes)
                {
                    Models.LogEventType dbEventType;
                    if (existingEventTypes.TryGetValue(eventType.Key, out dbEventType))
                    {
                        // Update
                        if (dbEventType.Name != eventType.Value.Name)
                            dbEventType.Name = eventType.Value.Name;
                        if (dbEventType.Severity != eventType.Value.Severity)
                            dbEventType.Severity = eventType.Value.Severity;
                        if (dbEventType.Format != eventType.Value.Format)
                            dbEventType.Format = eventType.Value.Format;
                    }
                    else
                    {
                        // Insert
                        dbEventType = new Models.LogEventType()
                        {
                            Id = eventType.Key,
                            ModuleId = module.Key,
                            Name = eventType.Value.Name,
                            Severity = eventType.Value.Severity,
                            Format = eventType.Value.Format
                        };
                        logDbContext.EventTypes.Add(dbEventType);
                    }
                }
            }

            logDbContext.SaveChanges();
        }

        public static string LogFileBasePath(DiscoDataContext DiscoContext)
        {
            var logDirectoryBase = Path.Combine(DiscoContext.DiscoConfiguration.DataStoreLocation, "Logs");
            // Create Directory Structure
            if (!Directory.Exists(logDirectoryBase))
            {
                Directory.CreateDirectory(logDirectoryBase);
            }
            // Ensure Logs are NTFS Compressed - TODO...
            //Utilities.CompressDirectory(logDirectory);
            // WMI - Doesn't Work for Network Folders...
            //var logDirectoryBaseInfo = new DirectoryInfo(logDirectoryBase);
            //if ((logDirectoryBaseInfo.Attributes & FileAttributes.Compressed) != FileAttributes.Compressed)
            //{
            //    var logDirectoryWmiPath = string.Format("Win32_Directory.Name=\"{0}\"", logDirectoryBase);
            //    using (ManagementObject logDirectoryBaseMO = new ManagementObject(logDirectoryWmiPath))
            //    {
            //        ManagementBaseObject outParams = logDirectoryBaseMO.InvokeMethod("Compress", null, null);
            //        Debug.WriteLine("LoggingContext.InitalizeCurrent: Compressing Log Folder; Result: " + outParams.Properties["ReturnValue"].Value.ToString());
            //    }
            //}
            return logDirectoryBase;
        }

        public static string LogFilePath(DiscoDataContext DiscoContext, DateTime Date, bool CreateDirectory = true)
        {
            var logDirectoryBase = LogFileBasePath(DiscoContext);
            var logDirectory = Path.Combine(logDirectoryBase, Date.Year.ToString());
            if (CreateDirectory && !Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }
            var logFileName = string.Format("DiscoLog_{0:yyy-MM-dd}.sdf", Date);
            return Path.Combine(logDirectory, logFileName);
        }

        internal static void ReInitalize(DiscoDataContext DiscoContext)
        {
            lock (_CurrentLock)
            {
                var logPath = LogFilePath(DiscoContext, DateTime.Today);

                //var connectionString = string.Format("Data Source=\"{0}\"", logPath);

                SqlCeConnectionStringBuilder sqlCeCSB = new SqlCeConnectionStringBuilder();
                sqlCeCSB.DataSource = logPath;
                var connectionString = sqlCeCSB.ToString();

                // Ensure Database Exists
                if (!File.Exists(logPath))
                {
                    // Create Database
                    using (var context = new Targets.LogPersistContext(connectionString))
                    {
                        context.Database.CreateIfNotExists();
                    }
                }

                // Add Modules/Event Types
                InitalizeModules();
                using (var context = new Targets.LogPersistContext(connectionString))
                {
                    InitalizeDatabase(context);
                }

                // Create Current LogContext
                var currentLogContext = new LogContext(logPath, connectionString);
                _Current = currentLogContext;
            }
            SystemLog.LogLogInitialized(_Current.PersistantStorePath);
            try
            {
                // Get Yesterdays Log
                var yesterdaysLogPath = LogFilePath(DiscoContext, DateTime.Today.AddDays(-1), false);
                if (File.Exists(yesterdaysLogPath))
                {
                    SqlCeConnectionStringBuilder sqlCeCSB = new SqlCeConnectionStringBuilder();
                    sqlCeCSB.DataSource = yesterdaysLogPath;
                    var connectionString = sqlCeCSB.ToString();
                    int logCount;
                    using (var context = new Targets.LogPersistContext(connectionString))
                    {
                        logCount = context.Events.Where(e => !(e.ModuleId == 0 && e.EventTypeId == 100)).Count();
                        if (logCount == 0)
                        {
                            // Delete (empty) Database
                            context.Database.Delete();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SystemLog.LogError("Error occurred while investigating yesterdays log for deletion", ex.GetType().Name, ex.Message, ex.StackTrace);
            }
        }

        private static IScheduler _ReInitializeScheduler;
        public static void Initalize(DiscoDataContext DiscoContext, ISchedulerFactory SchedulerFactory)
        {
            ReInitalize(DiscoContext);

            _ReInitializeScheduler = SchedulerFactory.GetScheduler();

            var reInitalizeJobDetail = new JobDetailImpl("DiscoLogContextReinialize", typeof(LogReInitalizeJob));

            // Simple Trigger - Issue with Day light savings
            //var reInitalizeTrigger = TriggerBuilder.Create()
            //    .WithIdentity("DiscoLogContextReinializeTrigger")
            //    .StartAt(DateBuilder.TomorrowAt(0,0,0))
            //    .WithSchedule(SimpleScheduleBuilder.Create().WithIntervalInHours(24).RepeatForever())
            //    .Build();
            // Use Cron Schedule instead
            var reInitalizeTrigger = TriggerBuilder.Create()
                .WithIdentity("DiscoLogContextReinializeTrigger")
                .StartNow()
                .WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(0, 0)) // Midnight
                .Build();

            _ReInitializeScheduler.ScheduleJob(reInitalizeJobDetail, reInitalizeTrigger);
        }
        public static string LiveLogAllEventsGroupName
        {
            get
            {
                return Targets.LogLiveContext.LiveLogNameAll;
            }
        }

        private LogContext(string PersistantStorePath, string PersistantStoreConnectionString)
        {
            this.PersistantStorePath = PersistantStorePath;
            this.PersistantStoreConnectionString = PersistantStoreConnectionString;
        }

        public string PersistantStorePath { get; private set; }
        public string PersistantStoreConnectionString { get; private set; }

        public void Log(int ModuleId, int EventTypeId, params object[] Args)
        {
            LogBase logModule;
            if (LogModules.TryGetValue(ModuleId, out logModule))
            {
                Models.LogEventType eventType;
                if (logModule.EventTypes.TryGetValue(EventTypeId, out eventType))
                {
                    var eventTimestamp = DateTime.Now;
                    if (eventType.UseLive)
                    {
                        Targets.LogLiveContext.Broadcast(logModule, eventType, eventTimestamp, Args);
                    }
                    if (eventType.UsePersist)
                    {
                        string args = null;
                        if (Args != null && Args.Length > 0)
                        { //args = fastJSON.JSON.Instance.ToJSON(Args, false); // Old fastJSON Implementation
                            args = JsonConvert.SerializeObject(Args);
                        }
                        using (var context = new Targets.LogPersistContext(PersistantStoreConnectionString))
                        {
                            var e = new Models.LogEvent()
                            {
                                Timestamp = eventTimestamp,
                                ModuleId = logModule.ModuleId,
                                EventTypeId = eventType.Id,
                                Arguments = args
                            };
                            context.Events.Add(e);
                            context.SaveChanges();
                        }
                    }
                }
                else
                    throw new InvalidOperationException(string.Format("Unknown Log Event Type Called: {0} (for Module: {1})", EventTypeId, ModuleId));
            }
            else
                throw new InvalidOperationException(string.Format("Unknown Log Module Called: {0}", ModuleId));
        }

    }
}
