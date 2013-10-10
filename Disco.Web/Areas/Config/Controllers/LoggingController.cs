using Disco.Models.UI.Config.Logging;
using Disco.Services.Authorization;
using Disco.Services.Logging;
using Disco.Services.Logging.Models;
using Disco.Services.Plugins.Features.UIExtension;
using Disco.Services.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Disco.Web.Areas.Config.Controllers
{
    public partial class LoggingController : AuthorizedDatabaseController
    {
        //
        // GET: /Config/Logs/
        [DiscoAuthorize(Claims.Config.Logging.Show)]
        public virtual ActionResult Index()
        {
            var m = new Models.Logging.IndexModel()
            {
                LogModules = new Dictionary<LogBase, List<LogEventType>>()
            };
            foreach (var logModule in LogContext.LogModules.Values)
            {
                m.LogModules.Add(logModule, logModule.EventTypes.Values.Where(et => et.UsePersist).ToList());
            }

            // UI Extensions
            UIExtensions.ExecuteExtensions<ConfigLoggingIndexModel>(this.ControllerContext, m);

            return View(m);
        }

        public virtual ActionResult TaskStatus(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException("id", "A Task Status Id is required");

            var taskStatus = Disco.Services.Tasks.ScheduledTasks.GetTaskStatus(id);
            if (taskStatus == null)
                return RedirectToAction(MVC.Config.Logging.Index());

            var m = new Models.Logging.TaskStatusModel() { SessionId = taskStatus.SessionId };

            // UI Extensions
            UIExtensions.ExecuteExtensions<ConfigLoggingTaskStatusModel>(this.ControllerContext, m);

            return View(m);
        }

    }
}
