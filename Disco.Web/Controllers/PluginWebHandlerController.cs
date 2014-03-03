using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Disco.Services.Plugins;
using Disco.Services.Authorization;
using Disco.Services.Users;

namespace Disco.Web.Controllers
{
    public partial class PluginWebHandlerController : Controller
    {
        [OutputCache(Duration = 0, Location = System.Web.UI.OutputCacheLocation.None)]
        public virtual ActionResult Index(string PluginId, string PluginAction)
        {
            var manifest = Plugins.GetPlugin(PluginId);

            if (manifest.HasWebHandler)
            {
                try
                {
                    using (var pluginWebHandler = manifest.CreateWebHandler(this))
                    {
                        pluginWebHandler.OnActionExecuting();
                        return pluginWebHandler.ExecuteAction(PluginAction);
                    }
                }
                catch (AccessDeniedException accessDeniedException)
                {
                    if (UserService.CurrentUserId != null)
                        AuthorizationLog.LogAccessDenied(UserService.CurrentUserId, string.Format("{0} [{1}]", accessDeniedException.Resource, Request.RawUrl), accessDeniedException.Message);

                    return new HttpUnauthorizedResult();
                }
            }

            return HttpNotFound("Plugin has no Web Handler");
        }

        [OutputCache(Duration = 2592000, Location = System.Web.UI.OutputCacheLocation.Any, VaryByParam = "*")]
        public virtual ActionResult Resource(string PluginId, string res, bool? Download)
        {
            var manifest = Plugins.GetPlugin(PluginId);

            Tuple<string, string> pluginResource;

            try
            {
                pluginResource = manifest.WebResourcePath(res);
            }
            catch (FileNotFoundException)
            {
                return HttpNotFound("Plugin Resource Not Found");
            }

            var pluginResourcePath = pluginResource.Item1;

            var mimeType = Disco.BI.Interop.MimeTypes.ResolveMimeType(pluginResourcePath);
            if (Download.HasValue && Download.Value)
                return File(pluginResourcePath, mimeType, Path.GetFileName(pluginResourcePath));
            else
                return File(pluginResourcePath, mimeType);
        }
    }
}
