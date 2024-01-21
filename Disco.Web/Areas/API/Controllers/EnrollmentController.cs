using Disco.Services.Authorization;
using Disco.Services.Devices.Enrolment;
using Disco.Services.Web;
using System;
using System.Web.Mvc;

namespace Disco.Web.Areas.API.Controllers
{
    public partial class EnrollmentController : AuthorizedDatabaseController
    {
        [HttpPost]
        [ValidateAntiForgeryToken]
        [DiscoAuthorize(Claims.Device.Actions.EnrolDevices)]
        public virtual ActionResult ResolveSessionPending(string sessionId, bool approve, string reason)
        {
            WindowsDeviceEnrolment.ResolvePendingEnrollment(sessionId, approve, CurrentUser.UserId, reason);

            return new HttpStatusCodeResult(200);
        }

        [HttpPost]
        [DiscoAuthorize(Claims.Config.Enrolment.Configure)]
        public virtual ActionResult PendingTimeoutMinutes(int PendingTimeoutMinutes)
        {
            try
            {
                if (PendingTimeoutMinutes > 0)
                {
                    Database.DiscoConfiguration.Bootstrapper.PendingTimeout = TimeSpan.FromMinutes(PendingTimeoutMinutes);
                    Database.SaveChanges();
                    return Json("OK", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    throw new Exception("The pending timeout must be greater than zero");
                }
            }
            catch (Exception ex)
            {
                return Json($"Error: {ex.Message}");
            }
        }
    }
}
