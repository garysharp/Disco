using Disco.Services.Authorization;
using Disco.Services.Devices.Enrolment;
using Disco.Services.Web;
using System;
using System.Web.Mvc;

namespace Disco.Web.Areas.API.Controllers
{
    public partial class EnrolmentController : AuthorizedDatabaseController
    {
        [HttpPost]
        [ValidateAntiForgeryToken]
        [DiscoAuthorize(Claims.Device.Actions.EnrolDevices)]
        public virtual ActionResult ResolveSessionPending(string sessionId, bool approve, int? deviceProfileId, int? deviceBatchId, string reason)
        {
            if (approve && deviceProfileId == null)
                throw new Exception("You must select a device profile to approve the enrollment");

            WindowsDeviceEnrolment.ResolvePendingEnrolment(sessionId, approve, CurrentUser.UserId, deviceProfileId, deviceBatchId, reason);

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
