using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Routing;
using RazorGenerator.Mvc;

namespace Disco.Services.Plugins
{
    public abstract class PluginWebHandler : IDisposable
    {
        public PluginManifest Manifest { get; set; }
        public Controller HostController { get; set; }

        public abstract ActionResult ExecuteAction(string ActionName);

        public virtual void Dispose()
        {
            // Nothing in Base Class
        }


        #region Action Results

        #region Compiled View
        private static string[] _viewFileNames = new string[] { "cshtml" };
        public ActionResult CompiledView(Type CompiledViewType, object Model, bool UseDiscoLayout)
        {
            string layoutPath = UseDiscoLayout ? "~/Views/Shared/_Layout.cshtml" : null;

            IView v = new PrecompiledMvcView(this.HostController.Request.Path, layoutPath, CompiledViewType, false, _viewFileNames);

            if (Model != null)
                this.HostController.ViewData.Model = Model;

            return new ViewResult { View = v, ViewData = this.HostController.ViewData, TempData = this.HostController.TempData };
        }
        public ActionResult CompiledView(Type CompiledViewType, bool UseDiscoLayout)
        {
            return this.CompiledView(CompiledViewType, null, UseDiscoLayout);
        }
        public ActionResult CompiledView(Type CompiledViewType, object Model)
        {
            return this.CompiledView(CompiledViewType, Model, true);
        }
        public  ActionResult CompiledView(Type CompiledViewType)
        {
            return this.CompiledView(CompiledViewType, false, true);
        }
        public  ActionResult CompiledPartialView(Type PartialCompiledViewType, object Model)
        {
            IView v = new PrecompiledMvcView(this.HostController.Request.Path, PartialCompiledViewType, false, _viewFileNames);

            if (Model != null)
                this.HostController.ViewData.Model = Model;

            return new PartialViewResult { View = v, ViewData = this.HostController.ViewData, TempData = this.HostController.TempData };
        }
        public  ActionResult CompiledPartialView(Type PartialCompiledViewType)
        {
            return this.CompiledView(PartialCompiledViewType, null);
        }
        #endregion

        #region Content
        public ActionResult Content(string content, string contentType, Encoding contentEncoding)
        {
            return new ContentResult { Content = content, ContentType = contentType, ContentEncoding = contentEncoding };
        }
        public ActionResult Content(string content, string contentType)
        {
            return this.Content(content, null, null);
        }
        public ActionResult Content(string content)
        {
            return this.Content(content, null);
        }
        #endregion

        #region Json
        public ActionResult Json(object data, JsonRequestBehavior behavior)
        {
            return new JsonResult { Data = data, ContentType = null, ContentEncoding = null, JsonRequestBehavior = behavior };
        }
        #endregion

        #region File
        public ActionResult File(Stream fileStream, string contentType)
        {
            return this.File(fileStream, contentType, null);
        }
        public ActionResult File(Stream fileStream, string contentType, string fileDownloadName)
        {
            return new FileStreamResult(fileStream, contentType) { FileDownloadName = fileDownloadName };
        }
        public ActionResult File(byte[] fileContents, string contentType)
        {
            return this.File(fileContents, contentType, null);
        }
        public ActionResult File(byte[] fileContents, string contentType, string fileDownloadName)
        {
            return new FileContentResult(fileContents, contentType) { FileDownloadName = fileDownloadName };
        }
        #endregion

        #region HttpNotFound
        public ActionResult HttpNotFound(string statusDescription)
        {
            return new HttpNotFoundResult(statusDescription);
        }
        public ActionResult HttpNotFound()
        {
            return this.HttpNotFound(null);
        }
        #endregion

        #region Redirect
        public ActionResult RedirectToScheduledTaskStatus(string SessionId)
        {
            if (string.IsNullOrEmpty(SessionId))
                throw new ArgumentNullException(SessionId);

            return this.RedirectToAction("TaskStatus", "Logging", "Config", new { id = SessionId });
        }
        public ActionResult Redirect(string url)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException("url");

            return new RedirectResult(url);
        }
        public ActionResult RedirectPermanent(string url)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException("url");

            return new RedirectResult(url, true);
        }
        public ActionResult RedirectToPluginAction(string PluginAction)
        {
            if (string.IsNullOrEmpty(PluginAction))
                throw new ArgumentNullException("PluginAction");

            var routeValues = new RouteValueDictionary(new { PluginId = this.Manifest.Id, PluginAction = PluginAction });
            string pluginActionUrl = UrlHelper.GenerateUrl("Plugin", null, null, routeValues, RouteTable.Routes, this.HostController.Request.RequestContext, false);

            return new RedirectResult(pluginActionUrl, false);
        }
        public ActionResult RedirectToPluginResource(string Resource, bool? Download)
        {
            var resourcePath = this.Manifest.WebResourcePath(Resource);

            var routeValues = new RouteValueDictionary(new { PluginId = this.Manifest.Id, res = Resource });
            string pluginActionUrl = UrlHelper.GenerateUrl("Plugin_Resources", null, null, routeValues, RouteTable.Routes, this.HostController.Request.RequestContext, false);

            pluginActionUrl += string.Format("?v={0}", resourcePath.Item2);

            if (Download.HasValue && Download.Value)
            {
                pluginActionUrl += "&Download=true";
            }

            return new RedirectResult(pluginActionUrl, false);
        }
        public ActionResult RedirectToPluginResource(string Resource)
        {
            return this.RedirectToPluginResource(Resource, null);
        }
        public ActionResult RedirectToRoute(string routeName, object routeValues)
        {
            RouteValueDictionary routeValueDictionary;
            if (routeValues != null)
                routeValueDictionary = new RouteValueDictionary(routeValues);
            else
                routeValueDictionary = new RouteValueDictionary();

            return new RedirectToRouteResult(routeName, routeValueDictionary);
        }
        public ActionResult RedirectToRoute(string routeName)
        {
            return this.RedirectToRoute(routeName, null);
        }
        public ActionResult RedirectToAction(string actionName, string controller, string areaName, object routeValues)
        {
            RouteValueDictionary routeValueDictionary;
            if (routeValues != null)
                routeValueDictionary = new RouteValueDictionary(routeValues);
            else
                routeValueDictionary = new RouteValueDictionary();

            routeValueDictionary["action"] = actionName;
            routeValueDictionary["controller"] = controller;
            if (areaName != null)
                routeValueDictionary["area"] = areaName;

            return new RedirectToRouteResult(routeValueDictionary);
        }
        public ActionResult RedirectToAction(string actionName, string controller, string areaName)
        {
            return this.RedirectToAction(actionName, controller, areaName, null);
        }
        public ActionResult RedirectToAction(string actionName, string controller, object routeValues)
        {
            return this.RedirectToAction(actionName, controller, null, routeValues);
        }
        public ActionResult RedirectToAction(string actionName, string controller)
        {
            return this.RedirectToAction(actionName, controller, null, null);
        }
        public ActionResult RedirectToDiscoJob(int jobId)
        {
            return this.RedirectToAction("Show", "Job", null, new { id = jobId.ToString() });
        }
        public ActionResult RedirectToDiscoDevice(string DeviceSerialNumber)
        {
            return this.RedirectToAction("Show", "Device", null, new { id = DeviceSerialNumber });
        }
        public ActionResult RedirectToDiscoUser(string UserId)
        {
            return this.RedirectToAction("Show", "User", null, new { id = UserId });
        }
        #endregion

        #endregion
    }
}
