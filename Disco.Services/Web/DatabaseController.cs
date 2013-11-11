using Disco.Data.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Disco.Services.Web
{
    [OutputCache(Duration = 0, Location = System.Web.UI.OutputCacheLocation.None)]
    public abstract class DatabaseController : BaseController
    {
        protected DiscoDataContext Database;

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            this.Database = new DiscoDataContext();
            this.Database.Configuration.LazyLoadingEnabled = false;

            base.OnActionExecuting(filterContext);
        }

        protected override void Dispose(bool disposing)
        {
            if (this.Database != null)
            {
                this.Database.Dispose();
                this.Database = null;
            }

            base.Dispose(disposing);
        }
    }
}
