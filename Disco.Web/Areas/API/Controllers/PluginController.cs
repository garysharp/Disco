using Disco.Services.Authorization;
using Disco.Services.Interop.DiscoServices;
using Disco.Services.Plugins;
using Disco.Services.Tasks;
using Disco.Services.Web;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Disco.Web.Areas.API.Controllers
{
    public partial class PluginController : AuthorizedDatabaseController
    {
        [DiscoAuthorize(Claims.Config.Plugin.Install)]
        public virtual ActionResult UpdateLibraryManifest(bool TryWaitingForCompletion = false)
        {
            var status = PluginLibraryUpdateTask.ScheduleNow();

            // If upload takes <= 2 seconds, return back to Plugin Install (rather than Task Status)
            if (TryWaitingForCompletion && status.WaitUntilFinished(TimeSpan.FromSeconds(3)) && status.TaskException == null)
            {
                return RedirectToAction(MVC.Config.Plugins.Install());
            }
            else
            {
                status.SetFinishedUrl(Url.Action(MVC.Config.Plugins.Install()));
                return RedirectToAction(MVC.Config.Logging.TaskStatus(status.SessionId));
            }
        }

        [DiscoAuthorize(Claims.Config.Plugin.Install)]
        public virtual ActionResult UpdateAll()
        {
            var status = UpdatePluginTask.UpdateAllPlugins();

            return RedirectToAction(MVC.Config.Logging.TaskStatus(status.SessionId));
        }

        [DiscoAuthorize(Claims.Config.Plugin.Install)]
        public virtual ActionResult Update(string PluginId)
        {
            if (string.IsNullOrEmpty(PluginId))
                throw new ArgumentNullException("PluginId");

            var status = UpdatePluginTask.UpdatePlugin(PluginId);

            return RedirectToAction(MVC.Config.Logging.TaskStatus(status.SessionId));
        }

        [DiscoAuthorizeAll(Claims.Config.Plugin.Install, Claims.Config.Plugin.InstallLocal)]
        public virtual ActionResult UpdateLocal(string PluginId, HttpPostedFileBase Plugin)
        {
            if (string.IsNullOrEmpty(PluginId))
                throw new ArgumentNullException("PluginId");

            if (Plugin == null || Plugin.ContentLength <= 0 || string.IsNullOrWhiteSpace(Plugin.FileName))
                throw new ArgumentException("A discoPlugin file must be uploaded", "Plugin");

            var tempPluginLocation = Path.Combine(Database.DiscoConfiguration.PluginPackagesLocation, Path.GetFileName(Plugin.FileName));

            if (!Directory.Exists(Database.DiscoConfiguration.PluginPackagesLocation))
                Directory.CreateDirectory(Database.DiscoConfiguration.PluginPackagesLocation);

            if (System.IO.File.Exists(tempPluginLocation))
                System.IO.File.Delete(tempPluginLocation);

            Plugin.SaveAs(tempPluginLocation);

            var status = UpdatePluginTask.UpdateLocalPlugin(PluginId, tempPluginLocation);

            return RedirectToAction(MVC.Config.Logging.TaskStatus(status.SessionId));
        }

        [DiscoAuthorize(Claims.Config.Plugin.Uninstall)]
        public virtual ActionResult Uninstall(string id, bool UninstallData)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException("id");

            PluginManifest manifest = Plugins.GetPlugin(id);

            var status = UninstallPluginTask.UninstallPlugin(manifest, UninstallData);

            return RedirectToAction(MVC.Config.Logging.TaskStatus(status.SessionId));
        }

        [DiscoAuthorize(Claims.Config.Plugin.Install)]
        public virtual ActionResult Install(string PluginId)
        {
            if (string.IsNullOrEmpty(PluginId))
                throw new ArgumentNullException("PluginId", "A PluginId must be supplied");

            var library = PluginLibrary.LoadManifest(Database);
            var libraryIncompatibility = library.LoadIncompatibilityData();
            var libraryItem = library.Plugins.FirstOrDefault(p => p.Id.Equals(PluginId));

            if (libraryItem == null)
                throw new ArgumentNullException("PluginId", "Plugin not found in library");

            var libraryItemRelease = libraryItem.LatestCompatibleRelease(libraryIncompatibility);

            if (libraryItemRelease == null)
                throw new ArgumentNullException("PluginId", "No compatibility releases were found in library");

            // Already Installed?
            if (Plugins.PluginInstalled(libraryItem.Id))
                throw new InvalidOperationException("This plugin is already installed");

            var tempPluginLocation = Path.Combine(Database.DiscoConfiguration.PluginPackagesLocation, string.Format("{0}.discoPlugin", libraryItem.Id));

            var status = InstallPluginTask.InstallPlugin(libraryItem.LatestCompatibleRelease(libraryIncompatibility).DownloadUrl, tempPluginLocation, true);

            return RedirectToAction(MVC.Config.Logging.TaskStatus(status.SessionId));
        }

        [DiscoAuthorizeAll(Claims.Config.Plugin.Install, Claims.Config.Plugin.InstallLocal)]
        public virtual ActionResult InstallLocal(HttpPostedFileBase Plugin)
        {
            if (Plugin == null || Plugin.ContentLength <= 0 || string.IsNullOrWhiteSpace(Plugin.FileName))
                throw new ArgumentException("A discoPlugin file must be uploaded", "Plugin");

            var tempPluginLocation = Path.Combine(Database.DiscoConfiguration.PluginPackagesLocation, Path.GetFileName(Plugin.FileName));

            if (!Directory.Exists(Database.DiscoConfiguration.PluginPackagesLocation))
                Directory.CreateDirectory(Database.DiscoConfiguration.PluginPackagesLocation);

            if (System.IO.File.Exists(tempPluginLocation))
                System.IO.File.Delete(tempPluginLocation);

            Plugin.SaveAs(tempPluginLocation);

            // Check for Install/Update
            PluginManifest packageManifest;
            using (var packageStream = System.IO.File.OpenRead(tempPluginLocation))
            {
                using (ZipArchive packageArchive = new ZipArchive(packageStream, ZipArchiveMode.Read, false))
                {
                    ZipArchiveEntry packageManifestEntry = packageArchive.GetEntry("manifest.json");
                    if (packageManifestEntry == null)
                        throw new InvalidDataException("The plugin package does not contain the 'manifest.json' entry");

                    using (Stream packageManifestStream = packageManifestEntry.Open())
                    {
                        packageManifest = PluginManifest.FromPluginManifestFile(packageManifestStream);
                    }
                }
            }
            
            ScheduledTaskStatus status;
            if (Plugins.PluginInstalled(packageManifest.Id))
                status = UpdatePluginTask.UpdateLocalPlugin(packageManifest.Id, tempPluginLocation);
            else
                status = InstallPluginTask.InstallLocalPlugin(tempPluginLocation, true);

            return RedirectToAction(MVC.Config.Logging.TaskStatus(status.SessionId));
        }
    }
}
