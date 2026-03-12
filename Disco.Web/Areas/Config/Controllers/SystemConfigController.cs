using Disco.Services.Authorization;
using Disco.Services.Interop.DiscoServices;
using Disco.Services.Web;
using Disco.Web.App_Start;
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

        [DiscoAuthorize(Claims.DiscoAdminAccount), HttpGet]
        public virtual ActionResult SSO(string session = null, string error = null)
        {
            if (!string.IsNullOrEmpty(error))
                throw new InvalidOperationException("SSO test failed: " + error);

            if (Database.DiscoConfiguration.SsoAdministrativelyDisabled)
                return RedirectToAction(MVC.Config.SystemConfig.Index());

            if (AuthenticationConfig.TryGetSuccessfulSessionConfiguration(session, out var ssoConfiguration))
            {
                var model = new SsoModel()
                {
                    Mode = SsoMode.Testing,
                    ClientId = ssoConfiguration.ClientId,
                    Authority = ssoConfiguration.Authority,
                    TestedSession = session,
                };
                return View(model);
            }

            if (Database.DiscoConfiguration.SsoEnabled)
            {
                ssoConfiguration = Database.DiscoConfiguration.SsoConfiguration;
                var model = new SsoModel()
                {
                    Mode = SsoMode.OpenIdConnect,
                    ClientId = ssoConfiguration.ClientId,
                    Authority = ssoConfiguration.Authority,
                };
                return View(model);
            }
            else
            {
                var model = new SsoModel()
                {
                    Mode = SsoMode.WindowsAuthentication,
                };
                return View(model);
            }
        }
    }
}
