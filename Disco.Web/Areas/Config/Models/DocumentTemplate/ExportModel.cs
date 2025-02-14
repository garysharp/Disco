using Disco.Models.Areas.Config.UI.UserFlag;
using Disco.Models.Services.Documents;
using Disco.Models.Services.Exporting;
using Disco.Models.UI.Shared;
using System;
using System.Collections.Generic;

namespace Disco.Web.Areas.Config.Models.DocumentTemplate
{
    public class ExportModel : ConfigDocumentTemplateExportModel
    {
        public DocumentExportOptions Options { get; set; }

        public Guid? ExportId { get; set; }
        public ExportResult ExportResult { get; set; }

        public List<Disco.Models.Repository.DocumentTemplate> DocumentTemplates { get; set; }
        public SharedExportFieldsModel<DocumentExportOptions> Fields { get; set; }
    }
}
