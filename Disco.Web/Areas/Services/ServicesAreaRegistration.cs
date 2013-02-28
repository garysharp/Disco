using System.Web.Mvc;

namespace Disco.Web.Areas.Services
{
    public class ServicesAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Services";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Services_default",
                "Services/{controller}/{action}/{feature}",
                new { action = "Index", feature = UrlParameter.Optional }
            );
        }
    }
}
