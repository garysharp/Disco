using System.Web.Mvc;
using System.Web.Routing;
using SignalR;

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
            context.Routes.MapConnection<Disco.Services.Logging.Targets.LogLiveContext>(
                "API_Logging_Notifications", "API/Logging/Notifications/{*operation}");

            context.Routes.MapConnection<Disco.Services.Tasks.ScheduledTasksLiveStatusService>(
                "API_Logging_TaskStatusNotifications", "API/Logging/TaskStatusNotifications/{*operation}");

            context.MapRoute(
                "API_Update",
                "API/{controller}/Update/{id}/{key}",
                new { action = "Update" }
            );

            context.MapRoute(
                "API_default",
                "API/{controller}/{action}/{id}",
                new { id = UrlParameter.Optional }
            );
        }
    }
}
