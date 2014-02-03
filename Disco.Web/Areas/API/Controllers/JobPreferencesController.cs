using Disco.Services.Authorization;
using Disco.Services.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Disco.Web.Areas.API.Controllers
{
    public partial class JobPreferencesController : AuthorizedDatabaseController
    {
        const string pLongRunningJobDaysThreshold = "longrunningjobdaysthreshold";

        [DiscoAuthorize(Claims.Config.JobPreferences.Configure)]
        public virtual ActionResult UpdateLongRunningJobDaysThreshold(int LongRunningJobDaysThreshold, bool redirect = false)
        {
            Database.DiscoConfiguration.JobPreferences.LongRunningJobDaysThreshold = LongRunningJobDaysThreshold;
            Database.SaveChanges();

            if (redirect)
                return RedirectToAction(MVC.Config.JobPreferences.Index());
            else
                return Json("OK", JsonRequestBehavior.AllowGet);
        }

    }
}