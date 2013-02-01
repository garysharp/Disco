using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Disco.Models.Repository
{
    public class JobComponent
    {
        [Key]
        public int Id { get; set; }
        public int JobId { get; set; }

        [Required]
        public string TechUserId { get; set; }
        [StringLength(500)]
        public string Description { get; set; }
        public decimal Cost { get; set; }
        
        [ForeignKey("JobId")]
        public virtual Job Job { get; set; }
        
        [ForeignKey("TechUserId")]
        public virtual User TechUser { get; set; }
    }
}
