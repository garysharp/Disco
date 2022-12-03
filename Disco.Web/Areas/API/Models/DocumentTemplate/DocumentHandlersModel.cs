using System.Collections.Generic;

namespace Disco.Web.Areas.API.Models.DocumentTemplate
{
    public class DocumentHandlersModel
    {
        public string TemplateId { get; set; }
        public string TemplateName { get; set; }
        public string TargetId { get; set; }
        public string TargetName { get; set; }
        
        public List<DocumentHandlerModel> Handlers { get; set; }
    }

    public class DocumentHandlerModel
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string UiUrl { get; set; }
        public string Icon { get; set; }
    }
}