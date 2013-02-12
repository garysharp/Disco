using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Disco.Services.Logging;
using Disco.Services.Logging.Models;

namespace Disco.Web.Areas.Config.Controllers
{
    public partial class LoggingController : dbAdminController
    {
        //
        // GET: /Config/Logs/

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

            return View(m);
        }

        public virtual ActionResult TaskStatus(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException("id", "A Task Status Id is required");

            var taskStatus = Disco.Services.Tasks.ScheduledTasks.GetTaskStatus(id);
            if (taskStatus == null)
                return RedirectToAction(MVC.Config.Logging.Index());

            return View(new Models.Logging.TaskStatusModel() { SessionId = taskStatus.SessionId });
        }

    }
}
