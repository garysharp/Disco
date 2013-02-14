using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Disco.Services.Plugins;
using Disco.Services.Plugins.CommunityInterop;

namespace Disco.Web.Areas.API.Controllers
{
    public partial class PluginController : dbAdminController
    {
        public virtual ActionResult UpdateLibraryCatalogue()
        {
            var status = PluginLibraryUpdateTask.ScheduleNow();

            status.SetFinishedUrl(Url.Action(MVC.Config.Plugins.Install()));

            return RedirectToAction(MVC.Config.Logging.TaskStatus(status.SessionId));
        }

        public virtual ActionResult UpdateAll()
        {
            var status = UpdatePluginTask.UpdateAllPlugins();

            return RedirectToAction(MVC.Config.Logging.TaskStatus(status.SessionId));
        }

        public virtual ActionResult Update(string PluginId)
        {
            if (string.IsNullOrEmpty(PluginId))
                throw new ArgumentNullException("PluginId");

            var status = UpdatePluginTask.UpdatePlugin(PluginId);

            return RedirectToAction(MVC.Config.Logging.TaskStatus(status.SessionId));
        }

        public virtual ActionResult UpdateLocal(string PluginId, HttpPostedFileBase Plugin)
        {
            if (string.IsNullOrEmpty(PluginId))
                throw new ArgumentNullException("PluginId");

            if (Plugin == null || Plugin.ContentLength <= 0 || string.IsNullOrWhiteSpace(Plugin.FileName))
                throw new ArgumentException("A discoPlugin file must be uploaded", "Plugin");

            var tempPluginLocation = Path.Combine(dbContext.DiscoConfiguration.PluginPackagesLocation, Path.GetFileName(Plugin.FileName));

            if (!Directory.Exists(dbContext.DiscoConfiguration.PluginPackagesLocation))
                Directory.CreateDirectory(dbContext.DiscoConfiguration.PluginPackagesLocation);

            if (System.IO.File.Exists(tempPluginLocation))
                System.IO.File.Delete(tempPluginLocation);

            Plugin.SaveAs(tempPluginLocation);

            var status = UpdatePluginTask.UpdateLocalPlugin(PluginId, tempPluginLocation);

            return RedirectToAction(MVC.Config.Logging.TaskStatus(status.SessionId));
        }

        public virtual ActionResult Uninstall(string id, bool UninstallData)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException("id");

            PluginManifest manifest = Plugins.GetPlugin(id);

            var status = UninstallPluginTask.UninstallPlugin(manifest, UninstallData);

            return RedirectToAction(MVC.Config.Logging.TaskStatus(status.SessionId));
        }

        public virtual ActionResult Install(string PluginId)
        {
            if (string.IsNullOrEmpty(PluginId))
                throw new ArgumentNullException("PluginId", "A PluginId must be supplied");

            var catalogue = Plugins.LoadCatalogue(dbContext);
            var plugin = catalogue.Plugins.FirstOrDefault(p => p.Id.Equals(PluginId));

            if (plugin == null)
                throw new ArgumentNullException("PluginId", "Plugin not found in catalogue");

            // Already Installed?
            if (Plugins.PluginInstalled(plugin.Id))
                throw new InvalidOperationException("This plugin is already installed");

            var tempPluginLocation = Path.Combine(dbContext.DiscoConfiguration.PluginPackagesLocation, string.Format("{0}.discoPlugin", plugin.Id));

            var status = InstallPluginTask.InstallPlugin(plugin.LatestDownloadUrl, tempPluginLocation, true);

            return RedirectToAction(MVC.Config.Logging.TaskStatus(status.SessionId));
        }

        public virtual ActionResult InstallLocal(HttpPostedFileBase Plugin)
        {
            if (Plugin == null || Plugin.ContentLength <= 0 || string.IsNullOrWhiteSpace(Plugin.FileName))
                throw new ArgumentException("A discoPlugin file must be uploaded", "Plugin");

            var tempPluginLocation = Path.Combine(dbContext.DiscoConfiguration.PluginPackagesLocation, Path.GetFileName(Plugin.FileName));

            if (!Directory.Exists(dbContext.DiscoConfiguration.PluginPackagesLocation))
                Directory.CreateDirectory(dbContext.DiscoConfiguration.PluginPackagesLocation);

            if (System.IO.File.Exists(tempPluginLocation))
                System.IO.File.Delete(tempPluginLocation);

            Plugin.SaveAs(tempPluginLocation);

            var status = InstallPluginTask.InstallLocalPlugin(tempPluginLocation, true);

            return RedirectToAction(MVC.Config.Logging.TaskStatus(status.SessionId));
        }
    }
}
