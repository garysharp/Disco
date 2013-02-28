using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Disco.Models.Repository
{
    public class DeviceCertificate
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(64)]
        public string ProviderId { get; set; }
        public int ProviderIndex { get; set; }

        [StringLength(28)]
        public string Name { get; set; }
        [MaxLength(16384)]
        public byte[] Content { get; set; }
        public bool Enabled { get; set; }

        // Added 2011-10-24 G#
        public DateTime? ExpirationDate { get; set; }
        // Added 2011-10-24 G#
        public DateTime? AllocatedDate { get; set; }

        public string DeviceSerialNumber { get; set; }

        [ForeignKey("DeviceSerialNumber")]
        public virtual Device Device { get; set; }
    }
}
