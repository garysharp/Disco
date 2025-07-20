using Disco.Models.Repository;
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
        const string pComments = "comments";
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult Update(int id, string key, string value = null, bool? redirect = null)
        {
            try
            {
                if (id < 0)
                    throw new ArgumentOutOfRangeException(nameof(id));
                if (string.IsNullOrEmpty(key))
                    throw new ArgumentNullException(nameof(key));
                var assignment = Database.DeviceFlagAssignments
                    .Include(a => a.DeviceFlag)
                    .FirstOrDefault(a => a.Id == id);
                if (assignment != null)
                {
                    switch (key.ToLower())
                    {
                        case pComments:
                            UpdateComments(assignment, value);
                            break;
                        default:
                            throw new Exception("Invalid Update Key");
                    }
                }
                else
                {
                    throw new Exception("Invalid Device Flag Assignment Id");
                }
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

        #region Update Shortcut Methods
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult UpdateComments(int id, string Comments = null, bool? redirect = null)
        {
            return Update(id, pComments, Comments, redirect);
        }
        #endregion

        #region Update Properties
        private void UpdateComments(DeviceFlagAssignment assignment, string comments)
        {
            if (!assignment.CanEdit())
                throw new InvalidOperationException("Editing comments for device flags is denied");

            assignment.OnEdit(comments);
            Database.SaveChanges();
        }
        #endregion

        #region Actions
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult AddDevice(int id, string deviceSerialNumber, string comments)
        {
            Database.Configuration.LazyLoadingEnabled = true;

            var flag = Database.DeviceFlags.Find(id);
            if (flag == null)
                throw new ArgumentException("Invalid Device Flag Id", nameof(id));

            var device = Database.Devices.Include(u => u.DeviceFlagAssignments).FirstOrDefault(d => d.SerialNumber == deviceSerialNumber);
            if (device == null)
                throw new ArgumentException("Invalid Device Serial Number", nameof(deviceSerialNumber));

            if (!device.CanAddDeviceFlag(flag))
                return Unauthorized("Adding device flag is denied");

            var assignment = device.OnAddDeviceFlag(Database, flag, comments);

            Database.SaveChanges();

            return Redirect($"{Url.Action(MVC.Device.Show(device.SerialNumber))}#DeviceDetailTab-Flags");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult RemoveDevice(int id)
        {
            Database.Configuration.LazyLoadingEnabled = true;

            var assignment = Database.DeviceFlagAssignments
                .Include(a => a.DeviceFlag)
                .FirstOrDefault(a => a.Id == id);
            if (assignment == null)
                throw new ArgumentException("Invalid Device Flag Assignment Id", nameof(id));

            if (!assignment.CanRemove())
                return Unauthorized("Removing device flag assignment is denied");

            assignment.OnRemove(Database);
            Database.SaveChanges();

            return Redirect($"{Url.Action(MVC.Device.Show(assignment.DeviceSerialNumber))}#DeviceDetailTab-Flags");
        }

        #endregion

    }
}
