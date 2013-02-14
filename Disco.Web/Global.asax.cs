using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using Disco.Data.Repository;
using Disco.Models.Repository;

namespace Disco.Web
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class DiscoApplication : System.Web.HttpApplication
    {
        public DiscoApplication()
        {
            base.AuthenticateRequest += new EventHandler(DiscoApplication_AuthenticateRequest);
            base.BeginRequest += new EventHandler(DiscoApplication_BeginRequest);
            base.Error += new EventHandler(DiscoApplication_Error);
        }

        protected void Application_Start()
        {
            var timer = new Stopwatch();
            long timer_last;
            timer.Start();


            Debug.WriteLine("Application Startup Profiling Started");
            timer_last = timer.ElapsedMilliseconds;

            if (AppConfig.InitializeDatabase())
            {
                // Database Initialized

                Debug.WriteLine("Initialized Database: +{0}ms", timer.ElapsedMilliseconds - timer_last);
                timer_last = timer.ElapsedMilliseconds;

                // Check for Post-Update
                Version previousVersion;
                Version currentVersion = Disco.BI.Interop.Community.UpdateCheck.CurrentDiscoVersion();
                using (DiscoDataContext dbContext = new DiscoDataContext())
                {
                    previousVersion = dbContext.DiscoConfiguration.InstalledDatabaseVersion;
                }
                if (currentVersion == previousVersion)
                {
                    // Normal Startup

                    AreaRegistration.RegisterAllAreas();

                    Debug.WriteLine("Registered Areas: +{0}ms", timer.ElapsedMilliseconds - timer_last);
                    timer_last = timer.ElapsedMilliseconds;

                    WebApiConfig.Register(GlobalConfiguration.Configuration);

                    Debug.WriteLine("Registered API: +{0}ms", timer.ElapsedMilliseconds - timer_last);
                    timer_last = timer.ElapsedMilliseconds;

                    FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);

                    Debug.WriteLine("Registered Global Filters: +{0}ms", timer.ElapsedMilliseconds - timer_last);
                    timer_last = timer.ElapsedMilliseconds;

                    RouteConfig.RegisterRoutes(RouteTable.Routes);

                    Debug.WriteLine("Registered Routes: +{0}ms", timer.ElapsedMilliseconds - timer_last);
                    timer_last = timer.ElapsedMilliseconds;

                    BundleConfig.RegisterBundles();

                    Debug.WriteLine("Registered Bundles: +{0}ms", timer.ElapsedMilliseconds - timer_last);
                    timer_last = timer.ElapsedMilliseconds;

                    AppConfig.InitalizeEnvironment();

                    Debug.WriteLine("Initialized Environment: +{0}ms", timer.ElapsedMilliseconds - timer_last);
                    timer_last = timer.ElapsedMilliseconds;
                }
                else
                {
                    // Post-Update Startup
                    AreaRegistration.RegisterAllAreas();
                    FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
                    RouteConfig.RegisterUpdateRoutes(RouteTable.Routes);
                    BundleConfig.RegisterBundles();
                    AppConfig.InitializeUpdateEnvironment();

                    using (DiscoDataContext dbContext = new DiscoDataContext())
                    {
                        dbContext.DiscoConfiguration.InstalledDatabaseVersion = currentVersion;
                        dbContext.SaveChanges();
                    }
                }
            }
            else
            {
                // Database Not Initialized
                // Install
                InitialConfig = true;
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
        void DiscoApplication_AuthenticateRequest(object sender, EventArgs e)
        {
            User u = CurrentUser;
            if (u != null)
            {
                base.Context.User = new GenericPrincipal(base.Context.User.Identity, new string[] { u.Type });
            }
        }

        public static bool InitialConfig { get; set; }

        public static User CurrentUser
        {
            get
            {
                if (!InitialConfig)
                    return BI.UserBI.UserCache.CurrentUser;
                else
                    return null;
            }
        }
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
        public static bool MultiSiteMode { get; set; }

        #region Version
        private static Lazy<string> _Version = new Lazy<string>(() =>
        {
            var AssemblyVersion = typeof(DiscoApplication).Assembly.GetName().Version;
            return string.Format("{0}.{1}.{2:0000}", AssemblyVersion.Major, AssemblyVersion.Minor, AssemblyVersion.Build);
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
        public static BI.DocumentTemplateBI.Importer.DocumentDropBoxMonitor DocumentDropBoxMonitor { get; set; }
        #endregion
        #region Global Error Logging
        void DiscoApplication_Error(object sender, EventArgs e)
        {
            try
            {
                Disco.Services.Logging.SystemLog.LogException("Global Application Exception Caught", Server.GetLastError());
            }
            catch (Exception)
            {
                // Ignore all logging errors
            }
        }
        #endregion
    }
}