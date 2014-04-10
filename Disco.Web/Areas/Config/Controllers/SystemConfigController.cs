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
    }
}
