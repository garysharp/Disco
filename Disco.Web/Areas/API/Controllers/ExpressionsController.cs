using Disco.Services.Authorization;
using Disco.Services.Expressions;
using Disco.Services.Plugins.Features.ExpressionExtensionProvider;
using Disco.Services.Web;
using System;
using System.Web.Compilation;
using System.Web.Mvc;

namespace Disco.Web.Areas.API.Controllers
{
    public partial class ExpressionsController : AuthorizedDatabaseController
    {
        [HttpPost, DiscoAuthorize(Claims.DiscoAdminAccount)]
        public virtual ActionResult ValidateExpression(string Expression)
        {
            var part = new EvaluateExpressionPart(Expression);
            return Json(Models.Expressions.ValidateExpressionModel.FromEvaluateExpressionPart(part));
        }

        [HttpPost, DiscoAuthorize(Claims.Config.Show), ValidateAntiForgeryToken]
        public virtual ActionResult TypeDescriptor(string type, bool staticMembersOnly = false)
        {
            if (string.IsNullOrWhiteSpace(type))
                return new HttpStatusCodeResult(400, "Type is required");

            var t = Type.GetType(type, false);

            if (t == null)
            {
                var typeNameParts = type.Split(new string[] { ", " }, StringSplitOptions.None);
                if (typeNameParts.Length < 2)
                    return Json("Invalid Type Specified");

                if (!ExpressionExtensionProviderFeature.TryGetExtensionAssembly(typeNameParts[1], out var assembly))
                    return Json("Invalid Type Specified");

                t = assembly.GetType(typeNameParts[0]);

                if (t == null)
                    return Json("Invalid Type Specified");
            }

            return Json(ExpressionTypeDescriptor.Build(t, staticMembersOnly));
        }
    }
}
