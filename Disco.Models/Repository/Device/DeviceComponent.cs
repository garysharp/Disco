using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Disco.Models.Repository
{
    public class DeviceComponent
    {
        [Key]
        public int Id { get; set; }
        public int? DeviceModelId { get; set; }
        [StringLength(100)]
        public string Description { get; set; }
        public decimal Cost { get; set; }

        [ForeignKey("DeviceModelId")]
        public virtual DeviceModel DeviceModel { get; set; }

        public virtual IList<JobSubType> JobSubTypes { get; set; }
    }
}
