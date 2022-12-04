using System.Collections.Generic;

namespace Disco.Models.UI.Config.DocumentTemplate
{
    public interface ConfigDocumentTemplateShowModel : BaseUIModel
    {
        Repository.DocumentTemplate DocumentTemplate { get; set; }
        int StoredInstanceCount { get; set; }
        List<bool> TemplatePagesHaveAttachmentId { get; set; }
        int TemplatePageCount { get; }
        string BulkGenerateDownloadId { get; }
        string BulkGenerateDownloadFilename { get; }

        List<Repository.JobType> JobTypes { get; set; }

        List<string> Scopes { get; }
    }
}
