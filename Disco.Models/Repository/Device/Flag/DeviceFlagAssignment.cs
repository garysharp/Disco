using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Disco.Models.Repository
{
    public class DeviceFlagAssignment
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int DeviceFlagId { get; set; }
        [Required]
        public string DeviceSerialNumber { get; set; }

        [Required]
        public DateTime AddedDate { get; set; }
        [Required]
        public string AddedUserId { get; set; }
        public DateTime? RemovedDate { get; set; }
        public string RemovedUserId { get; set; }

        public string Comments { get; set; }

        public string OnAssignmentExpressionResult { get; set; }
        public string OnUnassignmentExpressionResult { get; set; }

        [ForeignKey(nameof(DeviceFlagId)), InverseProperty("DeviceFlagAssignments")]
        public virtual DeviceFlag DeviceFlag { get; set; }

        [ForeignKey(nameof(DeviceSerialNumber)), InverseProperty("DeviceFlagAssignments")]
        public virtual Device Device { get; set; }

        [ForeignKey("AddedUserId")]
        public virtual User AddedUser { get; set; }
        [ForeignKey("RemovedUserId")]
        public virtual User RemovedUser { get; set; }

        public override string ToString()
        {
            return $"Device Flag Id: {DeviceFlagId}; Device Serial Number: {DeviceSerialNumber}; Added: {AddedDate:s}";
        }
    }
}
