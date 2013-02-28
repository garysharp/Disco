using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Disco.Web.Areas.Config.Controllers
{
    public partial class SystemConfigController : dbAdminController
    {
        [HttpGet]
        public virtual ActionResult Index()
        {
            var m = Models.SystemConfig.IndexModel.FromConfiguration(dbContext.DiscoConfiguration);
            return View(m);
        }
        [HttpPost]
        public virtual ActionResult Index(Models.SystemConfig.IndexModel config)
        {
            if (ModelState.IsValid)
            {
                config.ToConfiguration(dbContext);
                return RedirectToAction(MVC.Config.Config.Index());
            }
            else
            {
                return View();
            }
        }

    }
}
