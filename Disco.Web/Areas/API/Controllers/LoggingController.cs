using Disco.Models.Exporting;
using Disco.Models.Services.Logging;
using Disco.Services.Authorization;
using Disco.Services.Logging;
using Disco.Services.Tasks;
using Disco.Services.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Disco.Web.Areas.API.Controllers
{
    public partial class LoggingController : AuthorizedDatabaseController
    {
        [DiscoAuthorize(Claims.Config.Logging.Show)]
        public virtual ActionResult Modules()
        {
            var m = LogContext.LogModules.Values.Select(lm => Models.Logs.LogModuleModel.FromLogModule(lm)).ToList();

            return Json(m, JsonRequestBehavior.AllowGet);
        }

        [HttpPost, ValidateAntiForgeryToken, DiscoAuthorize(Claims.Config.Logging.Show)]
        public virtual ActionResult RetrieveEvents(string Format, DateTime? Start = null, DateTime? End = null, int? ModuleId = null, List<int> EventTypeIds = null, int? Take = null)
        {
            if (string.Equals(Format, "json", StringComparison.OrdinalIgnoreCase))
            {
                var logRetriever = new ReadLogContext()
                {
                    Start = Start,
                    End = End,
                    Module = ModuleId,
                    EventTypes = EventTypeIds,
                    Take = Take,
                };
                var results = logRetriever.Query(Database);
                return Json(results, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var exportFormat = ExportFormat.Xlsx;
                if (string.Equals(Format, "csv", StringComparison.OrdinalIgnoreCase))
                    exportFormat = ExportFormat.Csv;

                var options = new LogExportOptions()
                {
                    Format = exportFormat,
                    StartDate = Start,
                    EndDate = End,
                    ModuleId = ModuleId,
                    EventTypeIds = EventTypeIds,
                    Take = Take,
                };
                var exportContext = new LogExport(options);

                var export = exportContext.Export(Database, ScheduledTaskMockStatus.Create("Log Export"));

                return File(export.Result, export.MimeType, export.Filename);
            }
        }

        public virtual ActionResult ScheduledTaskStatus(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException("id");

            var status = ScheduledTasks.GetTaskStatus(id);

            if (status == null)
                throw new ArgumentException("Invalid ScheduledTask SessionId", "id");

            return Json(ScheduledTaskStatusLive.FromScheduledTaskStatus(status, null), JsonRequestBehavior.AllowGet);
        }
    }
}
