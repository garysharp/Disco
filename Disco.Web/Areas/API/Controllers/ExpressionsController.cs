using Disco.Services.Authorization;
using Disco.Services.Expressions;
using Disco.Services.Plugins.Features.ExpressionExtensionProvider;
using Disco.Services.Web;
using System;
using System.Web.Mvc;

namespace Disco.Web.Areas.API.Controllers
{
    public partial class ExpressionsController : AuthorizedDatabaseController
    {
        [DiscoAuthorize(Claims.DiscoAdminAccount)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult ValidateExpression(string Expression)
        {
            var part = new EvaluateExpressionPart(Expression);
            return Json(Models.Expressions.ValidateExpressionModel.FromEvaluateExpressionPart(part));
        }

        [DiscoAuthorize(Claims.Config.Show)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult TypeDescriptor(string type, bool staticMembersOnly = false)
        {
            if (string.IsNullOrWhiteSpace(type))
                return BadRequest("Type is required");

            var t = Type.GetType(type, false);

            if (t == null)
            {
                var typeNameParts = type.Split(new string[] { ", " }, StringSplitOptions.None);
                if (typeNameParts.Length < 2)
                    return BadRequest("Invalid Type Specified");

                if (!ExpressionExtensionProviderFeature.TryGetExtensionAssembly(typeNameParts[1], out var assembly))
                    return BadRequest("Invalid Type Specified");

                t = assembly.GetType(typeNameParts[0]);

                if (t == null)
                    return BadRequest("Invalid Type Specified");
            }

            return Json(ExpressionTypeDescriptor.Build(t, staticMembersOnly));
        }
    }
}
