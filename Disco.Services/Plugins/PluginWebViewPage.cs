using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Services.Plugins
{
    public abstract class PluginWebViewPage<T> : Disco.Services.Web.WebViewPage<T>
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
            var self = this.GetType();
            this.Manifest = Plugins.GetPlugin(self.Assembly);

            this._plugin = new Lazy<WebPageHelper<T>>(() => {
                if (this.Context == null)
                    throw new InvalidOperationException("The WebViewPage Context property is not initialized");

                return new WebPageHelper<T>(this, this.Manifest);
            });
        }
    }
}
