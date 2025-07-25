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
    public partial class UserFlagAssignmentController : AuthorizedDatabaseController
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
                    return Redirect($"{Url.Action(MVC.User.Show(userFlagAssignment.UserId))}#UserDetailTab-Flags");
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
        [DiscoAuthorizeAny(Claims.User.Actions.EditFlags)]
        public virtual ActionResult UpdateComments(int id, string Comments = null, bool? redirect = null)
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
            Database.Configuration.LazyLoadingEnabled = true;

            var userFlag = Database.UserFlags.Find(id);
            if (userFlag == null)
                throw new ArgumentException("Invalid User Flag Id", nameof(id));

            var user = Database.Users.Include(u => u.UserFlagAssignments).FirstOrDefault(u => u.UserId == UserId);
            if (user == null)
                throw new ArgumentException("Invalid User Id", nameof(UserId));

            if (!user.CanAddUserFlag(userFlag))
                throw new InvalidOperationException("Adding user flag is denied");

            var addingUser = Database.Users.Find(CurrentUser.UserId);

            var userFlagAssignment = user.OnAddUserFlag(Database, userFlag, addingUser, Comments);

            Database.SaveChanges();

            return Redirect($"{Url.Action(MVC.User.Show(user.UserId))}#UserDetailTab-Flags");
        }

        [DiscoAuthorizeAny(Claims.User.Actions.RemoveFlags)]
        public virtual ActionResult RemoveUser(int id)
        {
            Database.Configuration.LazyLoadingEnabled = true;

            var userFlagAssignment = Database.UserFlagAssignments.FirstOrDefault(a => a.Id == id);
            if (userFlagAssignment == null)
                throw new ArgumentException("Invalid User Flag Assignment Id", nameof(id));

            if (!userFlagAssignment.CanRemove())
                throw new InvalidOperationException("Removing user flag assignment is denied");

            var removingUser = Database.Users.Find(CurrentUser.UserId);

            userFlagAssignment.OnRemove(Database, removingUser);
            Database.SaveChanges();

            return Redirect($"{Url.Action(MVC.User.Show(userFlagAssignment.UserId))}#UserDetailTab-Flags");
        }

        #endregion

    }
}