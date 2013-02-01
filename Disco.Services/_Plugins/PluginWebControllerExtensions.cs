using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Routing;
using System.Web.Mvc;
using RazorGenerator.Mvc;
using System.IO;

namespace Disco.Services.Plugins
{
    public static class PluginWebControllerExtensions
    {
        #region Virtual Directories
        public static string WebControllerRootUrl(this IPluginWebController plugin, RequestContext requestContext)
        {
            var tempPath = plugin.WebControllerActionUrl(requestContext, "_");
            return tempPath.Substring(0, tempPath.LastIndexOf(@"/") + 1);
        }
        public static string WebControllerActionUrl(this IPluginWebController plugin, RequestContext requestContext, string PluginAction)
        {
            //return string.Format("~/Config/Plugins/{0}/{1}", HttpUtility.UrlEncode(((IDiscoPlugin)plugin).Id), HttpUtility.UrlEncode(PluginAction));
            var routeValues = new RouteValueDictionary(new { PluginId = ((Plugin)plugin).Id, PluginAction = PluginAction });
            return UrlHelper.GenerateUrl("Config_Plugins_PluginWebControllerActions", "PluginAction", "Plugins", routeValues, RouteTable.Routes, requestContext, true);
        }
        #endregion

        #region Action Results

        #region Compiled View
        private static string[] _viewFileNames = new string[] { "cshtml" };
        public static ActionResult CompiledView(this Controller HostController, Type CompiledViewType, object Model, bool UseDiscoLayout)
        {
            string layoutPath = UseDiscoLayout ? "~/Views/Shared/_Layout.cshtml" : null;

            IView v = new PrecompiledMvcView(HostController.Request.Path, layoutPath, CompiledViewType, false, _viewFileNames);

            if (Model != null)
                HostController.ViewData.Model = Model;

            return new ViewResult { View = v, ViewData = HostController.ViewData, TempData = HostController.TempData };
        }
        public static ActionResult CompiledView(this Controller HostController, Type CompiledViewType, bool UseDiscoLayout)
        {
            return HostController.CompiledView(CompiledViewType, null, UseDiscoLayout);
        }
        public static ActionResult CompiledView(this Controller HostController, Type CompiledViewType, object Model)
        {
            return HostController.CompiledView(CompiledViewType, Model, true);
        }
        public static ActionResult CompiledView(this Controller HostController, Type CompiledViewType)
        {
            return HostController.CompiledView(CompiledViewType, false, true);
        }
        public static ActionResult CompiledPartialView(this Controller HostController, Type PartialCompiledViewType, object Model)
        {
            IView v = new PrecompiledMvcView(HostController.Request.Path, PartialCompiledViewType, false, _viewFileNames);

            if (Model != null)
                HostController.ViewData.Model = Model;

            return new PartialViewResult { View = v, ViewData = HostController.ViewData, TempData = HostController.TempData };
        }
        public static ActionResult CompiledPartialView(this Controller HostController, Type PartialCompiledViewType)
        {
            return HostController.CompiledView(PartialCompiledViewType, null);
        }
        #endregion

        #region Content
        public static ActionResult Content(this Controller HostController, string content, string contentType, Encoding contentEncoding)
        {
            return new ContentResult { Content = content, ContentType = contentType, ContentEncoding = contentEncoding };
        }
        public static ActionResult Content(this Controller HostController, string content, string contentType)
        {
            return HostController.Content(content, null, null);
        }
        public static ActionResult Content(this Controller HostController, string content)
        {
            return HostController.Content(content, null);
        }
        #endregion

        #region Json
        public static ActionResult Json(this Controller HostController, object data, JsonRequestBehavior behavior)
        {
            return new JsonResult { Data = data, ContentType = null, ContentEncoding = null, JsonRequestBehavior = behavior };
        }
        #endregion

        #region File
        public static ActionResult File(this Controller HostController, Stream fileStream, string contentType)
        {
            return HostController.File(fileStream, contentType, null);
        }
        public static ActionResult File(this Controller HostController, Stream fileStream, string contentType, string fileDownloadName)
        {
            return new FileStreamResult(fileStream, contentType) { FileDownloadName = fileDownloadName };
        }
        public static ActionResult File(this Controller HostController, byte[] fileContents, string contentType)
        {
            return HostController.File(fileContents, contentType, null);
        }
        public static ActionResult File(this Controller HostController, byte[] fileContents, string contentType, string fileDownloadName)
        {
            return new FileContentResult(fileContents, contentType) { FileDownloadName = fileDownloadName };
        }
        #endregion

        #region HttpNotFound
        public static ActionResult HttpNotFound(this Controller HostController, string statusDescription)
        {
            return new HttpNotFoundResult(statusDescription);
        }
        public static ActionResult HttpNotFound(this Controller HostController)
        {
            return HostController.HttpNotFound(null);
        }
        #endregion

        #region Redirect
        public static ActionResult RedirectToScheduledTaskStatus(this Controller HostController, string SessionId)
        {
            if (string.IsNullOrEmpty(SessionId))
                throw new ArgumentNullException(SessionId);

            return HostController.RedirectToAction("TaskStatus", "Logging", "Config", new { id = SessionId });
        }
        public static ActionResult Redirect(this Controller HostController, string url)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException("url");

            return new RedirectResult(url);
        }
        public static ActionResult RedirectPermanent(this Controller HostController, string url)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException("url");

            return new RedirectResult(url, true);
        }
        public static ActionResult RedirectToAction(this Controller HostController, IPluginWebController Plugin, string PluginAction)
        {
            if (string.IsNullOrEmpty(PluginAction))
                throw new ArgumentNullException("PluginAction");

            string pluginActionUrl = Plugin.WebControllerActionUrl(HostController.Request.RequestContext, PluginAction);
            return new RedirectResult(pluginActionUrl, false);
        }
        public static ActionResult RedirectToRoute(this Controller HostController, string routeName, object routeValues)
        {
            RouteValueDictionary routeValueDictionary;
            if (routeValues != null)
                routeValueDictionary = new RouteValueDictionary(routeValues);
            else
                routeValueDictionary = new RouteValueDictionary();

            return new RedirectToRouteResult(routeName, routeValueDictionary);
        }
        public static ActionResult RedirectToRoute(this Controller HostController, string routeName)
        {
            return HostController.RedirectToRoute(routeName, null);
        }
        public static ActionResult RedirectToAction(this Controller HostController, string actionName, string controllerName, string areaName, object routeValues)
        {
            RouteValueDictionary routeValueDictionary;
            if (routeValues != null)
                routeValueDictionary = new RouteValueDictionary(routeValues);
            else
                routeValueDictionary = new RouteValueDictionary();

            routeValueDictionary["action"] = actionName;
            routeValueDictionary["controller"] = controllerName;
            if (areaName != null)
                routeValueDictionary["area"] = areaName;

            return new RedirectToRouteResult(routeValueDictionary);
        }
        public static ActionResult RedirectToAction(this Controller HostController, string actionName, string controllerName, string areaName)
        {
            return HostController.RedirectToAction(actionName, controllerName, areaName, null);
        }
        public static ActionResult RedirectToAction(this Controller HostController, string actionName, string controllerName, object routeValues)
        {
            return HostController.RedirectToAction(actionName, controllerName, null, routeValues);
        }
        public static ActionResult RedirectToAction(this Controller HostController, string actionName, string controllerName)
        {
            return HostController.RedirectToAction(actionName, controllerName, null, null);
        }
        public static ActionResult RedirectToDiscoJob(this Controller HostController, int jobId)
        {
            return HostController.RedirectToAction("Show", "Job", null, new { id = jobId.ToString() });
        }
        public static ActionResult RedirectToDiscoDevice(this Controller HostController, string DeviceSerialNumber)
        {
            return HostController.RedirectToAction("Show", "Device", null, new { id = DeviceSerialNumber });
        }
        public static ActionResult RedirectToDiscoUser(this Controller HostController, string UserId)
        {
            return HostController.RedirectToAction("Show", "User", null, new { id = UserId });
        }
        #endregion

        #endregion

    }
}
