using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

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

        [StringLength(500), DataType(DataType.MultilineText)]
        public string Description { get; set; }

        public int? DefaultOrganisationAddress { get; set; }

        // Migration from DeviceProfile Configuration
        // 2012-06-14 G#
        [Required, DataType(DataType.MultilineText)]
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

        public bool AllowUntrustedReimageJobEnrolment { get; set; }

        public string DevicesLinkedGroup { get; set; }
        public string AssignedUsersLinkedGroup { get; set; }

        public virtual IList<Device> Devices { get; set; }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(ShortName))
            {
                return Name;
            }
            return string.Format("{0} ({1})", Name, ShortName);
        }

        [StringLength(200)]
        public string CertificateProviders { get; set; }

        [StringLength(200)]
        public string CertificateAuthorityProviders { get; set; }

        [StringLength(200)]
        public string WirelessProfileProviders { get; set; }

        public const string DefaultComputerNameTemplate = "DeviceProfile.ShortName + '-' + SerialNumber";

        public enum DistributionTypes : int
        {
            OneToMany = 0,
            OneToOne = 1
        }
    }
}
