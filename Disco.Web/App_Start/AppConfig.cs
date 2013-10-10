using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Disco.Data.Repository;
using System.Threading;
using System.Reflection;

namespace Disco.Web
{
    public static class AppConfig
    {
        public static bool InitializeDatabase()
        {
            // Modified Connection Factory
            System.Data.Entity.Database.DefaultConnectionFactory = new DiscoDatabaseConnectionFactory(System.Data.Entity.Database.DefaultConnectionFactory);

            if (Disco.Data.Repository.DiscoDatabaseConnectionFactory.DiscoDataContextConnectionString == null)
            {
                // Database Connection String not configured - Trigger 'Install'
                return false;
            }

            // Migrate Database
            Disco.Data.Migrations.DiscoDataMigrator.MigrateLatest(true);

            return true;
        }

        private static void InitalizeEnvironment(DiscoDataContext Database)
        {
            // Initialize Logging
            Disco.Services.Logging.LogContext.Initalize(Database, DiscoApplication.SchedulerFactory);

            // Load Organisation Name
            DiscoApplication.OrganisationName = Database.DiscoConfiguration.OrganisationName;
            DiscoApplication.MultiSiteMode = Database.DiscoConfiguration.MultiSiteMode;

            // Setup Global Proxy
            DiscoApplication.SetGlobalProxy(Database.DiscoConfiguration.ProxyAddress,
                Database.DiscoConfiguration.ProxyPort,
                Database.DiscoConfiguration.ProxyUsername,
                Database.DiscoConfiguration.ProxyPassword);

            // Initialize User Service Interop
            Disco.Services.Users.UserService.Initialize(Database,
                (UserId, AdditionalProperties) => Disco.BI.Interop.ActiveDirectory.ActiveDirectory.GetUserAccount(UserId, AdditionalProperties),
                (UserId, AdditionalProperties) => Disco.BI.Interop.ActiveDirectory.ActiveDirectory.GetMachineAccount(UserId, null, null, AdditionalProperties));

        }

        public static void InitalizeNormalEnvironment(DiscoDataContext Database)
        {
            InitalizeEnvironment(Database);

            // Initialize Expressions
            BI.Expressions.Expression.InitializeExpressions();

            // Initialize Warranty Providers Plugins
            Disco.Services.Plugins.Plugins.InitalizePlugins(Database);

            // Initialize Scheduled Tasks
            Disco.Services.Tasks.ScheduledTasks.InitalizeScheduledTasks(Database, DiscoApplication.SchedulerFactory, true);

            // Schedule Immediate Check for Update (if never updated, or last updated over 2 days ago)
            if (Database.DiscoConfiguration.UpdateLastCheck == null ||
                Database.DiscoConfiguration.UpdateLastCheck.ResponseTimestamp < DateTime.Now.AddDays(-2))
            {
                Disco.BI.Interop.Community.UpdateCheckTask.ScheduleNow();
            }

            // Setup Attachment Monitor
            DiscoApplication.DocumentDropBoxMonitor = new BI.DocumentTemplateBI.Importer.DocumentDropBoxMonitor(Database, DiscoApplication.SchedulerFactory, HttpContext.Current.Cache);

            DiscoApplication.DocumentDropBoxMonitor.StartWatching();
            DiscoApplication.DocumentDropBoxMonitor.ScheduleCurrentFiles(10);

            // Enable SignalR-based Repository Notifications
            Disco.BI.Interop.SignalRHandlers.RepositoryMonitorNotifications.Initialize();
        }

        public static void InitializeUpdateEnvironment(DiscoDataContext Database, Version PreviousVersion)
        {
            InitalizeEnvironment(Database);

            // Initialize Scheduled Tasks
            Disco.Services.Tasks.ScheduledTasks.InitalizeScheduledTasks(Database, DiscoApplication.SchedulerFactory, true);

            // Import MAC Address Migration
            if (PreviousVersion != null && PreviousVersion < new Version(1, 2, 910, 0))
                Disco.BI.DeviceBI.Migration.LogMacAddressImporting.SetRequired(Database);
        }

        public static void DisposeEnvironment()
        {
            if (DiscoApplication.DocumentDropBoxMonitor != null)
                DiscoApplication.DocumentDropBoxMonitor.Dispose();

            if (DiscoApplication.SchedulerFactory != null)
            {
                foreach (var item in DiscoApplication.SchedulerFactory.AllSchedulers.ToArray())
                {
                    item.Shutdown(false);
                }
            }

            Disco.Services.Logging.SystemLog.LogUninitialized();
        }
    }
}