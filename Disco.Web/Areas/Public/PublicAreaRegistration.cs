using System.Web.Mvc;

namespace Disco.Web.Areas.Public
{
    public class PublicAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Public";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Public_Credits",
                "Public/Credits/{id}",
                new { controller = "Public", action = "Credits", id = UrlParameter.Optional },
                new string[] { "Disco.Web.Areas.Public.Controllers" }
            );
            context.MapRoute(
                "Public_Licence",
                "Public/Licence/{id}",
                new { controller = "Public", action = "Licence", id = UrlParameter.Optional },
                new string[] { "Disco.Web.Areas.Public.Controllers" }
            );

            context.MapRoute(
                "Public_default",
                "Public/{controller}/{action}/{id}",
                new { controller = "Public", action = "Index", id = UrlParameter.Optional },
                new string[] { "Disco.Web.Areas.Public.Controllers" }
            );
        }
    }
}
