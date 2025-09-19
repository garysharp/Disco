using Disco.Services;
using Disco.Services.Web;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace Disco.Web.Areas.API.Controllers
{
    public partial class UserFlagAssignmentController : AuthorizedDatabaseController
    {
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult Edit(int id, string comments, DateTime? removeDate, bool? redirect = null)
        {
            try
            {
                if (id < 0)
                    throw new ArgumentOutOfRangeException(nameof(id));

                var userFlagAssignment = Database.UserFlagAssignments
                    .Include(a => a.UserFlag)
                    .FirstOrDefault(a => a.Id == id)
                    ?? throw new Exception("Invalid User Flag Assignment Id");

                if (!userFlagAssignment.CanEdit())
                    throw new InvalidOperationException("Editing comments for user flags is denied");

                userFlagAssignment.OnEdit(comments, removeDate);
                Database.SaveChanges();

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

        #region Actions

        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult AddUser(int id, string UserId, string Comments, DateTime? RemoveDate)
        {
            Database.Configuration.LazyLoadingEnabled = true;

            var userFlag = Database.UserFlags.Find(id)
                ?? throw new ArgumentException("Invalid User Flag Id", nameof(id));

            var user = Database.Users
                .Include(u => u.UserFlagAssignments)
                .FirstOrDefault(u => u.UserId == UserId)
                ?? throw new ArgumentException("Invalid User Id", nameof(UserId));

            if (!user.CanAddUserFlag(userFlag))
                return Unauthorized("Adding user flag is denied");

            if (RemoveDate.HasValue && RemoveDate.Value < DateTime.Today.AddDays(1))
                RemoveDate = null;

            if (user.CanRemoveUserFlag(userFlag))
                user.OnAddUserFlag(Database, userFlag, Comments, RemoveDate);
            else
                user.OnAddUserFlag(Database, userFlag, Comments);

            Database.SaveChanges();

            return Redirect($"{Url.Action(MVC.User.Show(user.UserId))}#UserDetailTab-Flags");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult RemoveUser(int id)
        {
            Database.Configuration.LazyLoadingEnabled = true;

            var userFlagAssignment = Database.UserFlagAssignments
                .Include(a => a.UserFlag)
                .FirstOrDefault(a => a.Id == id)
                ?? throw new ArgumentException("Invalid User Flag Assignment Id", nameof(id));

            if (!userFlagAssignment.CanRemove())
                return Unauthorized("Removing user flag assignment is denied");

            userFlagAssignment.OnRemove(Database);
            Database.SaveChanges();

            return Redirect($"{Url.Action(MVC.User.Show(userFlagAssignment.UserId))}#UserDetailTab-Flags");
        }

        #endregion

    }
}
