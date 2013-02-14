using System;
using System.Data.SqlClient;
using System.Threading;
using System.Web.Mvc;
using Disco.Web.Models.InitialConfig;
using Disco.Data.Repository;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using System.IO.Compression;
using System.Management;
using System.Web;
using Disco.Services.Plugins;

namespace Disco.Web.Controllers
{
    [OutputCache(Duration = 0, Location = System.Web.UI.OutputCacheLocation.None)]
    public partial class UpdateController : Controller
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!Request.IsLocal && !InitialConfigController.ServerIsCoreSKU.Value)
            {
                filterContext.Result = new HttpStatusCodeResult(System.Net.HttpStatusCode.ServiceUnavailable, "Initial Configuration of Disco is only allowed via a local connection");
            }
            base.OnActionExecuting(filterContext);
        }

        public virtual ActionResult Index()
        {
            var status = UpdatePluginsAfterDiscoUpdateTask.UpdateDiscoPlugins(true);

            return RedirectToAction(MVC.Config.Logging.TaskStatus(status.SessionId));
        }
    }
}