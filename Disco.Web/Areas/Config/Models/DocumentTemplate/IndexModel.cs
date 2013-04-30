using Disco.Models.UI.Config.DocumentTemplate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Disco.Web.Areas.Config.Models.DocumentTemplate
{
    public class IndexModel : ConfigDocumentTemplateIndexModel
    {
        public List<Disco.Models.Repository.DocumentTemplate> DocumentTemplates { get; set; }
    }
}