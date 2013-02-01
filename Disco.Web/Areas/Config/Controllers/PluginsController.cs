using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Disco.Services.Plugins;
using Disco.Web.Areas.Config.Models.Plugins;

namespace Disco.Web.Areas.Config.Controllers
{
    public partial class PluginsController : dbAdminController
    {
        [HttpGet]
        public virtual ActionResult Index()
        {
            Models.Plugins.IndexViewModel vm = new Models.Plugins.IndexViewModel()
                {
                    PluginManifests = Plugins.GetPlugins()
                };
            return View(vm);
        }

        #region Plugin Configuration
        [HttpPost]
        public virtual ActionResult Configure(string PluginId, FormCollection form)
        {
            if (string.IsNullOrEmpty(PluginId))
                return RedirectToAction(MVC.Config.Plugins.Index());

            PluginManifest manifest = Plugins.GetPlugin(PluginId);

            using (PluginConfigurationHandler configHandler = manifest.CreateConfigurationHandler())
            {
                if (configHandler.Post(dbContext, form, this))
                {
                    dbContext.SaveChanges();

                    PluginsLog.LogPluginConfigurationSaved(manifest.Id, DiscoApplication.CurrentUser.Id);

                    return RedirectToAction(MVC.Config.Plugins.Index());
                }
                else
                {
                    // Config Errors
                    PluginConfigurationViewModel vm = new PluginConfigurationViewModel(configHandler.Get(dbContext, this));
                    return View(Views.Configure, vm);
                }
            }
        }

        [HttpGet]
        public virtual ActionResult Configure(string PluginId)
        {
            if (string.IsNullOrEmpty(PluginId))
                return RedirectToAction(MVC.Config.Plugins.Index());

            PluginManifest manifest = Plugins.GetPlugin(PluginId);

            using (PluginConfigurationHandler configHandler = manifest.CreateConfigurationHandler())
            {
                PluginConfigurationViewModel vm = new PluginConfigurationViewModel(configHandler.Get(dbContext, this));
                PluginsLog.LogPluginConfigurationLoaded(manifest.Id, DiscoApplication.CurrentUser.Id);
                return View(Views.Configure, vm);
            }
        }
        #endregion

        //public virtual ActionResult PluginAction(string PluginId, string PluginAction)
        //{
        //    if (string.IsNullOrEmpty(PluginId))
        //        return HttpNotFound("PluginId is Required");
        //    if (string.IsNullOrEmpty(PluginAction))
        //        return HttpNotFound("PluginAction is Required");

        //    PluginManifest def = Plugins.GetPlugin(PluginId);

        //    using (Plugin instance = def.CreateInstance())
        //    {
        //        IPluginWebController instanceController = instance as IPluginWebController;

        //        if (instanceController == null)
        //            return HttpNotFound("Plugin is not a Web Controller");

        //        PluginsLog.LogPluginWebControllerAccessed(instance.Id, PluginAction, DiscoApplication.CurrentUser.Id);

        //        try
        //        {
        //            return instanceController.ExecuteAction(PluginAction, this);
        //        }
        //        catch (Exception ex)
        //        {
        //            PluginsLog.LogPluginException("Disco Plugin Web Controller Action", new PluginWebControllerException(instance.Id, PluginAction, ex));
        //            return null;
        //        }
        //    }
        //}

    }
}
