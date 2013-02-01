using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Services.Plugins.Categories.InteroperabilityProvider
{
    [PluginCategory(DisplayName = "Interoperability Providers")]
    public abstract class InteroperabilityProviderPlugin : Plugin
    {
        public override sealed Type PluginCategoryType
        {
            get { return typeof(InteroperabilityProviderPlugin); }
        }
    }
}
