using Disco.Services.Authorization;
using Disco.Services.Web;
using System;
using System.Web.Mvc;

namespace Disco.Web.Areas.Config.Controllers
{
    [DiscoAuthorize(Claims.DiscoAdminAccount)]
    public partial class ExpressionsController : AuthorizedDatabaseController
    {
        // Under Construction - Not In Production

        public virtual ActionResult Index()
        {
            throw new NotImplementedException();

            //return View(Views.Editor, new Models.Expressions.EditorModel()
            //{
            //    Expression = @"JobComponentsTotalCost() < 100 ? JobComponentsTotalCost().ToString('c') : '$100.00'"
            //});
        }

    }
}
