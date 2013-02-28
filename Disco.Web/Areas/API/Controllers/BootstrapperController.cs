using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Disco.BI.Extensions;

namespace Disco.Web.Areas.API.Controllers
{
    public partial class BootstrapperController : dbAdminController
    {

        public virtual ActionResult MacSshUsername(string MacSshUsername)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(MacSshUsername))
                {
                    dbContext.DiscoConfiguration.Bootstrapper.MacSshUsername = MacSshUsername;
                    dbContext.SaveChanges();
                    return Json("OK", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    throw new Exception("The Username cannot be null or empty");
                }
            }
            catch (Exception ex)
            {
                return Json(string.Format("Error: {0}", ex.Message), JsonRequestBehavior.AllowGet);
            }
        }
        public virtual ActionResult MacSshPassword(string MacSshPassword)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(MacSshPassword))
                {
                    dbContext.DiscoConfiguration.Bootstrapper.MacSshPassword = MacSshPassword;
                    dbContext.SaveChanges();
                    return Json("OK", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    throw new Exception("The Password cannot be null or empty");
                }
            }
            catch (Exception ex)
            {
                return Json(string.Format("Error: {0}", ex.Message), JsonRequestBehavior.AllowGet);
            }
        }

    }
}
