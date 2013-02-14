using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Disco.Web
{
    public class RouteConfig
    {
        public static void RegisterInstallRoutes(RouteCollection routes)
        {
            // Install Route
            routes.MapRoute(
                name: "InitialConfig", // Route name
                url: "{controller}/{action}/{id}", // URL with parameters
                defaults: new { controller = "InitialConfig", action = "Index", id = UrlParameter.Optional }, // Parameter defaults
                namespaces: new string[] { "Disco.Web.Controllers" } // Controllers Namespace Only
            );
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            // Device Route
            routes.MapRoute(
                name: "Device", // Route name
                url: "Device/{action}/{id}", // URL with parameters
                defaults: new { controller = "Device", action = "Index", id = UrlParameter.Optional }, // Parameter defaults
                namespaces: new string[] { "Disco.Web.Controllers" } // Controllers Namespace Only
            );
            // Search Route
            routes.MapRoute(
                name: "SearchQuery",
                url: "Search/Query/{SearchQuery}",
                defaults: new { controller = "Search", action = "Query", SearchQuery = UrlParameter.Optional }
            );
            // User Route
            routes.MapRoute(
                name: "User", // Route name
                url: "User/{action}/{id}", // URL with parameters
                defaults: new { controller = "User", action = "Index", id = UrlParameter.Optional }, // Parameter defaults
                namespaces: new string[] { "Disco.Web.Controllers" } // Controllers Namespace Only
            );

            // Plugin Resources Route
            routes.MapRoute(
                name: "Plugin_Resources", // Route name
                url: "Plugin/{PluginId}/Resources/{*res}", // URL with parameters
                defaults: new { controller = "PluginWebHandler", action = "Resource" }, // Parameter defaults
                namespaces: new string[] { "Disco.Web.Controllers" } // Controllers Namespace Only
            );
            // Plugin Route
            routes.MapRoute(
                name: "Plugin", // Route name
                url: "Plugin/{PluginId}/{PluginAction}", // URL with parameters
                defaults: new { controller = "PluginWebHandler", action = "Index" }, // Parameter defaults
                namespaces: new string[] { "Disco.Web.Controllers" } // Controllers Namespace Only
            );

            // Job Route
            routes.MapRoute(
                name: "Job", // Route name
                url: "{controller}/{action}/{id}", // URL with parameters
                defaults: new { controller = "Job", action = "Index", id = UrlParameter.Optional }, // Parameter defaults
                namespaces: new string[] { "Disco.Web.Controllers" } // Controllers Namespace Only
            );
        }

        public static void RegisterUpdateRoutes(RouteCollection routes)
        {
            // Update Route
            routes.MapRoute(
                name: "Update", // Route name
                url: "{controller}/{action}/{id}", // URL with parameters
                defaults: new { controller = "Update", action = "Index", id = UrlParameter.Optional }, // Parameter defaults
                namespaces: new string[] { "Disco.Web.Controllers" } // Controllers Namespace Only
            );
        }
    }
}