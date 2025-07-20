using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Disco.Models.Repository
{
    public class UserDetail
    {
        [Key, Column(Order = 0)]
        public string UserId { get; set; }

        [Column(Order = 1), Key, StringLength(100)]
        public string Scope { get; set; }

        [Key, Column(Order = 2), StringLength(100)]
        public string Key { get; set; }

        public string Value { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }
}
