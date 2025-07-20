﻿using Disco.Data.Repository;
using Disco.Services.Authorization;
using Disco.Services.Interop.ActiveDirectory;
using Disco.Services.Users;
using Disco.Web.Areas.API.Models.Shared;
using Disco.Web.Models.InitialConfig;
using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Management;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.Mvc;

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
            if (!Request.IsLocal && !ServerIsCoreSKU.Value && !("true".Equals(ConfigurationManager.AppSettings["DiscoAllowRemoteMaintenance"], StringComparison.OrdinalIgnoreCase)))
            {
                filterContext.Result = new ContentResult()
                {
                    Content = "Initial Configuration of Disco ICT is only allowed via a localhost connection",
                    ContentType = "text/plain"
                };
            }
            base.OnActionExecuting(filterContext);
        }

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
            var cs = DiscoDatabaseConnectionFactory.DiscoDataContextConnectionString;

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
                DiscoDatabaseConnectionFactory.SetDiscoDataContextConnectionString(connectionString.ToString(), false);

                try
                {
                    Data.Migrations.DiscoDataMigrator.MigrateLatest(true);
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
                        ModelState.AddModelError(string.Empty, $"Unable to create or migrate the database to the latest version: [{sqlException.GetType().Name}] {sqlException.Message}");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, $"Unable to create or migrate the database to the latest version: [{innermostException.GetType().Name}] {innermostException.Message}");
                    }
                }

                if (ModelState.IsValid)
                {
                    // Save Connection String
                    //Disco.Data.Repository.DiscoDatabaseConnectionFactory.SetDiscoDataContextConnectionString(model.ToConnectionString().ToString(), true);
                    // Write Organisation Name into DB
                    using (DiscoDataContext database = new DiscoDataContext())
                    {
                        database.DiscoConfiguration.OrganisationName = DiscoApplication.OrganisationName;
                        database.SaveChanges();
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
                using (DiscoDataContext database = new DiscoDataContext())
                    FileStoreLocation = database.ConfigurationItems.Where(ci => ci.Scope == "System" && ci.Key == "DataStoreLocation").Select(ci => ci.Value).FirstOrDefault();
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
                using (DiscoDataContext database = new DiscoDataContext())
                {
                    database.DiscoConfiguration.DataStoreLocation = m.FileStoreLocation;
                    database.SaveChanges();

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
                        }
                        catch (Exception ex)
                        {
                            ModelState.AddModelError(string.Empty, $"Unable to extract File Store template: [{ex.GetType().Name}] {ex.Message}");
                        }
                    }

                    // Initialize Core Environment
                    AppConfig.InitalizeCoreEnvironment(database);
                }

                return RedirectToAction(MVC.InitialConfig.Administrators());
            }

            m.ExpandDirectoryModel();

            return View(m);
        }
        public virtual ActionResult FileStoreBranch(string Path)
        {
            return Json(FileStoreModel.FileStoreDirectoryModel.FromPath(Path, true), JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Administrators

        public virtual ActionResult Administrators()
        {
            var administratorSubjects = UserService.AdministratorSubjectIds
                .Select(subjectId => ActiveDirectory.RetrieveADObject(subjectId, Quick: true))
                .Where(item => item != null)
                .Select(item => SubjectDescriptorModel.FromActiveDirectoryObject(item))
                .OrderBy(item => item.Name).ToList();

            var m = new AdministratorsModel()
            {
                AdministratorSubjects = administratorSubjects
            };

            return View(m);
        }
        public virtual ActionResult AdministratorsSearch(string term)
        {
            var groupResults = ActiveDirectory.SearchADGroups(term).Cast<IADObject>();
            var userResults = ActiveDirectory.SearchADUserAccounts(term, Quick: true).Cast<IADObject>();

            var results = groupResults.Concat(userResults).OrderBy(r => r.SamAccountName)
                .Select(r => SubjectDescriptorModel.FromActiveDirectoryObject(r)).ToList();

            return Json(results, JsonRequestBehavior.AllowGet);
        }
        public virtual ActionResult AdministratorsSubject(string Id)
        {
            if (string.IsNullOrWhiteSpace(Id))
                return Json(null, JsonRequestBehavior.AllowGet);

            Id = ActiveDirectory.ParseDomainAccountId(Id);

            var subject = ActiveDirectory.RetrieveADObject(Id, Quick: true);

            if (subject == null || !(subject is ADUserAccount || subject is ADGroup))
                return Json(null, JsonRequestBehavior.AllowGet);
            else
                return Json(SubjectDescriptorModel.FromActiveDirectoryObject(subject), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public virtual ActionResult Administrators(string[] Subjects)
        {
            string[] proposedSubjects;
            string[] removedSubjects = null;
            string[] addedSubjects = null;

            // Validate Subjects
            if (Subjects != null || Subjects.Length > 0)
            {

                var subjects = Subjects
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Select(s => s.Trim())
                    .Select(s => Tuple.Create(s, ActiveDirectory.RetrieveADObject(s, Quick: true)))
                    .ToList();
                var invalidSubjects = subjects.Where(s => s.Item2 == null).ToList();

                if (invalidSubjects.Count > 0)
                    throw new ArgumentException($"Subjects not found: {string.Join(", ", invalidSubjects)}", "Subjects");

                proposedSubjects = subjects.Select(s => s.Item2.Id).OrderBy(s => s).ToArray();
                var currentSubjects = UserService.AdministratorSubjectIds;
                removedSubjects = currentSubjects.Except(proposedSubjects).ToArray();
                addedSubjects = proposedSubjects.Except(currentSubjects).ToArray();

                using (DiscoDataContext database = new DiscoDataContext())
                {
                    UserService.UpdateAdministratorSubjectIds(database, proposedSubjects);
                }

                if (removedSubjects != null && removedSubjects.Length > 0)
                    AuthorizationLog.LogAdministratorSubjectsRemoved("<Initial Configuration>", removedSubjects);
                if (addedSubjects != null && addedSubjects.Length > 0)
                    AuthorizationLog.LogAdministratorSubjectsAdded("<Initial Configuration>", addedSubjects);
            }

            return RedirectToAction(MVC.InitialConfig.Complete());
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
