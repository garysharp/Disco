using Disco.Models.UI.Config.DocumentTemplate;
using Disco.Web.Extensions;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Disco.Web.Areas.Config.Models.DocumentTemplate
{
    public class UndetectedPagesModel : ConfigDocumentTemplateUndetectedPagesModel
    {
        
        public List<Disco.Models.Repository.DocumentTemplate> DocumentTemplates { get; set; }

        public List<SelectListItem> DocumentTemplatesSelectListItems
        {
            get
            {
                var list = new List<SelectListItem>();
                list.Add(new SelectListItem() { Selected = false, Value = "--DEVICE", Text = "<Generic Device Document>" });
                list.Add(new SelectListItem() { Selected = true, Value = "--JOB", Text = "<Generic Job Document>" });
                list.Add(new SelectListItem() { Selected = false, Value = "--USER", Text = "<Generic User Document>" });
                list.AddRange(DocumentTemplates.ToSelectListItems());
                return list;
            }
        }
    }
}