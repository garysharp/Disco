using Disco.Services.Authorization;
using Disco.Services.Tasks;
using Disco.Services.Web;
using Disco.Web.App_Start;
using Disco.Web.Areas.Config.Models.SystemConfig;
using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Disco.Web.Areas.API.Controllers
{
    [DiscoAuthorize(Claims.DiscoAdminAccount)]
    [ValidateAntiForgeryToken]
    public partial class AuthenticationController : AuthorizedDatabaseController
    {
        [HttpPost]
        public virtual ActionResult ConfigureSsoTest(SsoModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest("Invalid model");

            AuthenticationConfig.SetTestEntraIdOpenIdConnectOptions(model.TenantId, model.ClientId, CurrentUser);

            return Redirect("/API/Authentication/OIDC/Test");
        }

        [HttpPost]
        public virtual ActionResult ConfigureSso([Required] string session)
        {
            if (!ModelState.IsValid)
                return BadRequest("Invalid model");

            AuthenticationConfig.EnableOpenIdConnect(session, Database);
            var status = RestartAppScheduledTask.ScheduleNow(TimeSpan.FromSeconds(1));
            status.Finished("SSO configuration applied successfully. Restarting Disco ICT...", Url.Action(MVC.Config.SystemConfig.Index()));

            return RedirectToAction(MVC.Config.Logging.TaskStatus(status.SessionId));
        }

        [HttpPost]
        public virtual ActionResult DisableSso()
        {
            if (!ModelState.IsValid)
                return BadRequest("Invalid model");

            AuthenticationConfig.DisableOpenIdConnect(Database);
            var status = RestartAppScheduledTask.ScheduleNow(TimeSpan.FromSeconds(1));
            status.Finished("SSO configuration applied successfully. Restarting Disco ICT...", Url.Action(MVC.Config.SystemConfig.Index()));

            return RedirectToAction(MVC.Config.Logging.TaskStatus(status.SessionId));
        }
    }
}
