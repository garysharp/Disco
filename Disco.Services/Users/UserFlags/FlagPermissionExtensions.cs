using Disco.Models.Repository;
using Disco.Services.Authorization;
using Disco.Services.Devices.DeviceFlags;
using Disco.Services.Users;
using Disco.Services.Users.UserFlags;
using System.Collections.Generic;
using System.Linq;

namespace Disco
{
    public static class FlagPermissionExtensions
    {
        public static bool CanShow(this FlagPermission permission)
        {
            var authorization = UserService.CurrentAuthorization;

            // inherited permission
            if (permission.Inherit &&
                authorization.Has(permission.FlagType == FlagType.User ? Claims.User.ShowFlagAssignments : Claims.Device.ShowFlagAssignments))
            {
                return true;
            }

            // permission override
            if (permission != null && (
                permission.CanShowSubjectIds.Contains(authorization.User.UserId) ||
                permission.CanShowSubjectIds.Overlaps(authorization.GroupMembership) ||
                permission.CanShowSubjectIds.Overlaps(authorization.RoleTokens.Select(r => $"[{r.Role.Id}]"))
                ))
            {
                return true;
            }

            return false;
        }
        public static bool CanAssign(this FlagPermission permission)
        {
            var authorization = UserService.CurrentAuthorization;

            // inherited permission
            if (permission.Inherit &&
                authorization.Has(permission.FlagType == FlagType.User ? Claims.User.Actions.AddFlags : Claims.Device.Actions.AddFlags))
            {
                return true;
            }

            // permission override
            if (permission != null && (
                permission.CanAssignSubjectIds.Contains(authorization.User.UserId) ||
                permission.CanAssignSubjectIds.Overlaps(authorization.GroupMembership) ||
                permission.CanAssignSubjectIds.Overlaps(authorization.RoleTokens.Select(r => $"[{r.Role.Id}]"))
                ))
            {
                return true;
            }

            return false;
        }
        public static bool CanEdit(this FlagPermission permission)
        {
            var authorization = UserService.CurrentAuthorization;

            // inherited permission
            if (permission.Inherit &&
                authorization.Has(permission.FlagType == FlagType.User ? Claims.User.Actions.EditFlags : Claims.Device.Actions.EditFlags))
            {
                return true;
            }

            // permission override
            if (permission != null && (
                permission.CanEditSubjectIds.Contains(authorization.User.UserId) ||
                permission.CanEditSubjectIds.Overlaps(authorization.GroupMembership) ||
                permission.CanEditSubjectIds.Overlaps(authorization.RoleTokens.Select(r => $"[{r.Role.Id}]"))
                ))
            {
                return true;
            }

            return false;
        }
        public static bool CanRemove(this FlagPermission permission)
        {
            var authorization = UserService.CurrentAuthorization;

            // inherited permission
            if (permission.Inherit &&
                authorization.Has(permission.FlagType == FlagType.User ? Claims.User.Actions.RemoveFlags : Claims.Device.Actions.RemoveFlags))
            {
                return true;
            }

            // permission override
            if (permission != null && (
                permission.CanRemoveSubjectIds.Contains(authorization.User.UserId) ||
                permission.CanRemoveSubjectIds.Overlaps(authorization.GroupMembership) ||
                permission.CanRemoveSubjectIds.Overlaps(authorization.RoleTokens.Select(r => $"[{r.Role.Id}]"))
                ))
            {
                return true;
            }

            return false;
        }

        public static bool CanShowAny(this IEnumerable<UserFlagAssignment> assignments)
        {
            if (assignments == null)
                return false;

            foreach (var assignment in assignments)
            {
                if (assignment.RemovedDate.HasValue)
                    continue;

                var (_, permission) = UserFlagService.GetUserFlag(assignment.UserFlagId);

                if (permission.CanShow())
                    return true;
            }

            return false;
        }

        public static bool CanShowAny(this IEnumerable<DeviceFlagAssignment> assignments)
        {
            if (assignments == null)
                return false;

            foreach (var assignment in assignments)
            {
                if (assignment.RemovedDate.HasValue)
                    continue;

                var (_, permission) = DeviceFlagService.GetDeviceFlag(assignment.DeviceFlagId);

                if (permission.CanShow())
                    return true;
            }

            return false;
        }
    }
}
