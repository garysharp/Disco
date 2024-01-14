using Disco.Models.Repository;
using Disco.Services;
using Disco.Services.Authorization;
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

        public virtual ActionResult Update(int id, string key, string value = null, bool? redirect = null)
        {
            try
            {
                if (id < 0)
                    throw new ArgumentOutOfRangeException(nameof(id));
                if (string.IsNullOrEmpty(key))
                    throw new ArgumentNullException(nameof(key));
                var assignment = Database.DeviceFlagAssignments.FirstOrDefault(a => a.Id == id);
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
                    return Json("OK", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                if (redirect.HasValue && redirect.Value)
                    throw;
                else
                    return Json($"Error: {ex.Message}", JsonRequestBehavior.AllowGet);
            }
        }

        #region Update Shortcut Methods
        [DiscoAuthorizeAny(Claims.Device.Actions.EditFlags)]
        public virtual ActionResult UpdateComments(int id, string Comments = null, bool? redirect = null)
        {
            return Update(id, pComments, Comments, redirect);
        }
        #endregion

        #region Update Properties
        private void UpdateComments(DeviceFlagAssignment assignment, string Comments)
        {
            if (!assignment.CanEditComments())
                throw new InvalidOperationException("Editing comments for device flags is denied");

            assignment.OnEditComments(Comments);
            Database.SaveChanges();
        }
        #endregion

        #region Actions

        [DiscoAuthorizeAny(Claims.Device.Actions.AddFlags)]
        public virtual ActionResult AddDevice(int id, string DeviceSerialNumber, string Comments)
        {
            Database.Configuration.LazyLoadingEnabled = true;

            var flag = Database.DeviceFlags.Find(id);
            if (flag == null)
                throw new ArgumentException("Invalid Device Flag Id", nameof(id));

            var device = Database.Devices.Include(u => u.DeviceFlagAssignments).FirstOrDefault(d => d.SerialNumber == DeviceSerialNumber);
            if (device == null)
                throw new ArgumentException("Invalid Device Serial Number", nameof(DeviceSerialNumber));

            if (!device.CanAddDeviceFlag(flag))
                throw new InvalidOperationException("Adding device flag is denied");

            var addingUser = Database.Users.Find(CurrentUser.UserId);

            var assignment = device.OnAddDeviceFlag(Database, flag, addingUser, Comments);

            Database.SaveChanges();

            return Redirect($"{Url.Action(MVC.Device.Show(device.SerialNumber))}#DeviceDetailTab-Flags");
        }

        [DiscoAuthorizeAny(Claims.Device.Actions.RemoveFlags)]
        public virtual ActionResult RemoveDevice(int id)
        {
            Database.Configuration.LazyLoadingEnabled = true;

            var assignment = Database.DeviceFlagAssignments.FirstOrDefault(a => a.Id == id);
            if (assignment == null)
                throw new ArgumentException("Invalid Device Flag Assignment Id", nameof(id));

            if (!assignment.CanRemove())
                throw new InvalidOperationException("Removing device flag assignment is denied");

            var removingUser = Database.Users.Find(CurrentUser.UserId);

            assignment.OnRemove(Database, removingUser);
            Database.SaveChanges();

            return Redirect($"{Url.Action(MVC.Device.Show(assignment.DeviceSerialNumber))}#DeviceDetailTab-Flags");
        }

        #endregion

    }
}