using Disco.Services.Authorization;
using Disco.Services.Interop.DiscoServices;
using Disco.Services.Web;
using Disco.Web.Areas.Config.Models.SystemConfig;
using System;
using System.Web.Mvc;

namespace Disco.Web.Areas.Config.Controllers
{
    public partial class SystemConfigController : AuthorizedDatabaseController
    {
        [DiscoAuthorize(Claims.Config.System.Show), HttpGet]
        public virtual ActionResult Index()
        {
            var m = IndexModel.FromConfiguration(Database.DiscoConfiguration);
            return View(m);
        }

        [DiscoAuthorize(Claims.DiscoAdminAccount), HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult Activate()
        {
            if (Database.DiscoConfiguration.IsActivated)
                return RedirectToAction(MVC.Config.SystemConfig.Index());

            var service = new ActivationService(Database);

            var model = new ActivateModel()
            {
                CallbackUrl = service.GetCallbackUrl(),
                DeploymentId = Guid.Parse(Database.DiscoConfiguration.DeploymentId),
                CorrelationId = Guid.NewGuid(),
                UserId = CurrentUser.UserId,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            };
            model.Proof = service.CalculateCallbackProof(model.CorrelationId, model.UserId, model.Timestamp);

            return View(model);
        }
    }
}
