using Disco.Services.Authorization;
using Disco.Services.Expressions;
using Disco.Services.Web;
using System.Web.Mvc;

namespace Disco.Web.Areas.API.Controllers
{
    [DiscoAuthorize(Claims.DiscoAdminAccount)]
    public partial class ExpressionsController : AuthorizedDatabaseController
    {
        public virtual ActionResult ValidateExpression(string Expression)
        {
            var part = new EvaluateExpressionPart(Expression);
            return Json(Models.Expressions.ValidateExpressionModel.FromEvaluateExpressionPart(part), JsonRequestBehavior.AllowGet);
        }
    }
}
