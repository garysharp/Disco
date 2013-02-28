using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Disco.Services.Logging.Models
{
    [Table("EventTypes")]
    public class LogEventType
    {
        [Required, Key, Column(Order=0), DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ModuleId { get; set; }
        [Required, Key, Column(Order = 1), DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        [Required, MaxLength(200)]
        public string Name { get; set; }
        [Required]
        public int Severity { get; set; }
        [MaxLength(1024)]
        public string Format { get; set; }
        
        [NotMapped]
        public bool UsePersist { get; set; }
        [NotMapped]
        public bool UseLive { get; set; }
        [NotMapped]
        public bool UseDisplay { get; set; }

        [ForeignKey("ModuleId")]
        public LogModule Module { get; set; }

        public enum Severities
        {
            Information = 0,
            Warning = 1,
            Error = 2
        }

        public string FormatMessage(object[] Arguments)
        {

            if (Arguments != null && Arguments.Length > 0)
            {
                if (!string.IsNullOrEmpty(Format))
                {
                    return string.Format(Format, Arguments);
                }
                else
                {
                    return Arguments
                        .Select(v => v == null ? string.Empty : v.ToString())
                        .Aggregate((a, b) => a + ", " + (b == null ? string.Empty : b));
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(Format))
                {
                    return Format;
                }
            }
            return string.Empty;
        }
    }
}
