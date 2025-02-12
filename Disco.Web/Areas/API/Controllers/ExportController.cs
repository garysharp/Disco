using Disco.Services.Authorization;
using Disco.Services.Exporting;
using Disco.Services.Interop.ActiveDirectory;
using Disco.Services.Web;
using Disco.Web.Areas.API.Models.Shared;
using Disco.Web.Areas.Config.Models.Export;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Disco.Web.Areas.API.Controllers
{
    [ValidateAntiForgeryToken]
    [DiscoAuthorize(Claims.Config.ManageSavedExports)]
    public partial class ExportController : AuthorizedDatabaseController
    {

        [HttpPost]
        public virtual ActionResult Update(EditModel model)
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

            if (!ModelState.IsValid)
            {
                var errorState = ModelState.First(m => m.Value.Errors.Any());
                var error = errorState.Value.Errors.First();
                return new HttpStatusCodeResult(400, $"{errorState.Key}: {error.Exception?.Message ?? error.ErrorMessage}");
            }

            SavedExports.UpdateSavedExport(Database, model.ToSavedExport());

            return RedirectToAction(MVC.Config.Export.Show(model.Id, saved: true));
        }

        [HttpPost]
        public virtual ActionResult Delete(Guid id)
        {
            SavedExports.DeleteSavedExport(Database, id);

            return RedirectToAction(MVC.Config.Export.Index());
        }

    }
}
