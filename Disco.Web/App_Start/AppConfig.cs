using Disco.Data.Repository;
using Disco.Services;
using Disco.Services.Interop.DiscoServices;
using System;
using System.Linq;

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

        public static void InitalizeCoreEnvironment(DiscoDataContext Database)
        {
            // Initialize Logging
            Disco.Services.Logging.LogContext.Initalize(Database, DiscoApplication.SchedulerFactory);

            // Load Organisation Name
            DiscoApplication.DeploymentId = Database.DiscoConfiguration.DeploymentId;
            DiscoApplication.OrganisationName = Database.DiscoConfiguration.OrganisationName;
            DiscoApplication.MultiSiteMode = Database.DiscoConfiguration.MultiSiteMode;

            // Initialize Active Directory Interop
            Disco.Services.Interop.ActiveDirectory.ActiveDirectory.Initialize(Database);

            // Setup Global Proxy
            DiscoApplication.SetGlobalProxy(Database.DiscoConfiguration.ProxyAddress,
                Database.DiscoConfiguration.ProxyPort,
                Database.DiscoConfiguration.ProxyUsername,
                Database.DiscoConfiguration.ProxyPassword);

            // Initialize User Service Interop
            Disco.Services.Users.UserService.Initialize(Database);
        }

        public static void InitalizeNormalEnvironment(DiscoDataContext Database)
        {
            InitalizeCoreEnvironment(Database);

            // Initialize Expressions
            Disco.Services.Expressions.Expression.InitializeExpressions();

            // Initialize Job Queues
            Disco.Services.Jobs.JobQueues.JobQueueService.Initialize(Database);

            // Initialize User Flags
            Disco.Services.Users.UserFlags.UserFlagService.Initialize(Database);

            // Initialize Satellite Managed Groups (which don't belong to any other component)
            Disco.Services.Devices.ManagedGroups.DeviceManagedGroups.Initialize(Database);
            Disco.Services.Documents.ManagedGroups.DocumentTemplateManagedGroups.Initialize(Database);

            // Initialize Plugins
            Disco.Services.Plugins.Plugins.InitalizePlugins(Database);

            // Initialize Scheduled Tasks
            Disco.Services.Tasks.ScheduledTasks.InitalizeScheduledTasks(Database, DiscoApplication.SchedulerFactory, true);

            // Schedule Immediate Check for Update (if never updated, or last updated over 2 days ago)
            if (Database.DiscoConfiguration.UpdateLastCheckResponse == null ||
                Database.DiscoConfiguration.UpdateLastCheckResponse.UpdateResponseDate < DateTime.Now.AddDays(-2))
            {
                UpdateQueryTask.ScheduleNow();
            }

            // Setup Attachment Monitor
            var dropboxLocation = DataStore.CreateLocation(Database, "DocumentDropBox");
            DiscoApplication.DocumentDropBoxMonitor = new Services.Documents.AttachmentImport.ImportDirectoryMonitor(dropboxLocation, DiscoApplication.SchedulerFactory.GetScheduler(), 5000);

            DiscoApplication.DocumentDropBoxMonitor.Start();
            DiscoApplication.DocumentDropBoxMonitor.ScheduleCurrentFiles(10000); // 10 Second Delay
        }

        public static void InitializeUpdateEnvironment(DiscoDataContext Database, Version PreviousVersion)
        {
            InitalizeCoreEnvironment(Database);

            // Initialize Scheduled Tasks
            Disco.Services.Tasks.ScheduledTasks.InitalizeScheduledTasks(Database, DiscoApplication.SchedulerFactory, false);

            // Import MAC Address Migration
            if (PreviousVersion != null && PreviousVersion < new Version(1, 2, 910, 0))
                Services.Devices.Enrolment.LogMacAddressImportingTask.SetRequired(Database);

            // Attachment PDF Thumbnail Update
            if (PreviousVersion != null && PreviousVersion < new Version(2, 2, 0, 0))
                Services.Documents.AttachmentImport.ThumbnailUpdateTask.SetRequired(Database);

            // AD Device Description Update
            if (PreviousVersion != null && PreviousVersion < new Version(2, 2, 16281, 0))
                Services.Interop.ActiveDirectory.ADDeviceDescriptionUpdateTask.SetRequired(Database);
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