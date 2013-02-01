using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Disco.Web.Areas.Config.Controllers
{
    public partial class EnrolmentController : dbAdminController
    {
        //
        // GET: /Config/Bootstrapper/

        public virtual ActionResult Index()
        {
            var m = new Models.Enrolment.IndexModel()
            {
                MacSshUsername = dbContext.DiscoConfiguration.Bootstrapper.MacSshUsername
            };

            return View(m);
        }
        public virtual ActionResult Status()
        {
            return View();
        }

    }
}
