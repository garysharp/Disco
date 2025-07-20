using Disco.Models.Repository;
using System.Collections.Generic;

namespace Disco.Web.Areas.API.Models.Shared
{
    public class FlagPermissionModel
    {
        public bool IsOverride { get; set; }
        public bool Inherit { get; set; }
        public List<string> CanShow { get; set; }
        public List<string> CanAssign { get; set; }
        public List<string> CanEdit { get; set; }
        public List<string> CanRemove { get; set; }

        public FlagPermission ToFlagPermission(UserFlag userFlag)
            => FlagPermission.Create(userFlag, Inherit, CanShow, CanAssign, CanEdit, CanRemove);

        public FlagPermission ToFlagPermission(DeviceFlag deviceFlag)
            => FlagPermission.Create(deviceFlag, Inherit, CanShow, CanAssign, CanEdit, CanRemove);
    }
}
