using System.Collections.Generic;

namespace Disco.Models.UI.Config.DocumentTemplate
{
    public interface ConfigDocumentTemplateBulkGenerate : BaseUIModel
    {
        Repository.DocumentTemplate DocumentTemplate { get; set; }
        int TemplatePageCount { get; set; }

        List<ItemWithCount<Repository.DeviceProfile>> DeviceProfiles { get; set; }
        List<ItemWithCount<Repository.DeviceBatch>> DeviceBatches { get; set; }
        List<ItemWithCount<Repository.DocumentTemplate>> DocumentTemplates { get; set; }
        List<ItemWithCount<string>> UserDetails { get; set; }
    }

    public interface ConfigDocumentTemplateBulkGenerateUser : ConfigDocumentTemplateBulkGenerate
    {
        List<ItemWithCount<Repository.UserFlag>> UserFlags { get; set; }
    }

    public interface ConfigDocumentTemplateBulkGenerateDevice : ConfigDocumentTemplateBulkGenerate
    {
        List<ItemWithCount<Repository.DeviceFlag>> DeviceFlags { get; set; }
    }

    public class ItemWithCount<T>
    {
        public T Item { get; set; }
        public int Count { get; set; }
    }
}
