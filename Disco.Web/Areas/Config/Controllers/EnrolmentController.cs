using Disco.Models.UI.Config.Enrolment;
using Disco.Services.Plugins.Features.UIExtension;
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

            // UI Extensions
            UIExtensions.ExecuteExtensions<ConfigEnrolmentIndexModel>(this.ControllerContext, m);

            return View(m);
        }
        public virtual ActionResult Status()
        {
            var m = new Models.Enrolment.StatusModel();

            // UI Extensions
            UIExtensions.ExecuteExtensions<ConfigEnrolmentStatusModel>(this.ControllerContext, m);

            return View(m);
        }

    }
}
