using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Services.Authorization;
using Disco.Services.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.BI.Extensions
{
    public static class UserFlagActionExtensions
    {

        #region Edit Comments
        public static bool CanEditComments(this UserFlagAssignment fa)
        {
            return UserService.CurrentAuthorization.Has(Claims.User.Actions.EditFlags);
        }
        public static void OnEditComments(this UserFlagAssignment fa, string Comments)
        {
            if (!fa.CanEditComments())
                throw new InvalidOperationException("Editing comments for user flags is denied");

            fa.Comments = string.IsNullOrWhiteSpace(Comments) ? null : Comments.Trim();
        }
        #endregion

        #region Remove
        public static bool CanRemove(this UserFlagAssignment fa)
        {
            if (fa.RemovedDate.HasValue)
                return false;

            return UserService.CurrentAuthorization.Has(Claims.User.Actions.RemoveFlags);
        }
        public static void OnRemove(this UserFlagAssignment fa, User Technician)
        {
            if (!fa.CanRemove())
                throw new InvalidOperationException("Removing user flags is denied");

            fa.RemovedDate = DateTime.Now;
            fa.RemovedUserId = Technician.UserId;
        }
        #endregion

        #region Add
        public static bool CanAddUserFlags(this User u)
        {
            return UserService.CurrentAuthorization.Has(Claims.User.Actions.AddFlags);
        }
        public static bool CanAddUserFlag(this User u, UserFlag flag)
        {
            // Shortcut
            if (!u.CanAddUserFlags())
                return false;

            // Already has User Flag?
            if (u.UserFlagAssignments.Any(fa => !fa.RemovedDate.HasValue && fa.UserFlagId == flag.Id))
                return false;

            return true;
        }
        public static UserFlagAssignment OnAddUserFlag(this User u, DiscoDataContext Database, UserFlag flag, User Technician, string Comments)
        {
            if (!u.CanAddUserFlag(flag))
                throw new InvalidOperationException("Adding user flag is denied");

            var fa = new UserFlagAssignment()
            {
                UserFlagId = flag.Id,
                UserId = u.UserId,
                AddedDate = DateTime.Now,
                AddedUserId = Technician.UserId,
                Comments = string.IsNullOrWhiteSpace(Comments) ? null : Comments.Trim()
            };

            Database.UserFlagAssignments.Add(fa);
            return fa;
        }
        #endregion

    }
}
