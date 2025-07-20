using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Disco.Models.Repository
{
    public class Device : IAttachmentTarget
    {
        [Required(ErrorMessage = "The Serial Number is Required"), Key, StringLength(60)]
        public string SerialNumber { get; set; }

        [StringLength(40)]
        public string AssetNumber { get; set; }
        [StringLength(250)]
        public string Location { get; set; }

        public int? DeviceModelId { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "A valid Device Profile is Required")]
        public int DeviceProfileId { get; set; }
        public int? DeviceBatchId { get; set; }

        [StringLength(50), Column("ComputerName")]
        public string DeviceDomainId { get; set; }
        public string AssignedUserId { get; set; }
        public DateTime? LastNetworkLogonDate { get; set; }

        public bool AllowUnauthenticatedEnrol { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime? EnrolledDate { get; set; }
        public DateTime? LastEnrolDate { get; set; }
        public DateTime? DecommissionedDate { get; set; }
        public DecommissionReasons? DecommissionReason { get; set; }

        [ForeignKey("DeviceModelId")]
        public virtual DeviceModel DeviceModel { get; set; }
        [ForeignKey("DeviceProfileId")]
        public virtual DeviceProfile DeviceProfile { get; set; }
        [ForeignKey("DeviceBatchId")]
        public virtual DeviceBatch DeviceBatch { get; set; }
        [ForeignKey("AssignedUserId")]
        public virtual User AssignedUser { get; set; }

        public virtual IList<DeviceUserAssignment> DeviceUserAssignments { get; set; }
        public virtual IList<DeviceDetail> DeviceDetails { get; set; }
        public virtual IList<DeviceAttachment> DeviceAttachments { get; set; }
        public virtual IList<DeviceCertificate> DeviceCertificates { get; set; }

        [InverseProperty(nameof(Job.Device))]
        public virtual IList<Job> Jobs { get; set; }
        public virtual IList<DeviceFlagAssignment> DeviceFlagAssignments { get; set; }
        
        [InverseProperty(nameof(DeviceComment.Device))]
        public virtual IList<DeviceComment> DeviceComments { get; set; }

        /// <summary>
        /// A list of the current device assignments, ordered by the most recent assignment date.
        /// </summary>
        [NotMapped]
        public IList<DeviceUserAssignment> CurrentDeviceUserAssignments
        {
            get
            {
                return DeviceUserAssignments?
                    .Where(dua => dua.UnassignedDate is null)
                    .OrderByDescending(dua => dua.AssignedDate)
                    .ToList();
            }
        }

        public override string ToString()
        {
            if (DeviceModel != null)
                return $"{DeviceModel} - {SerialNumber}";
            else
                return SerialNumber;
        }

        [NotMapped]
        public string ComputerName
        {
            get
            {
                if (DeviceDomainId == null)
                    return null;

                var index = DeviceDomainId.IndexOf('\\');
                return index < 0 ? DeviceDomainId : DeviceDomainId.Substring(index + 1);
            }
        }

        [NotMapped]
        public string ComputerDomainName
        {
            get
            {
                if (DeviceDomainId == null)
                    return null;

                var index = DeviceDomainId.IndexOf('\\');
                return index < 0 ? null : DeviceDomainId.Substring(0, index);
            }
        }

        [NotMapped]
        public string AttachmentReferenceId { get { return SerialNumber; } }

        [NotMapped]
        public AttachmentTypes HasAttachmentType { get { return AttachmentTypes.Device; } }
    }
}
