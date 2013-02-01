using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Disco.Services.Logging.Models
{
    [Table("Events")]
    public class LogEvent
    {
        [Required, Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        public int ModuleId { get; set; }
        [Required]
        public int EventTypeId { get; set; }
        [Required]
        public DateTime Timestamp { get; set; }
        public string Arguments { get; set; }
    }
}
