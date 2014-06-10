using Disco.Models.Repository;
using Disco.Services.Authorization;
using Disco.Services.Users.UserFlags;
using Disco.Services.Web;
using System;
using System.Web.Mvc;

namespace Disco.Web.Areas.API.Controllers
{
    public partial class UserFlagController : AuthorizedDatabaseController
    {
        const string pName = "name";
        const string pDescription = "description";
        const string pIcon = "icon";
        const string pIconColour = "iconcolour";

        [DiscoAuthorize(Claims.Config.UserFlag.Configure)]
        public virtual ActionResult Update(int id, string key, string value = null, Nullable<bool> redirect = null)
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
                    return Json("OK", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                if (redirect.HasValue && redirect.Value)
                    throw;
                else
                    return Json(string.Format("Error: {0}", ex.Message), JsonRequestBehavior.AllowGet);
            }
        }

        #region Update Shortcut Methods
        [DiscoAuthorize(Claims.Config.UserFlag.Configure)]
        public virtual ActionResult UpdateName(int id, string FlagName = null, Nullable<bool> redirect = null)
        {
            return Update(id, pName, FlagName, redirect);
        }

        [DiscoAuthorize(Claims.Config.UserFlag.Configure)]
        public virtual ActionResult UpdateDescription(int id, string Description = null, Nullable<bool> redirect = null)
        {
            return Update(id, pDescription, Description, redirect);
        }

        [DiscoAuthorize(Claims.Config.UserFlag.Configure)]
        public virtual ActionResult UpdateIcon(int id, string Icon = null, Nullable<bool> redirect = null)
        {
            return Update(id, pIcon, Icon, redirect);
        }

        [DiscoAuthorize(Claims.Config.UserFlag.Configure)]
        public virtual ActionResult UpdateIconColour(int id, string IconColour = null, Nullable<bool> redirect = null)
        {
            return Update(id, pIconColour, IconColour, redirect);
        }

        [DiscoAuthorize(Claims.Config.UserFlag.Configure)]
        public virtual ActionResult UpdateIconAndColour(int id, string Icon = null, string IconColour = null, bool redirect = false)
        {
            try
            {
                if (id < 0)
                    throw new ArgumentOutOfRangeException("id");

                var UserFlag = Database.UserFlags.Find(id);
                if (UserFlag != null)
                {
                    UpdateIconAndColour(UserFlag, Icon, IconColour);
                }
                else
                {
                    return Json("Invalid User Flag Id", JsonRequestBehavior.AllowGet);
                }
                if (redirect)
                    return RedirectToAction(MVC.Config.UserFlag.Index(UserFlag.Id));
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
        #endregion

        #region Update Properties
        private void UpdateIconAndColour(UserFlag UserFlag, string Icon, string IconColour)
        {
            if (string.IsNullOrWhiteSpace(Icon))
                throw new ArgumentNullException("Icon");
            if (string.IsNullOrWhiteSpace(IconColour))
                throw new ArgumentNullException("IconColour");

            UserFlag.Icon = Icon;
            UserFlag.IconColour = IconColour;
            UserFlagService.Update(Database, UserFlag);
        }
        private void UpdateIcon(UserFlag UserFlag, string Icon)
        {
            if (string.IsNullOrWhiteSpace(Icon))
                throw new ArgumentNullException("Icon");

            UserFlag.Icon = Icon;
            UserFlagService.Update(Database, UserFlag);
        }
        private void UpdateIconColour(UserFlag UserFlag, string IconColour)
        {
            if (string.IsNullOrWhiteSpace(IconColour))
                throw new ArgumentNullException("IconColour");

            UserFlag.IconColour = IconColour;
            UserFlagService.Update(Database, UserFlag);
        }

        private void UpdateName(UserFlag UserFlag, string Name)
        {
            UserFlag.Name = Name;
            UserFlagService.Update(Database, UserFlag);
        }

        private void UpdateDescription(UserFlag UserFlag, string Description)
        {
            UserFlag.Description = Description;
            UserFlagService.Update(Database, UserFlag);
        }
        #endregion

        #region Actions
        [DiscoAuthorize(Claims.Config.UserFlag.Delete)]
        public virtual ActionResult Delete(int id, Nullable<bool> redirect = false)
        {
            try
            {
                var jq = Database.UserFlags.Find(id);
                if (jq != null)
                {

                    var status = UserFlagDeleteTask.ScheduleNow(id);
                    status.SetFinishedUrl(Url.Action(MVC.Config.UserFlag.Index(null)));

                    if (redirect.HasValue && redirect.Value)
                        return RedirectToAction(MVC.Config.Logging.TaskStatus(status.SessionId));
                    else
                        return Json("OK", JsonRequestBehavior.AllowGet);
                }
                throw new Exception("Invalid User Flag Id");
            }
            catch (Exception ex)
            {
                if (redirect.HasValue && redirect.Value)
                    throw;
                else
                    return Json(string.Format("Error: {0}", ex.Message), JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
    }
}