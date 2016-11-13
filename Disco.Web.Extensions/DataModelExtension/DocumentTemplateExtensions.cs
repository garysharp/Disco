using Disco.Models.Repository;
using Disco.Models.Services.Documents;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Disco.Web.Extensions
{
    public static class DocumentTemplateExtensions
    {
        public static IEnumerable<SelectListItem> ToSelectListItems(this IEnumerable<DocumentTemplate> documentTemplates, string SelectedId = null)
        {
            if (SelectedId == null)
                return documentTemplates.Select(dt => new SelectListItem { Value = dt.Id, Text = dt.Description }).ToList();
            else
                return documentTemplates.Select(dt => new SelectListItem { Value = dt.Id, Text = dt.Description, Selected = (SelectedId == dt.Id) }).ToList();
        }

        public static IEnumerable<SelectListItem> ToSelectListItems(this IEnumerable<DocumentTemplatePackage> documentTemplatePackages, string SelectedId = null)
        {
            if (SelectedId == null)
                return documentTemplatePackages.Select(dt => new SelectListItem { Value = $"Package:{dt.Id}", Text = $"Package: {dt.Description}" }).ToList();
            else
                return documentTemplatePackages.Select(dt => new SelectListItem { Value = $"Package:{dt.Id}", Text = $"Package: {dt.Description}", Selected = (SelectedId == dt.Id) }).ToList();
        }
    }
}
