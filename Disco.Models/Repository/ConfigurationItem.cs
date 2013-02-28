using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Disco.Models.Repository
{
    [Table("Configuration")]
    public class ConfigurationItem
    {
        [StringLength(80), Column(Order = 1), Key]
        public string Key { get; set; }
        [Column(Order = 0), StringLength(80), Key]
        public string Scope { get; set; }
        public string Value { get; set; }
    }
}
