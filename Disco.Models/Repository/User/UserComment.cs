using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Disco.Models.Repository
{
    public class UserComment
    {
        [Key]
        public int Id { get; set; }
        public string UserId { get; set; }

        [Required]
        public string TechUserId { get; set; }
        public DateTime Timestamp { get; set; }
        [Required]
        public string Comments { get; set; }

        [ForeignKey(nameof(UserId))]
        public User User { get; set; }

        [ForeignKey(nameof(TechUserId))]
        public User TechUser { get; set; }
    }
}
