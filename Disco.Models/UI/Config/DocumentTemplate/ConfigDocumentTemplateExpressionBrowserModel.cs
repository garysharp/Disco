using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Models.UI.Config.DocumentTemplate
{
    public interface ConfigDocumentTemplateExpressionBrowserModel : BaseUIModel
    {
        string DeviceType { get; set; }
        string UserType { get; set; }
        string JobType { get; set; }

        Dictionary<string, string> Variables { get; set; }
        Dictionary<string, string> ExtensionLibraries { get; set; }
    }
}
