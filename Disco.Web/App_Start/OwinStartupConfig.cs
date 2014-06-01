using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Disco.Web.OwinStartupConfig))]

namespace Disco.Web
{
    public class OwinStartupConfig
    {
        public void Configuration(IAppBuilder app)
        {
            var hubConfig = new HubConfiguration()
            {
                EnableJavaScriptProxies = false
            };

            app.MapSignalR("/API/Signalling", hubConfig);
        }
    }
}