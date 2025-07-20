using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Disco.Models.Repository
{
    public class UserFlagAssignment
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int UserFlagId { get; set; }
        [Required]
        public string UserId { get; set; }

        [Required]
        public DateTime AddedDate { get; set; }
        [Required]
        public string AddedUserId { get; set; }
        public DateTime? RemovedDate { get; set; }
        public string RemovedUserId { get; set; }

        public string Comments { get; set; }

        public string OnAssignmentExpressionResult { get; set; }
        public string OnUnassignmentExpressionResult { get; set; }

        [ForeignKey("UserFlagId"), InverseProperty("UserFlagAssignments")]
        public virtual UserFlag UserFlag { get; set; }

        [ForeignKey("UserId"), InverseProperty("UserFlagAssignments")]
        public virtual User User { get; set; }

        [ForeignKey("AddedUserId")]
        public virtual User AddedUser { get; set; }
        [ForeignKey("RemovedUserId")]
        public virtual User RemovedUser { get; set; }

        public override string ToString()
        {
            return $"User Flag Id: {UserFlagId}; User Id: {UserId}; Added: {AddedDate:s}";
        }
    }
}