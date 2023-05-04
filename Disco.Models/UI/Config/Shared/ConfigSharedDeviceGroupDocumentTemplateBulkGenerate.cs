using System.Collections.Generic;

namespace Disco.Models.UI.Config.Shared
{
    public interface ConfigSharedDeviceGroupDocumentTemplateBulkGenerate : BaseUIModel
    {
        List<Repository.DocumentTemplate> BulkGenerateDocumentTemplates { get; set; }
        int DeviceGroupId { get; }
    }
}
