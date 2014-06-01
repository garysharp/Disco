using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.AspNet.SignalR;

namespace Disco.Web.Areas.API
{
    public class APIAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "API";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "API_Update",
                "API/{controller}/Update/{id}/{key}",
                new { action = "Update" }
            );

            context.MapRoute(
                "API_default",
                "API/{controller}/{action}/{id}",
                new { id = UrlParameter.Optional },
                new string[] { "Disco.Web.Areas.API.Controllers" }
            );
        }
    }
}
