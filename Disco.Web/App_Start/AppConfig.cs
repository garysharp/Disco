using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Disco.Data.Repository;

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

        public static void InitalizeEnvironment(DiscoDataContext dbContext)
        {
            // Initialize Logging
            Disco.Services.Logging.LogContext.Initalize(dbContext, DiscoApplication.SchedulerFactory);

            // Load Organisation Name
            DiscoApplication.OrganisationName = dbContext.DiscoConfiguration.OrganisationName;
            DiscoApplication.MultiSiteMode = dbContext.DiscoConfiguration.MultiSiteMode;

            // Setup Global Proxy
            DiscoApplication.SetGlobalProxy(dbContext.DiscoConfiguration.ProxyAddress,
                dbContext.DiscoConfiguration.ProxyPort,
                dbContext.DiscoConfiguration.ProxyUsername,
                dbContext.DiscoConfiguration.ProxyPassword);

            // Initialize Expressions
            BI.Expressions.Expression.InitializeExpressions();

            // Initialize Warranty Providers Plugins
            Disco.Services.Plugins.Plugins.InitalizePlugins(dbContext);

            // Initialize Scheduled Tasks
            Disco.Services.Tasks.ScheduledTasks.InitalizeScheduledTasks(dbContext, DiscoApplication.SchedulerFactory, true);

            // Schedule Immediate Check for Update (if never updated, or last updated over 2 days ago)
            if (dbContext.DiscoConfiguration.UpdateLastCheck == null ||
                dbContext.DiscoConfiguration.UpdateLastCheck.ResponseTimestamp < DateTime.Now.AddDays(-2))
            {
                Disco.BI.Interop.Community.UpdateCheckTask.ScheduleNow();
            }

            // Setup Attachment Monitor
            DiscoApplication.DocumentDropBoxMonitor = new BI.DocumentTemplateBI.Importer.DocumentDropBoxMonitor(dbContext, DiscoApplication.SchedulerFactory, HttpContext.Current.Cache);

            DiscoApplication.DocumentDropBoxMonitor.StartWatching();
            DiscoApplication.DocumentDropBoxMonitor.ScheduleCurrentFiles(10);

            // Enable SignalR-based Repository Notifications
            Disco.BI.Interop.SignalRHandlers.RepositoryMonitorNotifications.Initialize();
        }

        public static void InitializeUpdateEnvironment(DiscoDataContext dbContext)
        {
            // Initialize Logging
            Disco.Services.Logging.LogContext.Initalize(dbContext, DiscoApplication.SchedulerFactory);

            // Load Organisation Name
            DiscoApplication.OrganisationName = dbContext.DiscoConfiguration.OrganisationName;
            DiscoApplication.MultiSiteMode = dbContext.DiscoConfiguration.MultiSiteMode;

            // Setup Global Proxy
            DiscoApplication.SetGlobalProxy(dbContext.DiscoConfiguration.ProxyAddress,
                dbContext.DiscoConfiguration.ProxyPort,
                dbContext.DiscoConfiguration.ProxyUsername,
                dbContext.DiscoConfiguration.ProxyPassword);

            // Initialize Scheduled Tasks
            Disco.Services.Tasks.ScheduledTasks.InitalizeScheduledTasks(dbContext, DiscoApplication.SchedulerFactory, true);
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