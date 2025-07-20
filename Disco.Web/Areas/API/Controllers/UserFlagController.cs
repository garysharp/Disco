using Disco.Models.Repository;
using Disco.Services;
using Disco.Services.Authorization;
using Disco.Services.Exporting;
using Disco.Services.Interop.ActiveDirectory;
using Disco.Services.Tasks;
using Disco.Services.Users.UserFlags;
using Disco.Services.Web;
using Disco.Web.Areas.API.Models.Shared;
using Disco.Web.Areas.Config.Models.UserFlag;
using Disco.Web.Extensions;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Disco.Web.Areas.API.Controllers
{
    public partial class UserFlagController : AuthorizedDatabaseController
    {
        const string pName = "name";
        const string pDescription = "description";
        const string pIcon = "icon";
        const string pIconColour = "iconcolour";
        const string pOnAssignmentExpression = "onassignmentexpression";
        const string pOnUnassignmentExpression = "onunassignmentexpression";

        [DiscoAuthorize(Claims.Config.UserFlag.Configure)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult Update(int id, string key, string value = null, bool? redirect = null)
        {
            Authorization.Require(Claims.Config.UserFlag.Configure);

            try
            {
                if (id < 0)
                    throw new ArgumentOutOfRangeException("id");
                if (string.IsNullOrEmpty(key))
                    throw new ArgumentNullException("key");
                var flag = Database.UserFlags.Find(id);
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
                    throw new Exception("Invalid User Flag Id");
                }
                if (redirect.HasValue && redirect.Value)
                    return RedirectToAction(MVC.Config.UserFlag.Index(flag.Id));
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
        [DiscoAuthorize(Claims.Config.UserFlag.Configure)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult UpdateName(int id, string FlagName = null, bool? redirect = null)
        {
            return Update(id, pName, FlagName, redirect);
        }

        [DiscoAuthorize(Claims.Config.UserFlag.Configure)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult UpdateDescription(int id, string Description = null, bool? redirect = null)
        {
            return Update(id, pDescription, Description, redirect);
        }

        [DiscoAuthorize(Claims.Config.UserFlag.Configure)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult UpdateIcon(int id, string icon = null, bool? redirect = null)
        {
            return Update(id, pIcon, icon, redirect);
        }

        [DiscoAuthorize(Claims.Config.UserFlag.Configure)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult UpdateIconColour(int id, string iconColour = null, bool? redirect = null)
        {
            return Update(id, pIconColour, iconColour, redirect);
        }

        [DiscoAuthorize(Claims.Config.UserFlag.Configure)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult UpdateIconAndColour(int id, string icon = null, string iconColour = null, bool redirect = false)
        {
            try
            {
                if (id < 0)
                    throw new ArgumentOutOfRangeException(nameof(id));

                var UserFlag = Database.UserFlags.Find(id);
                if (UserFlag != null)
                {
                    UpdateIconAndColour(UserFlag, icon, iconColour);
                }
                else
                {
                    throw new ArgumentException("Invalid User Flag Id", nameof(id));
                }
                if (redirect)
                    return RedirectToAction(MVC.Config.UserFlag.Index(UserFlag.Id));
                else
                    return Ok();
            }
            catch (Exception ex)
            {
                if (redirect)
                    throw;
                else
                    return BadRequest(ex.Message);
            }
        }
        [DiscoAuthorize(Claims.Config.UserFlag.Configure)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult UpdateOnAssignmentExpression(int id, string OnAssignmentExpression = null, bool redirect = false)
        {
            return Update(id, pOnAssignmentExpression, OnAssignmentExpression, redirect);
        }
        [DiscoAuthorize(Claims.Config.UserFlag.Configure)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult UpdateOnUnassignmentExpression(int id, string OnUnassignmentExpression = null, bool redirect = false)
        {
            return Update(id, pOnUnassignmentExpression, OnUnassignmentExpression, redirect);
        }
        [DiscoAuthorize(Claims.Config.UserFlag.Configure)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult UpdateAssignedUsersLinkedGroup(int id, string GroupId = null, DateTime? FilterBeginDate = null, bool redirect = false)
        {
            try
            {
                if (id < 0)
                    throw new ArgumentOutOfRangeException(nameof(id));

                var UserFlag = Database.UserFlags.Find(id);
                if (UserFlag == null)
                    throw new ArgumentException("Invalid User Flag Id", nameof(id));


                var syncTaskStatus = UpdateAssignedUsersLinkedGroup(UserFlag, GroupId, FilterBeginDate);
                if (redirect)
                    if (syncTaskStatus == null)
                        return RedirectToAction(MVC.Config.UserFlag.Index(UserFlag.Id));
                    else
                    {
                        syncTaskStatus.SetFinishedUrl(Url.Action(MVC.Config.UserFlag.Index(UserFlag.Id)));
                        return RedirectToAction(MVC.Config.Logging.TaskStatus(syncTaskStatus.SessionId));
                    }
                else
                    return Ok();
            }
            catch (Exception ex)
            {
                if (redirect)
                    throw;
                else
                    return BadRequest(ex.Message);
            }
        }
        [DiscoAuthorize(Claims.Config.UserFlag.Configure)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult UpdateAssignedUserDevicesLinkedGroup(int id, string GroupId = null, DateTime? FilterBeginDate = null, bool redirect = false)
        {
            try
            {
                if (id < 0)
                    throw new ArgumentOutOfRangeException(nameof(id));

                var UserFlag = Database.UserFlags.Find(id);
                if (UserFlag == null)
                    throw new ArgumentException("Invalid User Flag Id", nameof(id));


                var syncTaskStatus = UpdateAssignedUserDevicesLinkedGroup(UserFlag, GroupId, FilterBeginDate);
                if (redirect)
                    if (syncTaskStatus == null)
                        return RedirectToAction(MVC.Config.UserFlag.Index(UserFlag.Id));
                    else
                    {
                        syncTaskStatus.SetFinishedUrl(Url.Action(MVC.Config.UserFlag.Index(UserFlag.Id)));
                        return RedirectToAction(MVC.Config.Logging.TaskStatus(syncTaskStatus.SessionId));
                    }
                else
                    return Ok();
            }
            catch (Exception ex)
            {
                if (redirect)
                    throw;
                else
                    return BadRequest(ex.Message);
            }
        }
        #endregion

        #region Update Properties
        private void UpdateIconAndColour(UserFlag UserFlag, string Icon, string IconColour)
        {
            if (string.IsNullOrWhiteSpace(Icon))
                throw new ArgumentNullException("Icon");
            if (string.IsNullOrWhiteSpace(IconColour))
                throw new ArgumentNullException("IconColour");

            if (UserFlag.Icon != Icon ||
                UserFlag.IconColour != IconColour)
            {
                UserFlag.Icon = Icon;
                UserFlag.IconColour = IconColour;
                UserFlagService.Update(Database, UserFlag);
            }
        }
        private void UpdateIcon(UserFlag UserFlag, string Icon)
        {
            if (string.IsNullOrWhiteSpace(Icon))
                throw new ArgumentNullException("Icon");

            if (UserFlag.Icon != Icon)
            {
                UserFlag.Icon = Icon;
                UserFlagService.Update(Database, UserFlag);
            }
        }
        private void UpdateIconColour(UserFlag UserFlag, string IconColour)
        {
            if (string.IsNullOrWhiteSpace(IconColour))
                throw new ArgumentNullException("IconColour");

            if (UserFlag.IconColour != IconColour)
            {
                UserFlag.IconColour = IconColour;
                UserFlagService.Update(Database, UserFlag);
            }
        }

        private void UpdateName(UserFlag UserFlag, string Name)
        {
            if (UserFlag.Name != Name)
            {
                UserFlag.Name = Name;
                UserFlagService.Update(Database, UserFlag);
            }
        }

        private void UpdateDescription(UserFlag UserFlag, string Description)
        {
            if (UserFlag.Description != Description)
            {
                UserFlag.Description = Description;
                UserFlagService.Update(Database, UserFlag);
            }
        }

        private void UpdateOnAssignmentExpression(UserFlag UserFlag, string OnAssignmentExpression)
        {
            if (string.IsNullOrWhiteSpace(OnAssignmentExpression))
            {
                UserFlag.OnAssignmentExpression = null;
            }
            else
            {
                UserFlag.OnAssignmentExpression = OnAssignmentExpression.Trim();
            }
            // Invalidate Cache
            UserFlag.OnAssignmentExpressionInvalidateCache();

            UserFlagService.Update(Database, UserFlag);
        }

        private void UpdateOnUnassignmentExpression(UserFlag UserFlag, string OnUnassignmentExpression)
        {
            if (string.IsNullOrWhiteSpace(OnUnassignmentExpression))
            {
                UserFlag.OnUnassignmentExpression = null;
            }
            else
            {
                UserFlag.OnUnassignmentExpression = OnUnassignmentExpression.Trim();
            }
            // Invalidate Cache
            UserFlag.OnUnassignmentExpressionInvalidateCache();

            UserFlagService.Update(Database, UserFlag);
        }

        private ScheduledTaskStatus UpdateAssignedUsersLinkedGroup(UserFlag UserFlag, string AssignedUsersLinkedGroup, DateTime? FilterBeginDate)
        {
            var configJson = ADManagedGroup.ValidConfigurationToJson(UserFlagUsersManagedGroup.GetKey(UserFlag), AssignedUsersLinkedGroup, FilterBeginDate);

            if (UserFlag.UsersLinkedGroup != configJson)
            {
                UserFlag.UsersLinkedGroup = configJson;
                UserFlagService.Update(Database, UserFlag);

                if (UserFlag.UsersLinkedGroup != null)
                {
                    // Sync Group
                    if (UserFlagUsersManagedGroup.TryGetManagedGroup(UserFlag, out var managedGroup))
                    {
                        return ADManagedGroupsSyncTask.ScheduleSync(managedGroup);
                    }
                }
            }

            return null;
        }
        private ScheduledTaskStatus UpdateAssignedUserDevicesLinkedGroup(UserFlag UserFlag, string AssignedUserDevicesLinkedGroup, DateTime? FilterBeginDate)
        {
            var configJson = ADManagedGroup.ValidConfigurationToJson(UserFlagUserDevicesManagedGroup.GetKey(UserFlag), AssignedUserDevicesLinkedGroup, FilterBeginDate);

            if (UserFlag.UserDevicesLinkedGroup != configJson)
            {
                UserFlag.UserDevicesLinkedGroup = configJson;
                UserFlagService.Update(Database, UserFlag);

                if (UserFlag.UserDevicesLinkedGroup != null)
                {
                    // Sync Group
                    if (UserFlagUserDevicesManagedGroup.TryGetManagedGroup(UserFlag, out var managedGroup))
                    {
                        return ADManagedGroupsSyncTask.ScheduleSync(managedGroup);
                    }
                }
            }

            return null;
        }
        #endregion

        #region Actions
        [DiscoAuthorizeAll(Claims.Config.UserFlag.Configure, Claims.Config.UserFlag.Delete)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult Delete(int id, bool? redirect = false)
        {
            try
            {
                var uf = Database.UserFlags.FirstOrDefault(f => f.Id == id);
                if (uf != null)
                {
                    var status = UserFlagDeleteTask.ScheduleNow(uf.Id);
                    status.SetFinishedUrl(Url.Action(MVC.Config.UserFlag.Index(null)));

                    if (redirect.HasValue && redirect.Value)
                        return RedirectToAction(MVC.Config.Logging.TaskStatus(status.SessionId));
                    else
                        return Ok();
                }
                throw new Exception("Invalid User Flag Id");
            }
            catch (Exception ex)
            {
                if (redirect.HasValue && redirect.Value)
                    throw;
                else
                    return BadRequest(ex.Message);
            }
        }

        [DiscoAuthorizeAll(Claims.Config.UserFlag.Configure, Claims.User.Actions.AddFlags, Claims.User.Actions.RemoveFlags, Claims.User.ShowFlagAssignments)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult BulkAssignUsers(int id, bool Override, string UserIds = null, string Comments = null)
        {
            if (id < 0)
                throw new ArgumentNullException("id");
            var userFlag = Database.UserFlags.FirstOrDefault(f => f.Id == id);
            if (userFlag == null)
                throw new ArgumentException("Invalid User Flag Id", "id");

            var userIds = UserIds.Split(new string[] { Environment.NewLine, ",", ";" }, StringSplitOptions.RemoveEmptyEntries).Select(d => d.Trim()).Where(d => !string.IsNullOrEmpty(d)).ToList();

            var taskStatus = UserFlagBulkAssignTask.ScheduleBulkAssignUsers(userFlag, CurrentUser, Comments, userIds, Override);
            taskStatus.SetFinishedUrl(Url.Action(MVC.Config.UserFlag.Index(userFlag.Id)));
            return RedirectToAction(MVC.Config.Logging.TaskStatus(taskStatus.SessionId));
        }
        [DiscoAuthorizeAll(Claims.Config.UserFlag.Configure, Claims.User.Actions.AddFlags, Claims.User.Actions.RemoveFlags, Claims.User.ShowFlagAssignments)]
        public virtual ActionResult AssignedUsers(int id)
        {
            if (id < 0)
                throw new ArgumentNullException("id");
            var userFlag = Database.UserFlags.FirstOrDefault(f => f.Id == id);
            if (userFlag == null)
                throw new ArgumentException("Invalid User Flag Id", "id");

            var assignedUsers = Database.UserFlagAssignments.Where(a => a.UserFlagId == userFlag.Id && !a.RemovedDate.HasValue).OrderBy(a => a.UserId).Select(a => a.UserId).ToList();

            return Json(assignedUsers, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Exporting

        [DiscoAuthorize(Claims.Config.UserFlag.Export)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult Export(ExportModel model)
        {
            if (model == null || model.Options == null)
                throw new ArgumentNullException(nameof(model));

            Database.DiscoConfiguration.UserFlags.LastExportOptions = model.Options;
            Database.SaveChanges();

            // Start Export
            var exportContext = new UserFlagExport(model.Options);
            var taskContext = ExportTask.ScheduleNowCacheResult(exportContext, id => Url.Action(MVC.Config.UserFlag.Export(id, null, null)));

            // Try waiting for completion
            if (taskContext.TaskStatus.WaitUntilFinished(TimeSpan.FromSeconds(2)))
                return RedirectToAction(MVC.Config.UserFlag.Export(taskContext.Id, null, null));
            else
                return RedirectToAction(MVC.Config.Logging.TaskStatus(taskContext.TaskStatus.SessionId));
        }

        [DiscoAuthorize(Claims.Config.UserFlag.Export)]
        public virtual ActionResult ExportRetrieve(Guid id)
        {
            if (!ExportTask.TryFromCache(id, out var context))
                throw new ArgumentException("The export id specified is invalid, or the export data expired (60 minutes)", nameof(id));

            if (context.Result == null || context.Result.Result == null)
                throw new ArgumentException("The export session is still running, or failed to complete successfully", nameof(id));

            if (context.Result.RecordCount == 0)
                throw new ArgumentException("No records were found to export", nameof(id));

            var fileStream = context.Result.Result;

            return this.File(fileStream.GetBuffer(), 0, (int)fileStream.Length, context.Result.MimeType, context.Result.Filename);
        }

        [DiscoAuthorizeAll(Claims.Config.ManageSavedExports, Claims.Config.UserFlag.Export)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult SaveExport(ExportModel model)
        {
            Database.DiscoConfiguration.UserFlags.LastExportOptions = model.Options;

            var export = new UserFlagExport(model.Options);
            var savedExport = SavedExports.SaveExport(export, Database, CurrentUser);

            return RedirectToAction(MVC.Config.Export.Create(savedExport.Id));
        }

        [DiscoAuthorize(Claims.Config.UserFlag.Configure)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult Permission(int id, FlagPermissionModel model = null)
        {
            var userFlag = Database.UserFlags.Find(id);

            if (userFlag == null)
                return NotFound();

            if (model == null || !model.IsOverride)
                userFlag.Permissions = null;
            else
                userFlag.Permissions = model.ToFlagPermission(userFlag);

            UserFlagService.Update(Database, userFlag);

            return RedirectToAction(MVC.Config.UserFlag.Index(userFlag.Id));
        }
        #endregion
    }
}