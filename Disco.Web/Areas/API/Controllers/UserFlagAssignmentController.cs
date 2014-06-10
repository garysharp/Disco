using Disco.BI.Extensions;
using Disco.Models.Repository;
using Disco.Services.Authorization;
using Disco.Services.Users.UserFlags;
using Disco.Services.Web;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Disco.Web.Areas.API.Controllers
{
    public partial class UserFlagAssignmentController : AuthorizedDatabaseController
    {
        const string pComments = "comments";

        public virtual ActionResult Update(int id, string key, string value = null, Nullable<bool> redirect = null)
        {
            try
            {
                if (id < 0)
                    throw new ArgumentOutOfRangeException("id");
                if (string.IsNullOrEmpty(key))
                    throw new ArgumentNullException("key");
                var userFlagAssignment = Database.UserFlagAssignments.FirstOrDefault(a => a.Id == id);
                if (userFlagAssignment != null)
                {
                    switch (key.ToLower())
                    {
                        case pComments:
                            UpdateComments(userFlagAssignment, value);
                            break;
                        default:
                            throw new Exception("Invalid Update Key");
                    }
                }
                else
                {
                    throw new Exception("Invalid User Flag Assignment Id");
                }
                if (redirect.HasValue && redirect.Value)
                    return Redirect(string.Format("{0}#UserDetailTab-Flags", Url.Action(MVC.User.Show(userFlagAssignment.UserId))));
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
        [DiscoAuthorizeAny(Claims.User.Actions.EditFlags)]
        public virtual ActionResult UpdateComments(int id, string Comments = null, Nullable<bool> redirect = null)
        {
            return Update(id, pComments, Comments, redirect);
        }
        #endregion

        #region Update Properties
        private void UpdateComments(UserFlagAssignment userFlagAssignment, string Comments)
        {
            if (!userFlagAssignment.CanEditComments())
                throw new InvalidOperationException("Editing comments for user flags is denied");

            userFlagAssignment.OnEditComments(Comments);
            Database.SaveChanges();
        }
        #endregion

        #region Actions

        [DiscoAuthorizeAny(Claims.User.Actions.AddFlags)]
        public virtual ActionResult AddUser(int id, string UserId, string Comments)
        {
            var userFlag = UserFlagService.GetUserFlag(id);
            if (userFlag == null)
                throw new ArgumentException("Invalid User Flag Id", "id");

            var user = Database.Users.Include("UserFlagAssignments").FirstOrDefault(u => u.UserId == UserId);
            if (user == null)
                throw new ArgumentException("Invalid User Id", "UserId");

            if (!user.CanAddUserFlag(userFlag))
                throw new InvalidOperationException("Adding user flag is denied");

            var userFlagAssignment = user.OnAddUserFlag(Database, userFlag, CurrentUser, Comments);
            Database.SaveChanges();

            return Redirect(string.Format("{0}#UserDetailTab-Flags", Url.Action(MVC.User.Show(user.UserId))));
        }

        [DiscoAuthorizeAny(Claims.User.Actions.RemoveFlags)]
        public virtual ActionResult RemoveUser(int id)
        {
            var userFlagAssignment = Database.UserFlagAssignments.FirstOrDefault(a => a.Id == id);
            if (userFlagAssignment == null)
                throw new ArgumentException("Invalid User Flag Assignment Id", "id");

            if (!userFlagAssignment.CanRemove())
                throw new InvalidOperationException("Removing user flag assignment is denied");

            userFlagAssignment.OnRemove(CurrentUser);
            Database.SaveChanges();

            return Redirect(string.Format("{0}#UserDetailTab-Flags", Url.Action(MVC.User.Show(userFlagAssignment.UserId))));
        }

        #endregion

    }
}