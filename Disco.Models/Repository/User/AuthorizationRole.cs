﻿using System.ComponentModel.DataAnnotations;

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
