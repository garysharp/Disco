using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Disco.Web.Areas.Config.Controllers
{
    public partial class ConfigController : dbAdminController
    {
        //
        // GET: /Config/Config/

        public virtual ActionResult Index()
        {

            var m = new Models.Config.IndexModel()
            {
                UpdateResponse = dbContext.DiscoConfiguration.UpdateLastCheck
            };

            return View(m);
        }

    }
}
