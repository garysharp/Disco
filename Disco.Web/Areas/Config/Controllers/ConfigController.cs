using Disco.Services.Authorization;
using Disco.Services.Web;
using System.Web.Mvc;

namespace Disco.Web.Areas.Config.Controllers
{
    public partial class ConfigController : AuthorizedDatabaseController
    {
        //
        // GET: /Config/Config/

        [DiscoAuthorize(Claims.Config.Show)]
        public virtual ActionResult Index()
        {

            var m = new Models.Config.IndexModel()
            {
                UpdateResponse = Database.DiscoConfiguration.UpdateLastCheckResponse
            };

            return View(m);
        }

    }
}
