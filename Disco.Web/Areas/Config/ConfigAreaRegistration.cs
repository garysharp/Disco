using System.Web.Mvc;

namespace Disco.Web.Areas.Config
{
    public class ConfigAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Config";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Config_DeviceModel_GenericComponents",
                "Config/DeviceModel/GenericComponents",
                new { controller = "DeviceModel", action = "GenericComponents" }
            );
            context.MapRoute(
                "Config_DeviceModel",
                "Config/DeviceModel/{id}",
                new { controller = "DeviceModel", action = "Index", id = UrlParameter.Optional }
            );
            context.MapRoute(
                "Config_DeviceBatch_Create",
                "Config/DeviceBatch/Create",
                new { controller = "DeviceBatch", action = "Create", id = UrlParameter.Optional }
            );
            context.MapRoute(
                "Config_DeviceBatch_Timeline",
                "Config/DeviceBatch/Timeline",
                new { controller = "DeviceBatch", action = "Timeline" }
            );
            context.MapRoute(
                "Config_DeviceBatch",
                "Config/DeviceBatch/{id}",
                new { controller = "DeviceBatch", action = "Index", id = UrlParameter.Optional }
            );
            context.MapRoute(
                "Config_DeviceProfile_Create",
                "Config/DeviceProfile/Create",
                new { controller = "DeviceProfile", action = "Create" }
            );
            context.MapRoute(
                "Config_DeviceProfile_Defaults",
                "Config/DeviceProfile/Defaults",
                new { controller = "DeviceProfile", action = "Defaults" }
            );
            context.MapRoute(
                "Config_DeviceProfile",
                "Config/DeviceProfile/{id}",
                new { controller = "DeviceProfile", action = "Index", id = UrlParameter.Optional }
            );
            context.MapRoute(
                "Config_AttachmentType_Create",
                "Config/AttachmentType/Create",
                new { controller = "AttachmentType", action = "Create" }
            );
            context.MapRoute(
                "Config_AttachmentType_ExpressionBrowser_Type",
                "Config/AttachmentType/ExpressionBrowser/{type}",
                new { controller = "AttachmentType", action = "ExpressionBrowser", type = UrlParameter.Optional }
            );
            context.MapRoute(
                "Config_AttachmentType",
                "Config/AttachmentType/{id}",
                new { controller = "AttachmentType", action = "Index", id = UrlParameter.Optional }
            );
            context.MapRoute(
                "Config_DocumentTemplate_Create",
                "Config/DocumentTemplate/Create",
                new { controller = "DocumentTemplate", action = "Create", id = UrlParameter.Optional }
            );
            context.MapRoute(
                "Config_DocumentTemplate_ImportStatus",
                "Config/DocumentTemplate/ImportStatus",
                new { controller = "DocumentTemplate", action = "ImportStatus", id = UrlParameter.Optional }
            );
            context.MapRoute(
                "Config_DocumentTemplate_UndetectedPages",
                "Config/DocumentTemplate/UndetectedPages",
                new { controller = "DocumentTemplate", action = "UndetectedPages", id = UrlParameter.Optional }
            );
            context.MapRoute(
                "Config_DocumentTemplate_ExpressionBrowser",
                "Config/DocumentTemplate/ExpressionBrowser",
                new { controller = "DocumentTemplate", action = "ExpressionBrowser", id = UrlParameter.Optional }
            );
            context.MapRoute(
                "Config_DocumentTemplate",
                "Config/DocumentTemplate/{id}",
                new { controller = "DocumentTemplate", action = "Index", id = UrlParameter.Optional }
            );
            context.MapRoute(
                "Config_AuthorizationRole_Create",
                "Config/AuthorizationRole/Create",
                new { controller = "AuthorizationRole", action = "Create", id = UrlParameter.Optional }
            );
            context.MapRoute(
                "Config_AuthorizationRole",
                "Config/AuthorizationRole/{id}",
                new { controller = "AuthorizationRole", action = "Index", id = UrlParameter.Optional }
            );
            context.MapRoute(
                "Config_JobQueue_Create",
                "Config/JobQueue/Create",
                new { controller = "JobQueue", action = "Create", id = UrlParameter.Optional }
            );
            context.MapRoute(
                "Config_JobQueue",
                "Config/JobQueue/{id}",
                new { controller = "JobQueue", action = "Index", id = UrlParameter.Optional }
            );

            context.MapRoute(
                "Config_Plugins",
                "Config/Plugins",
                new { controller = "Plugins", action = "Index"}
            );
            context.MapRoute(
                "Config_Plugins_Install",
                "Config/Plugins/Install",
                new { controller = "Plugins", action = "Install" }
            );
            context.MapRoute(
                "Config_Plugins_Configure",
                "Config/Plugins/{PluginId}",
                new { controller = "Plugins", action = "Configure" }
            );

            context.MapRoute(
                "Config_default",
                "Config/{controller}/{action}/{id}",
                new { controller = "Config", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
