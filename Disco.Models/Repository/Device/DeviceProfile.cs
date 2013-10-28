using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;
using System.Linq.Expressions;

namespace Disco.Models.Repository
{
    public partial class DeviceProfile
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; }

        [Required, StringLength(10)]
        public string ShortName { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        public int? DefaultOrganisationAddress { get; set; }

        // Migration from DeviceProfile Configuration
        // 2012-06-14 G#
        [Required]
        public string ComputerNameTemplate { get; set; }
        
        [Required]
        public DistributionTypes? DistributionType { get; set; }

        public string OrganisationalUnit { get; set; }
        // End Migration

        // 2012-06-14 G#
        public bool EnforceComputerNameConvention { get; set; }
        public bool EnforceOrganisationalUnit { get; set; }

        // 2012-06-28 G#
        public bool ProvisionADAccount { get; set; }

        public bool AssignedUserLocalAdmin { get; set; }

        public virtual IList<Device> Devices { get; set; }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(this.ShortName))
            {
                return this.Name;
            }
            return string.Format("{0} ({1})", this.Name, this.ShortName);
        }

        // 2012-06-21
        // public bool AllocateCertificate { get; set; } // Renamed from 'AllocateWirelessCertificate'
        [StringLength(64)]
        public string CertificateProviderId { get; set; }

        public enum DistributionTypes : int
        {
            OneToMany = 0,
            OneToOne = 1
        }
    }
}
