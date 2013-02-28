using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Disco.Models.Repository
{
    public class JobLog
    {
        [Key]
        public int Id { get; set; }
        public int JobId { get; set; }

        [Required]
        public string TechUserId { get; set; }
        public DateTime Timestamp { get; set; }
        [Required]
        public string Comments { get; set; }
        
        [ForeignKey("JobId")]
        public Job Job { get; set; }
        
        [ForeignKey("TechUserId")]
        public User TechUser { get; set; }
    }
}
