using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.AspNet.SignalR;
using Disco.BI.Interop.SignalRHandlers;

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
            context.Routes.MapConnection<LogNotifications>(
                "API_Logging_Notifications", "API/Logging/Notifications", new ConnectionConfiguration(), SignalRAuthenticationWorkaround.AddMiddleware);

            context.Routes.MapConnection<ScheduledTasksStatusNotifications>(
                "API_Logging_TaskStatusNotifications", "API/Logging/TaskStatusNotifications", new ConnectionConfiguration(), SignalRAuthenticationWorkaround.AddMiddleware);

            context.Routes.MapConnection<RepositoryMonitorNotifications>(
                "API_Repository_Notifications", "API/Repository/Notifications", new ConnectionConfiguration(), SignalRAuthenticationWorkaround.AddMiddleware);

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
