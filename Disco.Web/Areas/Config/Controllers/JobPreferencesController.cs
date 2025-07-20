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
                InitialCommentsTemplate = Database.DiscoConfiguration.JobPreferences.InitialCommentsTemplate,
                LongRunningJobDaysThreshold = Database.DiscoConfiguration.JobPreferences.LongRunningJobDaysThreshold,
                StaleJobMinutesThreshold = Database.DiscoConfiguration.JobPreferences.StaleJobMinutesThreshold,
                LodgmentIncludeAllAttachmentsByDefault = Database.DiscoConfiguration.JobPreferences.LodgmentIncludeAllAttachmentsByDefault,
                DefaultNoticeboardTheme = Database.DiscoConfiguration.JobPreferences.DefaultNoticeboardTheme,
                LocationMode = Database.DiscoConfiguration.JobPreferences.LocationMode,
                LocationList = Database.DiscoConfiguration.JobPreferences.LocationList,
                OnCreateExpression = Database.DiscoConfiguration.JobPreferences.OnCreateExpression,
                OnDeviceReadyForReturnExpression = Database.DiscoConfiguration.JobPreferences.OnDeviceReadyForReturnExpression,
                OnCloseExpression = Database.DiscoConfiguration.JobPreferences.OnCloseExpression,
            };

            // UI Extensions
            UIExtensions.ExecuteExtensions<ConfigJobPreferencesIndexModel>(ControllerContext, m);

            return View(m);
        }
    }
}
