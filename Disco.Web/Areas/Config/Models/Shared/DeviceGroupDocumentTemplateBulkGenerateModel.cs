using Disco.Models.UI.Config.Shared;
using System.Collections.Generic;

namespace Disco.Web.Areas.Config.Models.Shared
{
    public abstract class DeviceGroupDocumentTemplateBulkGenerateModel : ConfigSharedDeviceGroupDocumentTemplateBulkGenerate
    {
        public List<Disco.Models.Repository.DocumentTemplate> BulkGenerateDocumentTemplates { get; set; }

        public abstract int DeviceGroupId { get; }
    }
}
