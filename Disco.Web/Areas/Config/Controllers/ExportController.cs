using Disco.Models.UI.Config.Export;
using Disco.Services.Authorization;
using Disco.Services.Exporting;
using Disco.Services.Logging;
using Disco.Services.Plugins.Features.UIExtension;
using Disco.Services.Web;
using Disco.Web.Areas.Config.Models.Export;
using Disco.Web.Extensions;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Disco.Web.Areas.Config.Controllers
{
    [DiscoAuthorize]
    public partial class ExportController : AuthorizedDatabaseController
    {
        [HttpGet]
        [DiscoAuthorize(Claims.Config.ManageSavedExports)]
        public virtual ActionResult Index()
        {
            var exports = SavedExports.GetSavedExports(Database);
            var exportUserIds = exports.Select(e => e.CreatedBy).Distinct().ToList();
            var users = Database.Users.Where(u => exportUserIds.Contains(u.UserId)).ToDictionary(u => u.UserId, StringComparer.OrdinalIgnoreCase);

            var m = new IndexModel
            {
                SavedExports = exports,
                ExportTypeNames = SavedExports.GetSavedExportTypeNames(),
                CreatedUsers = users
            };

            // UI Extensions
            UIExtensions.ExecuteExtensions<ConfigExportIndexModel>(ControllerContext, m);

            return View(m);
        }

        [HttpGet]
        [DiscoAuthorize(Claims.Config.ManageSavedExports)]
        public virtual ActionResult Show(Guid id, bool? saved = null, bool? exported = null)
        {
            var export = SavedExports.GetSavedExport(Database, id, out var exportTypeName);

            if (export == null)
                return HttpNotFound();

            var m = ShowModel.FromSavedExport(export, exportTypeName, saved ?? false, exported ?? false);

            // UI Extensions
            UIExtensions.ExecuteExtensions<ConfigExportShowModel>(ControllerContext, m);

            return View(m);
        }

        [HttpGet]
        [DiscoAuthorize(Claims.Config.ManageSavedExports)]
        public virtual ActionResult Create(Guid id)
        {
            var export = SavedExports.GetSavedExport(Database, id, out var exportTypeName);

            if (export == null)
                return HttpNotFound();

            var m = EditModel.FromNewSavedExport(export, exportTypeName);

            // UI Extensions
            UIExtensions.ExecuteExtensions<ConfigExportCreateModel>(ControllerContext, m);

            return View(m);
        }

        [HttpGet]
        public virtual ActionResult Run(Guid id)
        {
            var export = SavedExports.GetSavedExport(Database, id, out _);

            if (export == null)
                return HttpNotFound();

            if (!SavedExports.IsAuthorized(export, Authorization))
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.Forbidden, "Not authorized to run export");

            var exportResult = SavedExports.EvaluateSavedExport(Database, export);

            var fileStream = exportResult.Result;

            SystemLog.LogInformation($"Ran '{export.Name}' [{export.Name}] for {Authorization.User.UserId}");

            return this.File(fileStream.GetBuffer(), 0, (int)fileStream.Length, exportResult.MimeType, exportResult.Filename);
        }

        [HttpGet]
        [DiscoAuthorize(Claims.Config.ManageSavedExports)]
        public virtual ActionResult RunScheduled(Guid id)
        {
            var export = SavedExports.GetSavedExport(Database, id, out _);

            if (export == null)
                return HttpNotFound();

            SavedExports.EvaluateSavedExportSchedule(Database, export);

            return RedirectToAction(MVC.Config.Export.Show(export.Id, exported: true));
        }
    }
}
