using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace Disco.Logging.Models
{
    [Table("Modules")]
    public class LogModule
    {
        [Required, Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        [Required, MaxLength(200)]
        public string Name { get; set; }
        [Required, MaxLength(500)]
        public string Description { get; set; }

        public virtual IList<LogEventType> EventTypes { get; set; }
    }
}
