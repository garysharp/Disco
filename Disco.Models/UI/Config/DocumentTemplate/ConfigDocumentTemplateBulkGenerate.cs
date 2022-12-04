namespace Disco.Models.UI.Config.DocumentTemplate
{
    public interface ConfigDocumentTemplateBulkGenerate : BaseUIModel
    {
        Repository.DocumentTemplate DocumentTemplate { get; set; }
        int TemplatePageCount { get; set; }
    }
}
