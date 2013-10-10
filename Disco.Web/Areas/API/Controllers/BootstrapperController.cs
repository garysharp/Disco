using Disco.Services.Authorization;
using Disco.Services.Web;
using System;
using System.Web.Mvc;

namespace Disco.Web.Areas.API.Controllers
{
    [DiscoAuthorize(Claims.Config.Enrolment.Configure)]
    public partial class BootstrapperController : AuthorizedDatabaseController
    {
        public virtual ActionResult MacSshUsername(string MacSshUsername)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(MacSshUsername))
                {
                    Database.DiscoConfiguration.Bootstrapper.MacSshUsername = MacSshUsername;
                    Database.SaveChanges();
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
                    Database.DiscoConfiguration.Bootstrapper.MacSshPassword = MacSshPassword;
                    Database.SaveChanges();
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
