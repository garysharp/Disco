using System;
using System.Data.SqlClient;
using System.Threading;
using System.Web.Mvc;
using Disco.Web.Models.InitialConfig;
using Disco.Data.Repository;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using System.IO.Compression;
using System.Management;
using System.Web;

namespace Disco.Web.Controllers
{
    [OutputCache(Duration = 0, Location = System.Web.UI.OutputCacheLocation.None)]
    public partial class InitialConfigController : Controller
    {

        #region Determine Server Is Core SKU
        // Added 2012-11-01 G#
        // http://www.discoict.com.au/forum/support/2012/10/install-on-server-core.aspx
        internal static Lazy<bool> ServerIsCoreSKU = new Lazy<bool>(() =>
        {
            try
            {
                uint osSKU = 0;
                using (var mSearcher = new ManagementObjectSearcher("SELECT OperatingSystemSKU FROM Win32_OperatingSystem"))
                {
                    using (var mResults = mSearcher.Get())
                    {
                        foreach (ManagementObject mResult in mResults)
                        {
                            osSKU = (uint)mResult.Properties["OperatingSystemSKU"].Value;
                            break;
                        }
                    }
                }

                switch (osSKU)
                {
                    case 12: // Datacenter Server Core Edition
                    case 13: // Standard Server Core Edition
                    case 14: // Enterprise Server Core Edition
                        return true;
                    default:
                        return false;
                }
            }
            catch (Exception)
            {
                // Ignore Exceptions
            }

            // Default to "Not Core"
            return false;
        });
        // End Added 2012-11-01 G#
        #endregion

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Updated 2012-11-01 G# - Consider ServerIsCoreSKU
            if (!Request.IsLocal && !ServerIsCoreSKU.Value)
            {
                filterContext.Result = new HttpStatusCodeResult(System.Net.HttpStatusCode.ServiceUnavailable, "Initial Configuration of Disco is only allowed via a local connection");
            }
            base.OnActionExecuting(filterContext);
        }

        //
        // GET: /Install/

        public virtual ActionResult Index()
        {
            return RedirectToAction(MVC.InitialConfig.Welcome());
        }

        #region Welcome
        public virtual ActionResult Welcome()
        {
            var m = new WelcomeModel();

            m.AutodetectOrganisation();

            return View(m);
        }
        [HttpPost]
        public virtual ActionResult Welcome(WelcomeModel model)
        {
            if (ModelState.IsValid)
            {
                DiscoApplication.OrganisationName = model.OrganisationName;

                return RedirectToAction(MVC.InitialConfig.Database());
            }

            return View(model);
        }
        #endregion

        #region Database
        public virtual ActionResult Database()
        {
            var cs = Disco.Data.Repository.DiscoDatabaseConnectionFactory.DiscoDataContextConnectionString;

            DatabaseModel m;

            if (cs == null)
                m = new DatabaseModel(); // Just use Defaults
            else
                m = DatabaseModel.FromConnectionString(cs); // Import from existing Connection String

            return View(m);
        }
        [HttpPost]
        public virtual ActionResult Database(DatabaseModel model)
        {
            if (ModelState.IsValid)
            {
                // Continue with Configuration
                var connectionString = model.ToConnectionString();

                // Try Creating/Migrating
                connectionString.ConnectTimeout = 5;
                Disco.Data.Repository.DiscoDatabaseConnectionFactory.SetDiscoDataContextConnectionString(connectionString.ToString(), false);

                try
                {
                    Disco.Data.Migrations.DiscoDataMigrator.MigrateLatest(true);
                }
                catch (Exception ex)
                {
                    // Find inner exception
                    SqlException sqlException = null;
                    Exception innermostException = ex;
                    do
                    {
                        if (sqlException == null)
                            sqlException = innermostException as SqlException;
                        if (innermostException.InnerException != null)
                            innermostException = innermostException.InnerException;
                        else
                            break;
                    } while (true);

                    if (sqlException != null)
                    {
                        ModelState.AddModelError(string.Empty, string.Format("Unable to create or migrate the database to the latest version: [{0}] {1}", sqlException.GetType().Name, sqlException.Message));
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, string.Format("Unable to create or migrate the database to the latest version: [{0}] {1}", innermostException.GetType().Name, innermostException.Message));
                    }
                }

                if (ModelState.IsValid)
                {
                    // Save Connection String
                    //Disco.Data.Repository.DiscoDatabaseConnectionFactory.SetDiscoDataContextConnectionString(model.ToConnectionString().ToString(), true);
                    // Write Organisation Name into DB
                    using (DiscoDataContext db = new DiscoDataContext())
                    {
                        db.DiscoConfiguration.OrganisationName = DiscoApplication.OrganisationName;
                        db.SaveChanges();
                    }

                    return RedirectToAction(MVC.InitialConfig.FileStore());
                }
            }

            return View(model);
        }
        #endregion

        #region FileStore
        public virtual ActionResult FileStore()
        {
            // Try and retrieve FileStore path from DB
            string FileStoreLocation = null;
            try
            {
                using (DiscoDataContext db = new DiscoDataContext())
                    FileStoreLocation = db.ConfigurationItems.Where(ci => ci.Scope == "System" && ci.Key == "DataStoreLocation").Select(ci => ci.Value).FirstOrDefault();
            }
            catch (Exception) { } // Ignore All Errors

            FileStoreModel m = new FileStoreModel();

            // Test for valid Format
            if (!string.IsNullOrEmpty(FileStoreLocation))
                if (!Regex.IsMatch(FileStoreLocation, @"^[A-z]:(\\[^\\/:*?""<>|0x0-0x1F]+)+(\\)?$", RegexOptions.Singleline))
                    FileStoreLocation = null;
            m.FileStoreLocation = FileStoreLocation;
            if (m.FileStoreLocation != null && m.FileStoreLocation.EndsWith(@"\"))
                m.FileStoreLocation = m.FileStoreLocation.TrimEnd('\\');

            m.ExpandDirectoryModel();

            return View(m);
        }
        [HttpPost]
        public virtual ActionResult FileStore(FileStoreModel m)
        {
            if (ModelState.IsValid)
            {
                // Ensure Path Exists
                using (DiscoDataContext db = new DiscoDataContext())
                {
                    var configItem = db.ConfigurationItems.Where(ci => ci.Scope == "System" && ci.Key == "DataStoreLocation").FirstOrDefault();
                    if (configItem == null)
                    { // Create Config
                        db.ConfigurationItems.Add(new Disco.Models.Repository.ConfigurationItem()
                        {
                            Scope = "System",
                            Key = "DataStoreLocation",
                            Value = m.FileStoreLocation
                        });
                    }
                    else
                    { // Update Config
                        configItem.Value = m.FileStoreLocation;
                    }
                    db.SaveChanges();
                }

                // Extract DataStore Template into FileStore
                var templatePath = Server.MapPath("~/ClientBin/DataStoreTemplate.zip");
                if (System.IO.File.Exists(templatePath))
                {
                    try
                    {
                        using (ZipArchive templateArchive = ZipFile.Open(templatePath, ZipArchiveMode.Read))
                        {
                            foreach (var entry in templateArchive.Entries)
                            {
                                var entryDestinationPath = Path.Combine(m.FileStoreLocation, entry.FullName);
                                if (System.IO.File.Exists(entryDestinationPath))
                                    System.IO.File.Delete(entryDestinationPath);
                            }
                            templateArchive.ExtractToDirectory(m.FileStoreLocation);
                        }
                        return RedirectToAction(MVC.InitialConfig.Complete());
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, string.Format("Unable to extract File Store template: [{0}] {1}", ex.GetType().Name, ex.Message));
                    }
                }
                else
                {
                    return RedirectToAction(MVC.InitialConfig.Complete());
                }
            }

            m.ExpandDirectoryModel();

            return View(m);
        }
        public virtual ActionResult FileStoreBranch(string Path)
        {
            return Json(FileStoreModel.FileStoreDirectoryModel.FromPath(Path, true), JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Complete
        public virtual ActionResult Complete()
        {
            var m = new CompleteModel();

            m.PerformTests();

            return View(m);
        }
        #endregion

        #region Restart WebApp

        public virtual ActionResult RestartWebApp()
        {
            RestartWebApp(1500);
            return View();
        }


        private static object _restartTimerLock = new object();
        private static Timer _restartTimer;
        private void RestartWebApp(int DelayMilliseconds)
        {
            lock (_restartTimerLock)
            {
                if (_restartTimer != null)
                {
                    _restartTimer.Dispose();
                }

                _restartTimer = new Timer((state) =>
                {
                    HttpRuntime.UnloadAppDomain();
                    //AppDomain.Unload(AppDomain.CurrentDomain);
                }, null, DelayMilliseconds, Timeout.Infinite);
            }
        }
        #endregion


    }
}
