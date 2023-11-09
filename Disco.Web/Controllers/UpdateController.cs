using Disco.Services.Plugins;
using System.Web.Mvc;

namespace Disco.Web.Controllers
{
    [OutputCache(Duration = 0, Location = System.Web.UI.OutputCacheLocation.None)]
    public partial class UpdateController : Controller
    {
        public virtual ActionResult Index()
        {
            var status = UpdatePluginsAfterDiscoUpdateTask.UpdateDiscoPlugins(true);

            var model = new Models.Update.IndexModel()
            {
                SessionId = status.SessionId
            };

            return View(model);
        }
    }
}