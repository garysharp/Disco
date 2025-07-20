using Disco.Data.Repository;
using Disco.Services.Interop.DiscoServices;
using Disco.Services.Web;
using System;
using System.Configuration;
using System.Net;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;

namespace Disco.Web
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class DiscoApplication : System.Web.HttpApplication
    {
        public DiscoApplication()
        {
            base.BeginRequest += new EventHandler(DiscoApplication_BeginRequest);
            base.Error += new EventHandler(DiscoApplication_Error);
        }

        protected void Application_Start()
        {
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

            if (AppConfig.InitializeDatabase())
            {
                // Database Initialized

                using (DiscoDataContext database = new DiscoDataContext())
                {
                    // Check for Post-Update
                    var previousVersion = database.DiscoConfiguration.InstalledDatabaseVersion;
                    bool isVersionUpdate = previousVersion != UpdateQuery.CurrentDiscoVersion();
                    bool ignoreVersionUpdate = false;

                    if (isVersionUpdate)
                    {
                        // Update Database with New Version
                        database.DiscoConfiguration.InstalledDatabaseVersion = UpdateQuery.CurrentDiscoVersion();
                        database.SaveChanges();

                        // Check if configured to Ignore Plugin Updates (Mainly for Dev environment)
                        bool.TryParse(ConfigurationManager.AppSettings["DiscoIgnoreVersionUpdate"], out ignoreVersionUpdate);
                        // Only Update if Plugins are installed
                        if (!ignoreVersionUpdate)
                            ignoreVersionUpdate = (Disco.Services.Plugins.UpdatePluginTask.OfflineInstalledPlugins(database).Count == 0);
                    }

                    if (!isVersionUpdate || ignoreVersionUpdate)
                    {
                        // Normal Startup

                        AreaRegistration.RegisterAllAreas();

                        FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);

                        RouteConfig.RegisterRoutes(RouteTable.Routes);

                        BundleConfig.RegisterBundles();

                        AppConfig.InitalizeNormalEnvironment(database);
                    }
                    else
                    {
                        // Post-Update Startup
                        FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
                        RouteConfig.RegisterUpdateRoutes(RouteTable.Routes);
                        BundleConfig.RegisterBundles();
                        AppConfig.InitializeUpdateEnvironment(database, previousVersion);
                    }
                }
            }
            else
            {
                // Database Not Initialized
                // Install
                InitialConfig = true;
                FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
                RouteConfig.RegisterInstallRoutes(RouteTable.Routes);
                BundleConfig.RegisterBundles();
            }
        }
        protected void Application_End()
        {
            AppConfig.DisposeEnvironment();
        }

        void DiscoApplication_BeginRequest(object sender, EventArgs e)
        {
            // Force Disable IE Compatibility Mode
            Response.Headers.Add("X-UA-Compatible", "IE=edge");
        }

        #region Authentication

        public static bool InitialConfig { get; set; }

        #endregion
        #region Cached Properties

        private static string _OrganisationName;
        public static string OrganisationName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_OrganisationName))
                {
                    return "Unknown";
                }
                return _OrganisationName;
            }
            set
            {
                _OrganisationName = value;
            }
        }

        public static string DeploymentId { get; set; }

        public static bool MultiSiteMode { get; set; }

        #region Version
        private static Lazy<string> _Version = new Lazy<string>(() =>
        {
            var AssemblyVersion = typeof(DiscoApplication).Assembly.GetName().Version;
            return $"{AssemblyVersion.Major}.{AssemblyVersion.Minor}.{AssemblyVersion.Build:0000}.{AssemblyVersion.Revision:0000}";
        });
        public static string Version
        {
            get
            {
                return _Version.Value;
            }
        }
        #endregion

        #endregion
        #region Proxy
        private static IWebProxy _defaultProxy;
        public static void SetGlobalProxy(string Address, int Port, string Username, string Password)
        {
            if (_defaultProxy == null)
            {
                _defaultProxy = WebRequest.DefaultWebProxy;
            }
            if (!string.IsNullOrWhiteSpace(Address))
            {
                WebProxy p = new WebProxy(Address, Port);
                p.BypassProxyOnLocal = true;
                if (!string.IsNullOrWhiteSpace(Username))
                {
                    p.Credentials = new NetworkCredential(Username, Password);
                }
                else
                {
                    // Added 2013-02-08 G#
                    // Improve support for Integrated Windows Authentication
                    p.UseDefaultCredentials = true;
                }
                WebRequest.DefaultWebProxy = p;
            }
            else
            {
                WebRequest.DefaultWebProxy = _defaultProxy;
            }
        }
        #endregion
        #region Scheduler Factory
        private static Lazy<Quartz.ISchedulerFactory> _SchedulerFactory = new Lazy<Quartz.ISchedulerFactory>(() =>
        {
            // Initialize Scheduler Factory
            return new Quartz.Impl.StdSchedulerFactory();
        });
        public static Quartz.ISchedulerFactory SchedulerFactory
        {
            get
            {
                return _SchedulerFactory.Value;
            }
        }
        #endregion
        #region Dropbox Monitor
        public static Services.Documents.AttachmentImport.ImportDirectoryMonitor DocumentDropBoxMonitor { get; set; }
        #endregion
        #region Global Error Logging
        void DiscoApplication_Error(object sender, EventArgs e)
        {
            this.HandleException();
        }
        #endregion
    }
}