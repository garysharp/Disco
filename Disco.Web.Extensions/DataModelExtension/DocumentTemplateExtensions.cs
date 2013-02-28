using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Disco.Models.Repository;

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
    }
}
