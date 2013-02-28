using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Disco.Web.Areas.Public.Controllers
{
    public partial class PublicController : Controller
    {
        //
        // GET: /Public/Public/

        public virtual ActionResult Index()
        {
            return View();
        }

        public virtual ActionResult Credits()
        {
            return View();
        }
        public virtual ActionResult Licence()
        {
            return View();
        }

    }
}
