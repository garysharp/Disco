using Disco.Models.UI.Config.Enrolment;
using Disco.Services.Authorization;
using Disco.Services.Plugins.Features.UIExtension;
using Disco.Services.Web;
using System.Web.Mvc;

namespace Disco.Web.Areas.Config.Controllers
{
    public partial class EnrolmentController : AuthorizedDatabaseController
    {
        [DiscoAuthorize(Claims.Config.Enrolment.Show)]
        public virtual ActionResult Index()
        {
            var m = new Models.Enrolment.IndexModel()
            {
                MacSshUsername = Database.DiscoConfiguration.Bootstrapper.MacSshUsername
            };

            // UI Extensions
            UIExtensions.ExecuteExtensions<ConfigEnrolmentIndexModel>(this.ControllerContext, m);

            return View(m);
        }

        [DiscoAuthorize(Claims.Config.Enrolment.ShowStatus)]
        public virtual ActionResult Status()
        {
            var m = new Models.Enrolment.StatusModel();

            // UI Extensions
            UIExtensions.ExecuteExtensions<ConfigEnrolmentStatusModel>(this.ControllerContext, m);

            return View(m);
        }

    }
}
