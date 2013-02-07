using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Disco.Services.Plugins;

namespace Disco.Web.Areas.API.Controllers
{
    public partial class PluginController : dbAdminController
    {

        public virtual ActionResult Uninstall(string id, bool UninstallData)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException("id");

            PluginManifest manifest = Plugins.GetPlugin(id);

            var status = UninstallPluginTask.UninstallPlugin(manifest, UninstallData);

            return RedirectToAction(MVC.Config.Logging.TaskStatus(status.SessionId));
        }

        public virtual ActionResult Install(HttpPostedFileBase Plugin)
        {
            if (Plugin == null || Plugin.ContentLength <= 0 || string.IsNullOrWhiteSpace(Plugin.FileName))
                throw new ArgumentException("A discoPlugin file must be uploaded", "Plugin");

            var tempPluginLocation = Path.Combine(dbContext.DiscoConfiguration.PluginPackagesLocation, Path.GetFileName(Plugin.FileName));

            if (!Directory.Exists(dbContext.DiscoConfiguration.PluginPackagesLocation))
                Directory.CreateDirectory(dbContext.DiscoConfiguration.PluginPackagesLocation);

            if (System.IO.File.Exists(tempPluginLocation))
                System.IO.File.Delete(tempPluginLocation);


            Plugin.SaveAs(tempPluginLocation);

            var status = InstallPluginTask.InstallPlugin(tempPluginLocation, true);

            return RedirectToAction(MVC.Config.Logging.TaskStatus(status.SessionId));
        }
    }
}
