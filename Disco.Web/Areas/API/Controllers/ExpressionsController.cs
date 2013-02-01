using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Disco.Web.Areas.API.Controllers
{
    public partial class ExpressionsController : dbAdminController
    {
        public virtual ActionResult ValidateExpression(string Expression)
        {
            var part = new BI.Expressions.EvaluateExpressionPart(Expression);
            return Json(Models.Expressions.ValidateExpressionModel.FromEvaluateExpressionPart(part), JsonRequestBehavior.AllowGet);
        }
    }
}
