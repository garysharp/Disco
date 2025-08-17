using Disco.Models.UI.Config.DocumentTemplate;
using System.Collections.Generic;

namespace Disco.Web.Areas.Config.Models.DocumentTemplate
{
    public class BulkGenerateModel : ConfigDocumentTemplateBulkGenerate
    {
        public Disco.Models.Repository.DocumentTemplate DocumentTemplate { get; set; }
        public int TemplatePageCount { get; set; }
        public List<ItemWithCount<Disco.Models.Repository.DeviceProfile>> DeviceProfiles { get; set; }
        public List<ItemWithCount<Disco.Models.Repository.DeviceBatch>> DeviceBatches { get; set; }
        public List<ItemWithCount<Disco.Models.Repository.DocumentTemplate>> DocumentTemplates { get; set; }
        public List<ItemWithCount<string>> UserDetails { get; set; }
    }

    public class BulkGenerateUserModel : BulkGenerateModel, ConfigDocumentTemplateBulkGenerateUser
    {
        public List<ItemWithCount<Disco.Models.Repository.UserFlag>> UserFlags { get; set; }
    }

    public class BulkGenerateDeviceModel : BulkGenerateModel, ConfigDocumentTemplateBulkGenerateDevice
    {
        public List<ItemWithCount<Disco.Models.Repository.DeviceFlag>> DeviceFlags { get; set; }
    }
}
