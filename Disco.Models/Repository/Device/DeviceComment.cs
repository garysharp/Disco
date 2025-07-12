using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Disco.Models.Repository
{
    public class DeviceComment
    {
        [Key]
        public int Id { get; set; }
        public string DeviceSerialNumber { get; set; }

        [Required]
        public string TechUserId { get; set; }
        public DateTime Timestamp { get; set; }
        [Required]
        public string Comments { get; set; }
        
        [ForeignKey(nameof(DeviceSerialNumber))]
        public Device Device { get; set; }
        
        [ForeignKey(nameof(TechUserId))]
        public User TechUser { get; set; }
    }
}
