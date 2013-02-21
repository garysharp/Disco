using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Disco.Services.Plugins.Features.UIExtension
{
    public abstract class UIExtensionResult
    {
        public PluginFeatureManifest Source { get; private set; }
        
        public UIExtensionResult(PluginFeatureManifest Source)
        {
            this.Source = Source;
        }

        public abstract void ExecuteResult<T>(WebViewPage<T> page);
    }
}
