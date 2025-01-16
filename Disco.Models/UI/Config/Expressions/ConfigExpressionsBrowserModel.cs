using System.Collections.Generic;

namespace Disco.Models.UI.Config.Expressions
{
    public interface ConfigExpressionsBrowserModel : BaseUIModel
    {
        string DeviceType { get; set; }
        string UserType { get; set; }
        string JobType { get; set; }

        Dictionary<string, string> Variables { get; set; }
        Dictionary<string, string> ExtensionLibraries { get; set; }
        Dictionary<string, string> PluginExtensionLibraries { get; set; }
    }
}
