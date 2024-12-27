using Disco.Services.Authorization;
using Disco.Services.Interop.DiscoServices;
using Disco.Services.Web;
using Disco.Web.Areas.API.Models.Activation;
using System;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Disco.Web.Areas.API.Controllers
{

    [DiscoAuthorize(Claims.DiscoAdminAccount)]
    public partial class ActivationController : AuthorizedDatabaseController
    {
        [HttpPost]
        public virtual ActionResult TestCallback(CallbackModel model)
        {
            return this.PrecompiledPartialView<API.Views.Activation._ActivateCallback>(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public virtual async Task<ActionResult> Begin()
        {
            var service = new ActivationService(Database);

            var challengeModel = await service.BeginActivation(CurrentUser, Url.ActionAbsolute(MVC.API.Activation.Complete()), Url.ActionAbsolute(MVC.Config.SystemConfig.Index()));

            var model = new BeginModel()
            {
                ActivationId = challengeModel.ActivationId,
                ChallengeResponse = Convert.ToBase64String(challengeModel.ChallengeResponse),
                ChallengeResponseIv = Convert.ToBase64String(challengeModel.ChallengeResponseIv),
                RedirectUrl = challengeModel.RedirectUrl
            };

            return View(model);
        }

        [HttpGet]
        public virtual async Task<ActionResult> Complete(Guid activationId, string challenge, string challengeIv, string signature)
        {
            var service = new ActivationService(Database);

            var challengeBytes = Convert.FromBase64String(challenge.Replace('-', '+').Replace('_', '/'));
            var challengeIvBytes = Convert.FromBase64String(challengeIv.Replace('-', '+').Replace('_', '/'));
            var signatureBytes = Convert.FromBase64String(signature.Replace('-', '+').Replace('_', '/'));

            await service.CompleteActivation(activationId, challengeBytes, challengeIvBytes, signatureBytes);

            return RedirectToAction(MVC.Config.SystemConfig.Index());
        }

    }
}
