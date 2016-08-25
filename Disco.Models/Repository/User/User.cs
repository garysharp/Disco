using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Disco.Models.Repository
{
    public class User : IAttachmentTarget
    {
        [StringLength(50), Key, Column("Id")]
        public string UserId { get; set; }

        [StringLength(200)]
        public string DisplayName { get; set; }
        [StringLength(200)]
        public string Surname { get; set; }
        [StringLength(200)]
        public string GivenName { get; set; }

        [StringLength(100)]
        public string PhoneNumber { get; set; }
        [StringLength(150)]
        public string EmailAddress { get; set; }

        public virtual IList<UserDetail> UserDetails { get; set; }
        public virtual IList<UserAttachment> UserAttachments { get; set; }
        public virtual IList<DeviceUserAssignment> DeviceUserAssignments { get; set; }
        [InverseProperty("UserId")]
        public virtual IList<Job> Jobs { get; set; }
        public virtual IList<UserFlagAssignment> UserFlagAssignments { get; set; }

        [NotMapped, Obsolete("Should be using Combined Domain\\User format - UserId")]
        public string Id
        {
            get
            {
                return DomainUsername;
            }
        }
        
        [NotMapped]
        public string DomainUsername
        {
            get
            {
                var index = UserId.IndexOf('\\');
                return index < 0 ? UserId : UserId.Substring(index + 1);
            }
        }

        [NotMapped]
        public string Domain
        {
            get
            {
                var index = UserId.IndexOf('\\');
                return index < 0 ? null : UserId.Substring(0, index);
            }
        }

        [NotMapped]
        public string AttachmentReferenceId { get { return UserId; } }

        [NotMapped]
        public AttachmentTypes HasAttachmentType { get { return AttachmentTypes.User; } }

        public override string ToString()
        {
            return string.Format("{0} ({1})", this.DisplayName, this.UserId);
        }

        public void UpdateSelf(User u)
        {
            if (!this.UserId.Equals(u.UserId, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("User Id's do not match", "u");

            if (this.Surname != u.Surname)
                this.Surname = u.Surname;
            if (this.GivenName != u.GivenName)
                this.GivenName = u.GivenName;
            if (this.DisplayName != u.DisplayName)
                this.DisplayName = u.DisplayName;
            if (this.EmailAddress != u.EmailAddress)
                this.EmailAddress = u.EmailAddress;
            if (this.PhoneNumber != u.PhoneNumber)
                this.PhoneNumber = u.PhoneNumber;
        }
    }
}
