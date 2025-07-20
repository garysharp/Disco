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
        public DateTime? RemoveDate { get; set; }
        public string RemoveUserId { get; set; }

        public string Comments { get; set; }

        public string OnAssignmentExpressionResult { get; set; }
        public string OnUnassignmentExpressionResult { get; set; }

        [ForeignKey(nameof(UserFlagId)), InverseProperty(nameof(Repository.UserFlag.UserFlagAssignments))]
        public virtual UserFlag UserFlag { get; set; }

        [ForeignKey(nameof(UserId)), InverseProperty(nameof(Repository.User.UserFlagAssignments))]
        public virtual User User { get; set; }

        [ForeignKey(nameof(AddedUserId))]
        public virtual User AddedUser { get; set; }
        [ForeignKey(nameof(RemovedUserId))]
        public virtual User RemovedUser { get; set; }
        [ForeignKey(nameof(RemoveUserId))]
        public virtual User RemoveUser { get; set; }

        public override string ToString()
            => $"User Flag Id: {UserFlagId}; User Id: {UserId}; Added: {AddedDate:s}";
    }
}