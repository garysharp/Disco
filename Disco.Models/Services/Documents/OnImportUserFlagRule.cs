using Disco.Models.Repository;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;

namespace Disco.Models.Services.Documents
{
    public class OnImportUserFlagRule
    {
        public Guid Id { get; set; }
        public int FlagId { get; set; }
        [StringLength(50)]
        public string UserId { get; set; }
        public bool AddFlag { get; set; }
        public string Comments { get; set; }

        [JsonIgnore]
        public UserFlag UserFlag { get; set; }
    }
}
