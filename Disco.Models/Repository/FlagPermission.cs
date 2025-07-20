using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Disco.Models.Repository
{
    public class FlagPermission
    {
        private static readonly FlagPermission DefaultDeviceFlagPermissions = new FlagPermission(FlagType.Device, 0);
        private static readonly FlagPermission DefaultUserFlagPermissions = new FlagPermission(FlagType.User, 0);

        [JsonIgnore]
        public FlagType FlagType { get; }
        [JsonIgnore]
        public int FlagId { get; }
        public bool Inherit { get; set; }
        public HashSet<string> CanShowSubjectIds { get; }
        public HashSet<string> CanAssignSubjectIds { get; }
        public HashSet<string> CanEditSubjectIds { get; }
        public HashSet<string> CanRemoveSubjectIds { get; }

        private FlagPermission(FlagType flagType, int flagId)
        {
            FlagType = flagType;
            FlagId = flagId;
            Inherit = true;
            CanShowSubjectIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            CanAssignSubjectIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            CanEditSubjectIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            CanRemoveSubjectIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        public bool IsDefault()
        {
            return ReferenceEquals(this, DefaultDeviceFlagPermissions) ||
                ReferenceEquals(this, DefaultUserFlagPermissions);
        }

        public bool IsSimple()
        {
            return CanShowSubjectIds.SetEquals(CanAssignSubjectIds) &&
                   CanAssignSubjectIds.SetEquals(CanEditSubjectIds) &&
                   CanEditSubjectIds.SetEquals(CanRemoveSubjectIds);
        }

        public bool HasSubjects()
        {
            return CanShowSubjectIds.Count > 0 ||
                   CanAssignSubjectIds.Count > 0 ||
                   CanEditSubjectIds.Count > 0 ||
                   CanRemoveSubjectIds.Count > 0;
        }

        public HashSet<string> AllSubjects()
        {
            var allSubjects = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            allSubjects.UnionWith(CanShowSubjectIds);
            allSubjects.UnionWith(CanAssignSubjectIds);
            allSubjects.UnionWith(CanEditSubjectIds);
            allSubjects.UnionWith(CanRemoveSubjectIds);
            return allSubjects;
        }

        public static FlagPermission FromFlag(UserFlag userFlag)
        {
            if (userFlag.PermissionsJson == null)
                return DefaultUserFlagPermissions;

            var permission = new FlagPermission(FlagType.User, userFlag.Id);

            JsonConvert.PopulateObject(userFlag.PermissionsJson, permission);

            return permission;
        }

        public static FlagPermission FromFlag(DeviceFlag deviceFlag)
        {
            if (deviceFlag.PermissionsJson == null)
                return DefaultDeviceFlagPermissions;

            var permission = new FlagPermission(FlagType.Device, deviceFlag.Id);

            JsonConvert.PopulateObject(deviceFlag.PermissionsJson, permission);

            return permission;
        }

        public static FlagPermission Create(UserFlag userFlag, bool inherit, IEnumerable<string> canShow, IEnumerable<string> canAssign, IEnumerable<string> canEdit, IEnumerable<string> canRemove)
        {
            var permission = new FlagPermission(FlagType.User, userFlag.Id);
            return Create(permission, inherit, canShow, canAssign, canEdit, canRemove);
        }

        public static FlagPermission Create(DeviceFlag deviceFlag, bool inherit, IEnumerable<string> canShow, IEnumerable<string> canAssign, IEnumerable<string> canEdit, IEnumerable<string> canRemove)
        {
            var permission = new FlagPermission(FlagType.Device, deviceFlag.Id);
            return Create(permission, inherit, canShow, canAssign, canEdit, canRemove);
        }

        private static FlagPermission Create(FlagPermission permission, bool inherit, IEnumerable<string> canShow, IEnumerable<string> canAssign, IEnumerable<string> canEdit, IEnumerable<string> canRemove)
        {
            permission.Inherit = inherit;
            if (canShow != null)
                permission.CanShowSubjectIds.UnionWith(canShow);
            if (canAssign != null)
                permission.CanAssignSubjectIds.UnionWith(canAssign);
            if (canEdit != null)
                permission.CanEditSubjectIds.UnionWith(canEdit);
            if (canRemove != null)
                permission.CanRemoveSubjectIds.UnionWith(canRemove);
            return permission;
        }

        public string ToJson()
        {
            if (IsDefault())
                return null;
            return JsonConvert.SerializeObject(this);
        }
    }
}
