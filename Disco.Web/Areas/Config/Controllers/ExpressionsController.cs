using Disco.Models.Repository;
using Disco.Models.UI.Config.Expressions;
using Disco.Services.Authorization;
using Disco.Services.Expressions;
using Disco.Services.Plugins.Features.ExpressionExtensionProvider;
using Disco.Services.Plugins.Features.UIExtension;
using Disco.Services.Web;
using Disco.Web.Areas.Config.Models.Expressions;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Disco.Web.Areas.Config.Controllers
{
    public partial class ExpressionsController : AuthorizedDatabaseController
    {
        [DiscoAuthorize(Claims.DiscoAdminAccount)]
        public virtual ActionResult Index()
        {
            throw new NotImplementedException();
        }

        [DiscoAuthorize(Claims.Config.Show)]
        public virtual ActionResult Browser()
        {
            var m = new BrowserModel()
            {
                DeviceType = typeof(Device).AssemblyQualifiedName,
                JobType = typeof(Job).AssemblyQualifiedName,
                UserType = typeof(User).AssemblyQualifiedName,
                Variables = Expression.StandardVariableTypes(),
                ExtensionLibraries = Expression.ExtensionLibraryTypes(),
                PluginExtensionLibraries = ExpressionExtensionProviderFeature.GetExpressionExtensionRegistrations().ToDictionary(r => r.Alias, r => r.ExtensionType.AssemblyQualifiedName)
            };

            // UI Extensions
            UIExtensions.ExecuteExtensions<ConfigExpressionsBrowserModel>(ControllerContext, m);

            return View(m);
        }
    }
}
