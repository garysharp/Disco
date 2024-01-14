using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Disco.Models.Repository
{
    public class DeviceFlag
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

        public string DevicesLinkedGroup { get; set; }
        public string DeviceUsersLinkedGroup { get; set; }

        [DataType(DataType.MultilineText)]
        public string OnAssignmentExpression { get; set; }
        [DataType(DataType.MultilineText)]
        public string OnUnassignmentExpression { get; set; }

        public virtual IList<DeviceFlagAssignment> DeviceFlagAssignments { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
