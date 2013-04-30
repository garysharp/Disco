using Disco.Models.UI.Config.DocumentTemplate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Disco.Web.Areas.Config.Models.DocumentTemplate
{
    public class ExpressionBrowserModel : ConfigDocumentTemplateExpressionBrowserModel
    {
        public string DeviceType { get; set; }
        public string UserType { get; set; }
        public string JobType { get; set; }

        public Dictionary<string, string> Variables { get; set; }
        public Dictionary<string, string> ExtensionLibraries { get; set; }
    }
}