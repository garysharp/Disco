using Disco.Services.Authorization;
using Disco.Services.Devices.Enrolment;
using Disco.Services.Web;
using System;
using System.Web.Mvc;

namespace Disco.Web.Areas.API.Controllers
{
    public partial class EnrolmentController : AuthorizedDatabaseController
    {
        [DiscoAuthorize(Claims.Device.Actions.EnrolDevices)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult ResolveSessionPending(string sessionId, bool approve, int? deviceProfileId, int? deviceBatchId, string reason)
        {
            if (approve && deviceProfileId == null)
                throw new Exception("You must select a device profile to approve the enrollment");

            WindowsDeviceEnrolment.ResolvePendingEnrolment(sessionId, approve, CurrentUser.UserId, deviceProfileId, deviceBatchId, reason);

            return Ok();
        }

        [DiscoAuthorize(Claims.Config.Enrolment.Configure)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult PendingTimeoutMinutes(int PendingTimeoutMinutes)
        {
            try
            {
                if (PendingTimeoutMinutes > 0)
                {
                    Database.DiscoConfiguration.Bootstrapper.PendingTimeout = TimeSpan.FromMinutes(PendingTimeoutMinutes);
                    Database.SaveChanges();
                    return Ok();
                }
                else
                {
                    throw new Exception("The pending timeout must be greater than zero");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [DiscoAuthorize(Claims.Config.Enrolment.Configure)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult MacSshUsername(string MacSshUsername)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(MacSshUsername))
                {
                    Database.DiscoConfiguration.Bootstrapper.MacSshUsername = MacSshUsername;
                    Database.SaveChanges();
                    return Ok();
                }
                else
                {
                    throw new Exception("The Username cannot be null or empty");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [DiscoAuthorize(Claims.Config.Enrolment.Configure)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult MacSshPassword(string MacSshPassword)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(MacSshPassword))
                {
                    Database.DiscoConfiguration.Bootstrapper.MacSshPassword = MacSshPassword;
                    Database.SaveChanges();
                    return Ok();
                }
                else
                {
                    throw new Exception("The Password cannot be null or empty");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [DiscoAuthorize(Claims.Config.Enrolment.Configure)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult LegacyDiscovery(bool enabled)
        {
            try
            {
                Database.DiscoConfiguration.Devices.EnrollmentLegacyDiscoveryDisabled = !enabled;
                Database.SaveChanges();
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
