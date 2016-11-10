using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Disco.Models.Repository
{
    public class DeviceUserAssignment
    {
        [Key, Column(Order = 0)]
        public string DeviceSerialNumber { get; set; }
        public string AssignedUserId { get; set; }

        [Column(Order = 1), Key]
        public DateTime AssignedDate { get; set; }
        public DateTime? UnassignedDate { get; set; }

        [ForeignKey("AssignedUserId")]
        public virtual User AssignedUser { get; set; }

        [ForeignKey("DeviceSerialNumber")]
        public virtual Device Device { get; set; }
    }
}
