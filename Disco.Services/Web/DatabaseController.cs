using Disco.Data.Repository;
using System.Web.Mvc;

namespace Disco.Services.Web
{
    [OutputCache(Duration = 0, Location = System.Web.UI.OutputCacheLocation.None)]
    public abstract class DatabaseController : BaseController
    {
        protected DiscoDataContext Database;

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            Database = new DiscoDataContext();
            Database.Configuration.LazyLoadingEnabled = false;

            base.OnActionExecuting(filterContext);
        }

        protected override void Dispose(bool disposing)
        {
            if (Database != null)
            {
                Database.Dispose();
                Database = null;
            }

            base.Dispose(disposing);
        }
    }
}
