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
            {
                string sessionId;
                do
                {
                    System.Threading.Thread.Sleep(100);
                    sessionId = Disco.Services.Tasks.ScheduledTasks.GetTaskStatuses(typeof(Disco.BI.Interop.ActiveDirectory.ActiveDirectoryUpdateLastNetworkLogonDateJob)).Select(t => t.SessionId).FirstOrDefault();
                } while (sessionId == null);

                return View(new Models.Logging.TaskStatusModel() { SessionId = sessionId });
            }
            else
            {
                var taskStatus = Disco.Services.Tasks.ScheduledTasks.GetTaskStatus(id);
                return View(new Models.Logging.TaskStatusModel() { SessionId = taskStatus.SessionId });
            }
            
        }

    }
}
