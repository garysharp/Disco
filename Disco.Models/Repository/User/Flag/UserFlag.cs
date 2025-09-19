using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Disco.Models.Repository
{
    public class UserFlag
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; }

        [StringLength(500), DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [Required, StringLength(25)]
        public string Icon { get; set; }
        [Required, StringLength(10)]
        public string IconColour { get; set; }

        public string UsersLinkedGroup { get; set; }
        public string UserDevicesLinkedGroup { get; set; }

        [DataType(DataType.MultilineText)]
        public string OnAssignmentExpression { get; set; }
        [DataType(DataType.MultilineText)]
        public string OnUnassignmentExpression { get; set; }

        [Column("Permissions")]
        public string PermissionsJson { get; set; }
        [NotMapped]
        public FlagPermission Permissions
        {
            get => FlagPermission.FromFlag(this);
            set => PermissionsJson = value?.ToJson();
        }
        [Range(0, int.MaxValue)]
        public int? DefaultRemoveDays { get; set; }

        public virtual IList<UserFlagAssignment> UserFlagAssignments { get; set; }

        public override string ToString()
            => Name;
    }
}