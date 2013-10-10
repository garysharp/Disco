using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Models.Repository
{
    public class AuthorizationRole
    {
        [Key]
        public int Id { get; set; }
        [Required, StringLength(100)]
        public string Name { get; set; }

        public string SubjectIds { get; set; }

        public string ClaimsJson { get; set; }
    }
}
