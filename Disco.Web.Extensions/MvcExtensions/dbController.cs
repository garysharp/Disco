using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Disco.Data.Repository;

namespace Disco.Web
{
    [OutputCache(Duration = 0, Location = System.Web.UI.OutputCacheLocation.None)]
    public class dbController : Controller
    {
        protected DiscoDataContext dbContext;

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            this.dbContext = new DiscoDataContext();
            this.dbContext.Configuration.LazyLoadingEnabled = false;

            base.OnActionExecuting(filterContext);
        }

        protected override void Dispose(bool disposing)
        {
            if (this.dbContext != null)
            {
                this.dbContext.Dispose();
                this.dbContext = null;
            }

            base.Dispose(disposing);
        }
    }
}