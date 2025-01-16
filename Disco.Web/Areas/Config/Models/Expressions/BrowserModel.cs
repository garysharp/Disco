using Disco.Models.UI.Config.Expressions;
using System.Collections.Generic;

namespace Disco.Web.Areas.Config.Models.Expressions
{
    public class BrowserModel : ConfigExpressionsBrowserModel
    {
        public string DeviceType { get; set; }
        public string UserType { get; set; }
        public string JobType { get; set; }

        public Dictionary<string, string> Variables { get; set; }
        public Dictionary<string, string> ExtensionLibraries { get; set; }
        public Dictionary<string, string> PluginExtensionLibraries { get; set; }
    }
}
