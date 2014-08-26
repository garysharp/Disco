using Disco.Models.UI.Config.JobPreferences;
using Disco.Services.Authorization;
using Disco.Services.Plugins.Features.UIExtension;
using Disco.Services.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Disco.Web.Areas.Config.Controllers
{
    public partial class JobPreferencesController : AuthorizedDatabaseController
    {
        [DiscoAuthorize(Claims.Config.JobPreferences.Show)]
        public virtual ActionResult Index()
        {
            var m = new Models.JobPreferences.IndexModel()
            {
                LongRunningJobDaysThreshold = Database.DiscoConfiguration.JobPreferences.LongRunningJobDaysThreshold,
                StaleJobMinutesThreshold = Database.DiscoConfiguration.JobPreferences.StaleJobMinutesThreshold,
                DefaultNoticeboardTheme = Database.DiscoConfiguration.JobPreferences.DefaultNoticeboardTheme,
                LocationMode = Database.DiscoConfiguration.JobPreferences.LocationMode,
                LocationList = Database.DiscoConfiguration.JobPreferences.LocationList
            };

            // UI Extensions
            UIExtensions.ExecuteExtensions<ConfigJobPreferencesIndexModel>(this.ControllerContext, m);

            return View(m);
        }
    }
}
