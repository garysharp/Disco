using Disco.Services.Authorization;
using Disco.Services.Web;
using System.Web.Mvc;

namespace Disco.Web.Areas.Config.Controllers
{
    public partial class SystemConfigController : AuthorizedDatabaseController
    {
        [DiscoAuthorize(Claims.Config.System.Show), HttpGet]
        public virtual ActionResult Index()
        {
            var m = Models.SystemConfig.IndexModel.FromConfiguration(Database.DiscoConfiguration);
            return View(m);
        }

        [DiscoAuthorizeAll(Claims.Config.System.Show, Claims.Config.System.ConfigureProxy), HttpPost]
        public virtual ActionResult Index(Models.SystemConfig.IndexModel config)
        {
            if (ModelState.IsValid)
            {
                config.ToConfiguration(Database);
                return RedirectToAction(MVC.Config.Config.Index());
            }
            else
            {
                return View();
            }
        }

    }
}
