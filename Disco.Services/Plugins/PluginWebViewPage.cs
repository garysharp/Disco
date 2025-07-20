using System;

namespace Disco.Services.Plugins
{
    public abstract class PluginWebViewPage<T> : Web.WebViewPage<T>
    {
        private Lazy<WebPageHelper<T>> _plugin;

        public PluginManifest Manifest {get;private set;}
        public WebPageHelper<T> Plugin
        {
            get
            {
                return _plugin.Value;
            }
        }

        public PluginWebViewPage()
        {
            var self = GetType();
            Manifest = Plugins.GetPlugin(self.Assembly);

            _plugin = new Lazy<WebPageHelper<T>>(() => {
                if (Context == null)
                    throw new InvalidOperationException("The WebViewPage Context property is not initialized");

                return new WebPageHelper<T>(this, Manifest);
            });
        }
    }
}
