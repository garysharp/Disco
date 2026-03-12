using Disco.Web.App_Start;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Disco.Web.OwinStartupConfig))]

namespace Disco.Web
{
    public static class OwinStartupConfig
    {
        public static void Configuration(IAppBuilder app)
        {
            app.ConfigureDiscoIctAuthentication();
        }
    }
}