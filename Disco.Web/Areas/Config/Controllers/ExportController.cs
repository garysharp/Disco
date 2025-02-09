using Disco.Models.UI.Config.Export;
using Disco.Services.Authorization;
using Disco.Services.Exporting;
using Disco.Services.Interop.ActiveDirectory;
using Disco.Services.Plugins.Features.UIExtension;
using Disco.Services.Web;
using Disco.Web.Areas.API.Models.Shared;
using Disco.Web.Areas.Config.Models.Export;
using Disco.Web.Extensions;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Disco.Web.Areas.Config.Controllers
{
    [DiscoAuthorize(Claims.Config.ManageSavedExports)]
    public partial class ExportController : AuthorizedDatabaseController
    {
        [HttpGet]
        public virtual ActionResult Create(Guid id)
        {
            var export = SavedExports.GetSavedExport(Database, id, out var exportTypeName);

            if (export == null)
                return HttpNotFound();

            var m = CreateModel.FromSavedExport(export, exportTypeName);

            // UI Extensions
            UIExtensions.ExecuteExtensions<ConfigExportCreateModel>(ControllerContext, m);

            return View(m);
        }

        [HttpPost]
        public virtual ActionResult Create(CreateModel model)
        {
            if (model.OnDemandPrincipals != null)
            {
                model.OnDemandSubjects = model.OnDemandPrincipals
                    .Where(p => !string.IsNullOrWhiteSpace(p))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Select(p => ActiveDirectory.RetrieveADObject(p, true))
                    .Where(ad => ad is ADUserAccount || ad is ADGroup)
                    .Select(ad => SubjectDescriptorModel.FromActiveDirectoryObject(ad))
                    .ToList();
            }

            if (ModelState.IsValid)
            {
                SavedExports.UpdateSavedExport(Database, model.ToSavedExport());
            }

            // UI Extensions
            UIExtensions.ExecuteExtensions<ConfigExportCreateModel>(ControllerContext, model);

            return View(model);
        }

        [DiscoAuthorize]
        public virtual ActionResult Run(Guid id)
        {
            var export = SavedExports.GetSavedExport(Database, id, out _);

            if (export == null)
                return HttpNotFound();

            if (!SavedExports.IsAuthorized(export, Authorization))
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.Forbidden, "Not authorized to run export");

            var exportResult = SavedExports.EvaluateSavedExport(Database, export);

            var fileStream = exportResult.Result;

            return this.File(fileStream.GetBuffer(), 0, (int)fileStream.Length, exportResult.MimeType, exportResult.Filename);
        }
    }
}
