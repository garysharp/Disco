using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Services.Plugins
{
    public abstract class PluginWebViewPage<T> : Disco.Services.Web.WebViewPage<T>
    {
        public WebPageHelper<T> Plugin { get; private set; }

        public PluginWebViewPage()
        {
            var self = this.GetType();
            var manifest = Plugins.GetPlugin(self.Assembly);

            this.Plugin = new WebPageHelper<T>(this, manifest);
        }
    }
}
