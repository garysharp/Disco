using Disco.Models.Services.Documents;
using Disco.Models.Services.Exporting;
using Disco.Models.UI;
using Disco.Models.UI.Shared;
using System;
using System.Collections.Generic;

namespace Disco.Models.Areas.Config.UI.UserFlag
{
    public interface ConfigDocumentTemplateExportModel : BaseUIModel
    {
        DocumentExportOptions Options { get; set; }

        Guid? ExportId { get; set; }
        ExportResult ExportResult { get; set; }

        List<Repository.DocumentTemplate> DocumentTemplates { get; set; }
        SharedExportFieldsModel<DocumentExportOptions> Fields { get; set; }
    }
}
