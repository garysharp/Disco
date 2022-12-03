using Disco.Models.Repository;
using Disco.Models.Services.Documents;
using System.Collections.Generic;

namespace Disco.Web.Models.Shared
{
    public class GenerateDocumentControlModel
    {
        public IAttachmentTarget Target { get; set; }
        public List<DocumentTemplate> Templates { get; set; }
        public List<DocumentTemplatePackage> TemplatePackages { get; set; }
        public bool HandlersPresent { get; set; }
    }
}
