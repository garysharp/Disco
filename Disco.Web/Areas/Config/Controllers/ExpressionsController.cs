using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Disco.Web.Areas.Config.Controllers
{
    public partial class ExpressionsController : Controller
    {
        //
        // GET: /Config/Expressions/

        public virtual ActionResult Index()
        {
            return View(Views.Editor, new Models.Expressions.EditorModel()
            {
                Expression = @"JobComponentsTotalCost() < 100 ? JobComponentsTotalCost().ToString('c') : '$100.00'"
            });
        }

    }
}
