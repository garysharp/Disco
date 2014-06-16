using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

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

        public virtual IList<UserFlagAssignment> UserFlagAssignments { get; set; }

        public override string ToString()
        {
            return this.Name;
        }
    }
}