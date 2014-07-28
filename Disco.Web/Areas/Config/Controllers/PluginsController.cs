using Disco.Services.Authorization;
using Disco.Services.Interop.DiscoServices;
using Disco.Services.Plugins;
using Disco.Services.Users;
using Disco.Services.Web;
using Disco.Web.Areas.Config.Models.Plugins;
using System;
using System.Web.Mvc;

namespace Disco.Web.Areas.Config.Controllers
{
    public partial class PluginsController : AuthorizedDatabaseController
    {
        [DiscoAuthorize(Claims.Config.Plugin.Show), HttpGet]
        public virtual ActionResult Index()
        {
            Models.Plugins.IndexViewModel vm = new Models.Plugins.IndexViewModel()
                {
                    PluginManifests = Plugins.GetPlugins(),
                    PluginLibrary = PluginLibrary.LoadManifest(Database)
                };
            return View(vm);
        }

        #region Plugin Configuration
        [DiscoAuthorize(Claims.Config.Plugin.Configure), HttpPost]
        public virtual ActionResult Configure(string PluginId, FormCollection form)
        {
            if (string.IsNullOrEmpty(PluginId))
                return RedirectToAction(MVC.Config.Plugins.Index());

            PluginManifest manifest = Plugins.GetPlugin(PluginId);

            using (PluginConfigurationHandler configHandler = manifest.CreateConfigurationHandler())
            {
                if (configHandler.Post(Database, form, this))
                {
                    Database.SaveChanges();

                    PluginsLog.LogPluginConfigurationSaved(manifest.Id, UserService.CurrentUserId);

                    return RedirectToAction(MVC.Config.Plugins.Index());
                }
                else
                {
                    // Config Errors
                    PluginConfigurationViewModel vm = new PluginConfigurationViewModel(configHandler.Get(Database, this));
                    return View(Views.Configure, vm);
                }
            }
        }

        [DiscoAuthorize(Claims.Config.Plugin.Configure), HttpGet]
        public virtual ActionResult Configure(string PluginId)
        {
            if (string.IsNullOrEmpty(PluginId))
                return RedirectToAction(MVC.Config.Plugins.Index());

            PluginManifest manifest = Plugins.GetPlugin(PluginId);

            using (PluginConfigurationHandler configHandler = manifest.CreateConfigurationHandler())
            {
                PluginConfigurationViewModel vm = new PluginConfigurationViewModel(configHandler.Get(Database, this));
                PluginsLog.LogPluginConfigurationLoaded(manifest.Id, UserService.CurrentUserId);
                return View(Views.Configure, vm);
            }
        }
        #endregion

        [DiscoAuthorize(Claims.Config.Plugin.Install)]
        public virtual ActionResult Install()
        {
            // Check for recent catalogue
            var library = PluginLibrary.LoadManifest(Database);

            if (library == null || library.ManifestDate < DateTime.Now.AddHours(-1))
            {
                // Need to Update Catalogue (over 1 hour old)
                return RedirectToAction(MVC.API.Plugin.UpdateLibraryManifest(true));
            }
            else
            {
                var model = new Models.Plugins.InstallModel()
                {
                    Library = library
                };

                return View(model);
            }
        }

    }
}
