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
