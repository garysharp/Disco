using Disco.Models.Repository;
using System.Collections.Generic;

namespace Disco.Models.UI.Config.DocumentTemplate
{
    public interface ConfigDocumentTemplateCreatePackageModel : BaseUIModel
    {
        string Id { get; set; }
        string Description { get; set; }
        AttachmentTypes Scope { get; set; }

        List<string> Scopes { get; }
    }
}
