using Disco.Services;
using Disco.Services.Web;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace Disco.Web.Areas.API.Controllers
{
    public partial class DeviceFlagAssignmentController : AuthorizedDatabaseController
    {
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult Edit(int id, string comments, DateTime? removeDate, bool? redirect = null)
        {
            try
            {
                if (id < 0)
                    throw new ArgumentOutOfRangeException(nameof(id));

                var assignment = Database.DeviceFlagAssignments
                    .Include(a => a.DeviceFlag)
                    .FirstOrDefault(a => a.Id == id)
                    ?? throw new Exception("Invalid Device Flag Assignment Id");

                if (!assignment.CanEdit())
                    throw new InvalidOperationException("Editing comments for device flags is denied");

                assignment.OnEdit(comments, removeDate);
                Database.SaveChanges();

                if (redirect.HasValue && redirect.Value)
                    return Redirect($"{Url.Action(MVC.Device.Show(assignment.DeviceSerialNumber))}#DeviceDetailTab-Flags");
                else
                    return Ok();
            }
            catch (Exception ex)
            {
                if (redirect.HasValue && redirect.Value)
                    throw;
                else
                    return BadRequest(ex.Message);
            }
        }

        #region Actions
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult AddDevice(int id, string deviceSerialNumber, string comments, DateTime? removeDate)
        {
            Database.Configuration.LazyLoadingEnabled = true;

            var flag = Database.DeviceFlags.Find(id)
                ?? throw new ArgumentException("Invalid Device Flag Id", nameof(id));

            var device = Database.Devices
                .Include(u => u.DeviceFlagAssignments)
                .FirstOrDefault(d => d.SerialNumber == deviceSerialNumber)
                ?? throw new ArgumentException("Invalid Device Serial Number", nameof(deviceSerialNumber));

            if (!device.CanAddDeviceFlag(flag))
                return Unauthorized("Adding device flag is denied");

            if (removeDate.HasValue && removeDate.Value < DateTime.Today.AddDays(1))
                removeDate = null;

            if (device.CanRemoveDeviceFlag(flag))
                device.OnAddDeviceFlag(Database, flag, comments, removeDate);
            else
                device.OnAddDeviceFlag(Database, flag, comments);

            Database.SaveChanges();

            return Redirect($"{Url.Action(MVC.Device.Show(device.SerialNumber))}#DeviceDetailTab-Flags");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult RemoveDevice(int id)
        {
            Database.Configuration.LazyLoadingEnabled = true;

            var assignment = Database.DeviceFlagAssignments
                .Include(a => a.DeviceFlag)
                .FirstOrDefault(a => a.Id == id)
                ?? throw new ArgumentException("Invalid Device Flag Assignment Id", nameof(id));

            if (!assignment.CanRemove())
                return Unauthorized("Removing device flag assignment is denied");

            assignment.OnRemove(Database);
            Database.SaveChanges();

            return Redirect($"{Url.Action(MVC.Device.Show(assignment.DeviceSerialNumber))}#DeviceDetailTab-Flags");
        }

        #endregion

    }
}
