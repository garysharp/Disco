using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Disco.Web.Areas.API.Controllers
{
    public partial class DeviceCertificateController : dbAdminController
    {

        public virtual ActionResult Download(int id)
        {
            var wc = dbContext.DeviceCertificates.Find(id);
            if (wc == null)
            {
                throw new Exception("Invalid Device Certificate Id");
            }
            return File(wc.Content, "application/x-pkcs12", string.Format("{0}.pfx", wc.Name));
        }

    }
}
