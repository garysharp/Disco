using Disco.Models.UI.Config.JobPreferences;
using Disco.Services.Authorization;
using Disco.Services.Plugins.Features.UIExtension;
using Disco.Services.Web;
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
                LocationList = Database.DiscoConfiguration.JobPreferences.LocationList,
                OnCreateExpression = Database.DiscoConfiguration.JobPreferences.OnCreateExpression,
                OnCloseExpression = Database.DiscoConfiguration.JobPreferences.OnCloseExpression
            };

            // UI Extensions
            UIExtensions.ExecuteExtensions<ConfigJobPreferencesIndexModel>(this.ControllerContext, m);

            return View(m);
        }
    }
}
