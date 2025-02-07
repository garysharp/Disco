using Disco.Models.Repository;
using Disco.Models.Services.Devices.DeviceFlag;
using Disco.Services;
using Disco.Services.Authorization;
using Disco.Services.Devices.DeviceFlags;
using Disco.Services.Exporting;
using Disco.Services.Interop.ActiveDirectory;
using Disco.Services.Tasks;
using Disco.Services.Web;
using Disco.Web.Areas.Config.Models.DeviceFlag;
using Disco.Web.Extensions;
using System;
using System.Linq;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;

namespace Disco.Web.Areas.API.Controllers
{
    public partial class DeviceFlagController : AuthorizedDatabaseController
    {
        const string pName = "name";
        const string pDescription = "description";
        const string pIcon = "icon";
        const string pIconColour = "iconcolour";
        const string pOnAssignmentExpression = "onassignmentexpression";
        const string pOnUnassignmentExpression = "onunassignmentexpression";

        [DiscoAuthorize(Claims.Config.DeviceFlag.Configure)]
        public virtual ActionResult Update(int id, string key, string value = null, bool? redirect = null)
        {
            Authorization.Require(Claims.Config.DeviceFlag.Configure);

            try
            {
                if (id < 0)
                    throw new ArgumentOutOfRangeException("id");
                if (string.IsNullOrEmpty(key))
                    throw new ArgumentNullException("key");
                var flag = Database.DeviceFlags.Find(id);
                if (flag != null)
                {
                    switch (key.ToLower())
                    {
                        case pName:
                            UpdateName(flag, value);
                            break;
                        case pDescription:
                            UpdateDescription(flag, value);
                            break;
                        case pIcon:
                            UpdateIcon(flag, value);
                            break;
                        case pIconColour:
                            UpdateIconColour(flag, value);
                            break;
                        case pOnAssignmentExpression:
                            UpdateOnAssignmentExpression(flag, value);
                            break;
                        case pOnUnassignmentExpression:
                            UpdateOnUnassignmentExpression(flag, value);
                            break;
                        default:
                            throw new Exception("Invalid Update Key");
                    }
                }
                else
                {
                    throw new Exception("Invalid Device Flag Id");
                }
                if (redirect.HasValue && redirect.Value)
                    return RedirectToAction(MVC.Config.DeviceFlag.Index(flag.Id));
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
        [DiscoAuthorize(Claims.Config.DeviceFlag.Configure)]
        public virtual ActionResult UpdateName(int id, string FlagName = null, bool? redirect = null)
        {
            return Update(id, pName, FlagName, redirect);
        }

        [DiscoAuthorize(Claims.Config.DeviceFlag.Configure)]
        public virtual ActionResult UpdateDescription(int id, string Description = null, bool? redirect = null)
        {
            return Update(id, pDescription, Description, redirect);
        }

        [DiscoAuthorize(Claims.Config.DeviceFlag.Configure)]
        public virtual ActionResult UpdateIcon(int id, string Icon = null, bool? redirect = null)
        {
            return Update(id, pIcon, Icon, redirect);
        }

        [DiscoAuthorize(Claims.Config.DeviceFlag.Configure)]
        public virtual ActionResult UpdateIconColour(int id, string IconColour = null, bool? redirect = null)
        {
            return Update(id, pIconColour, IconColour, redirect);
        }

        [DiscoAuthorize(Claims.Config.DeviceFlag.Configure)]
        public virtual ActionResult UpdateIconAndColour(int id, string Icon = null, string IconColour = null, bool redirect = false)
        {
            try
            {
                if (id < 0)
                    throw new ArgumentOutOfRangeException("id");

                var DeviceFlag = Database.DeviceFlags.Find(id);
                if (DeviceFlag != null)
                {
                    UpdateIconAndColour(DeviceFlag, Icon, IconColour);
                }
                else
                {
                    throw new ArgumentException("Invalid Device Flag Id", "id");
                }
                if (redirect)
                    return RedirectToAction(MVC.Config.DeviceFlag.Index(DeviceFlag.Id));
                else
                    return Json("OK", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                if (redirect)
                    throw;
                else
                    return Json(string.Format("Error: {0}", ex.Message), JsonRequestBehavior.AllowGet);
            }
        }
        [DiscoAuthorize(Claims.Config.DeviceFlag.Configure)]
        public virtual ActionResult UpdateOnAssignmentExpression(int id, string OnAssignmentExpression = null, bool redirect = false)
        {
            return Update(id, pOnAssignmentExpression, OnAssignmentExpression, redirect);
        }
        [DiscoAuthorize(Claims.Config.DeviceFlag.Configure)]
        public virtual ActionResult UpdateOnUnassignmentExpression(int id, string OnUnassignmentExpression = null, bool redirect = false)
        {
            return Update(id, pOnUnassignmentExpression, OnUnassignmentExpression, redirect);
        }
        [DiscoAuthorize(Claims.Config.DeviceFlag.Configure)]
        public virtual ActionResult UpdateDevicesLinkedGroup(int id, string GroupId = null, DateTime? FilterBeginDate = null, bool redirect = false)
        {
            try
            {
                if (id < 0)
                    throw new ArgumentOutOfRangeException("id");

                var deviceFlag = Database.DeviceFlags.Find(id);
                if (deviceFlag == null)
                    throw new ArgumentException("Invalid Device Flag Id", "id");


                var syncTaskStatus = UpdateDevicesLinkedGroup(deviceFlag, GroupId, FilterBeginDate);
                if (redirect)
                    if (syncTaskStatus == null)
                        return RedirectToAction(MVC.Config.DeviceFlag.Index(deviceFlag.Id));
                    else
                    {
                        syncTaskStatus.SetFinishedUrl(Url.Action(MVC.Config.DeviceFlag.Index(deviceFlag.Id)));
                        return RedirectToAction(MVC.Config.Logging.TaskStatus(syncTaskStatus.SessionId));
                    }
                else
                    return Json("OK", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                if (redirect)
                    throw;
                else
                    return Json($"Error: {ex.Message}", JsonRequestBehavior.AllowGet);
            }
        }
        [DiscoAuthorize(Claims.Config.DeviceFlag.Configure)]
        public virtual ActionResult UpdateAssignedUserLinkedGroup(int id, string GroupId = null, DateTime? FilterBeginDate = null, bool redirect = false)
        {
            try
            {
                if (id < 0)
                    throw new ArgumentOutOfRangeException("id");

                var DeviceFlag = Database.DeviceFlags.Find(id);
                if (DeviceFlag == null)
                    throw new ArgumentException("Invalid Device Flag Id", "id");


                var syncTaskStatus = UpdateAssignedUserLinkedGroup(DeviceFlag, GroupId, FilterBeginDate);
                if (redirect)
                    if (syncTaskStatus == null)
                        return RedirectToAction(MVC.Config.DeviceFlag.Index(DeviceFlag.Id));
                    else
                    {
                        syncTaskStatus.SetFinishedUrl(Url.Action(MVC.Config.DeviceFlag.Index(DeviceFlag.Id)));
                        return RedirectToAction(MVC.Config.Logging.TaskStatus(syncTaskStatus.SessionId));
                    }
                else
                    return Json("OK", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                if (redirect)
                    throw;
                else
                    return Json($"Error: {ex.Message}", JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region Update Properties
        private void UpdateIconAndColour(DeviceFlag deviceFlag, string icon, string iconColour)
        {
            if (string.IsNullOrWhiteSpace(icon))
                throw new ArgumentNullException("Icon");
            if (string.IsNullOrWhiteSpace(iconColour))
                throw new ArgumentNullException("IconColour");

            if (deviceFlag.Icon != icon ||
                deviceFlag.IconColour != iconColour)
            {
                deviceFlag.Icon = icon;
                deviceFlag.IconColour = iconColour;
                DeviceFlagService.Update(Database, deviceFlag);
            }
        }
        private void UpdateIcon(DeviceFlag deviceFlag, string icon)
        {
            if (string.IsNullOrWhiteSpace(icon))
                throw new ArgumentNullException("Icon");

            if (deviceFlag.Icon != icon)
            {
                deviceFlag.Icon = icon;
                DeviceFlagService.Update(Database, deviceFlag);
            }
        }
        private void UpdateIconColour(DeviceFlag deviceFlag, string iconColour)
        {
            if (string.IsNullOrWhiteSpace(iconColour))
                throw new ArgumentNullException("IconColour");

            if (deviceFlag.IconColour != iconColour)
            {
                deviceFlag.IconColour = iconColour;
                DeviceFlagService.Update(Database, deviceFlag);
            }
        }

        private void UpdateName(DeviceFlag deviceFlag, string name)
        {
            if (deviceFlag.Name != name)
            {
                deviceFlag.Name = name;
                DeviceFlagService.Update(Database, deviceFlag);
            }
        }

        private void UpdateDescription(DeviceFlag deviceFlag, string description)
        {
            if (deviceFlag.Description != description)
            {
                deviceFlag.Description = description;
                DeviceFlagService.Update(Database, deviceFlag);
            }
        }

        private void UpdateOnAssignmentExpression(DeviceFlag deviceFlag, string onAssignmentExpression)
        {
            if (string.IsNullOrWhiteSpace(onAssignmentExpression))
            {
                deviceFlag.OnAssignmentExpression = null;
            }
            else
            {
                deviceFlag.OnAssignmentExpression = onAssignmentExpression.Trim();
            }
            // Invalidate Cache
            deviceFlag.OnAssignmentExpressionInvalidateCache();

            DeviceFlagService.Update(Database, deviceFlag);
        }

        private void UpdateOnUnassignmentExpression(DeviceFlag deviceFlag, string onUnassignmentExpression)
        {
            if (string.IsNullOrWhiteSpace(onUnassignmentExpression))
            {
                deviceFlag.OnUnassignmentExpression = null;
            }
            else
            {
                deviceFlag.OnUnassignmentExpression = onUnassignmentExpression.Trim();
            }
            // Invalidate Cache
            deviceFlag.OnUnassignmentExpressionInvalidateCache();

            DeviceFlagService.Update(Database, deviceFlag);
        }

        private ScheduledTaskStatus UpdateDevicesLinkedGroup(DeviceFlag deviceFlag, string devicesLinkedGroup, DateTime? filterBeginDate)
        {
            var configJson = ADManagedGroup.ValidConfigurationToJson(DeviceFlagDevicesManagedGroup.GetKey(deviceFlag), devicesLinkedGroup, filterBeginDate);

            if (deviceFlag.DevicesLinkedGroup != configJson)
            {
                deviceFlag.DevicesLinkedGroup = configJson;
                DeviceFlagService.Update(Database, deviceFlag);

                if (deviceFlag.DevicesLinkedGroup != null && DeviceFlagDevicesManagedGroup.TryGetManagedGroup(deviceFlag, out var managedGroup))
                {
                    // Sync Group
                    return ADManagedGroupsSyncTask.ScheduleSync(managedGroup);
                }
            }

            return null;
        }
        private ScheduledTaskStatus UpdateAssignedUserLinkedGroup(DeviceFlag deviceFlag, string assignedUserLinkedGroup, DateTime? filterBeginDate)
        {
            var configJson = ADManagedGroup.ValidConfigurationToJson(DeviceFlagDeviceAssignedUsersManagedGroup.GetKey(deviceFlag), assignedUserLinkedGroup, filterBeginDate);

            if (deviceFlag.DeviceUsersLinkedGroup != configJson)
            {
                deviceFlag.DeviceUsersLinkedGroup = configJson;
                DeviceFlagService.Update(Database, deviceFlag);

                if (deviceFlag.DeviceUsersLinkedGroup != null && DeviceFlagDeviceAssignedUsersManagedGroup.TryGetManagedGroup(deviceFlag, out var managedGroup))
                {
                    // Sync Group
                    return ADManagedGroupsSyncTask.ScheduleSync(managedGroup);
                }
            }

            return null;
        }
        #endregion

        #region Actions
        [DiscoAuthorizeAll(Claims.Config.DeviceFlag.Configure, Claims.Config.DeviceFlag.Delete)]
        public virtual ActionResult Delete(int id, bool? redirect = false)
        {
            try
            {
                var uf = Database.DeviceFlags.FirstOrDefault(f => f.Id == id);
                if (uf != null)
                {
                    var status = DeviceFlagDeleteTask.ScheduleNow(uf.Id);
                    status.SetFinishedUrl(Url.Action(MVC.Config.DeviceFlag.Index(null)));

                    if (redirect.HasValue && redirect.Value)
                        return RedirectToAction(MVC.Config.Logging.TaskStatus(status.SessionId));
                    else
                        return Json("OK", JsonRequestBehavior.AllowGet);
                }
                throw new ArgumentException("Invalid Device Flag Id", nameof(id));
            }
            catch (Exception ex)
            {
                if (redirect.HasValue && redirect.Value)
                    throw;
                else
                    return Json($"Error: {ex.Message}", JsonRequestBehavior.AllowGet);
            }
        }

        [DiscoAuthorizeAll(Claims.Config.DeviceFlag.Configure, Claims.Device.Actions.AddFlags, Claims.Device.Actions.RemoveFlags, Claims.Device.ShowFlagAssignments)]
        public virtual ActionResult BulkAssignDevices(int id, bool Override, string DeviceSerialNumbers = null, string Comments = null)
        {
            if (id < 0)
                throw new ArgumentNullException("id");
            var flag = Database.DeviceFlags.FirstOrDefault(f => f.Id == id);
            if (flag == null)
                throw new ArgumentException("Invalid Device Flag Id", nameof(id));

            var serialNumbers = DeviceSerialNumbers.Split(new string[] { Environment.NewLine, ",", ";" }, StringSplitOptions.RemoveEmptyEntries).Select(d => d.Trim()).Where(d => !string.IsNullOrEmpty(d)).ToList();

            var taskStatus = DeviceFlagBulkAssignTask.ScheduleBulkAssignDevices(flag, CurrentUser, Comments, serialNumbers, Override);
            taskStatus.SetFinishedUrl(Url.Action(MVC.Config.DeviceFlag.Index(flag.Id)));
            return RedirectToAction(MVC.Config.Logging.TaskStatus(taskStatus.SessionId));
        }
        [DiscoAuthorizeAll(Claims.Config.DeviceFlag.Configure, Claims.Device.Actions.AddFlags, Claims.Device.Actions.RemoveFlags, Claims.Device.ShowFlagAssignments)]
        public virtual ActionResult AssignedDevices(int id)
        {
            if (id < 0)
                throw new ArgumentNullException(nameof(id));
            var flag = Database.DeviceFlags.FirstOrDefault(f => f.Id == id);
            if (flag == null)
                throw new ArgumentException("Invalid Device Flag Id", nameof(id));

            var serialNumbers = Database
                .DeviceFlagAssignments
                .Where(a => a.DeviceFlagId == flag.Id && !a.RemovedDate.HasValue)
                .OrderBy(a => a.DeviceSerialNumber).Select(a => a.DeviceSerialNumber).ToList();

            return Json(serialNumbers, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Exporting

        [DiscoAuthorize(Claims.Config.DeviceFlag.Export)]
        public virtual ActionResult Export(ExportModel Model)
        {
            if (Model == null || Model.Options == null)
                throw new ArgumentNullException(nameof(Model));

            // Start Export
            var exportContext = new DeviceFlagExport(Model.Options);
            var taskContext = ExportTask.ScheduleNowCacheResult(exportContext, id => Url.Action(MVC.Config.DeviceFlag.Export(id, null, null)));

            // Try waiting for completion
            if (taskContext.TaskStatus.WaitUntilFinished(TimeSpan.FromSeconds(1)))
                return RedirectToAction(MVC.Config.DeviceFlag.Export(taskContext.Id, null, null));
            else
                return RedirectToAction(MVC.Config.Logging.TaskStatus(taskContext.TaskStatus.SessionId));
        }
        [DiscoAuthorize(Claims.Config.DeviceFlag.Export)]
        public virtual ActionResult ExportRetrieve(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentNullException(nameof(id));

            if (!ExportTask.TryFromCache(id, out var context))
                throw new ArgumentException("The export id specified is invalid, or the export data expired (60 minutes)", nameof(id));

            if (context.Result == null || context.Result.Result == null)
                throw new ArgumentException("The export session is still running, or failed to complete successfully", nameof(id));

            if (context.Result.RecordCount == 0)
                throw new ArgumentException("No records were found to export", nameof(id));

            var fileStream = context.Result.Result;

            return this.File(fileStream.GetBuffer(), 0, (int)fileStream.Length, context.Result.MimeType, context.Result.Filename);
        }

        #endregion
    }
}