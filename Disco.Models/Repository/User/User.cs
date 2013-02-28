using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Disco.Models.Repository
{
    public class User
    {
        [StringLength(50), Key]
        public string Id { get; set; }

        [StringLength(200)]
        public string DisplayName { get; set; }
        [StringLength(200)]
        public string Surname { get; set; }
        [StringLength(200)]
        public string GivenName { get; set; }

        [StringLength(8)]
        public string Type { get; set; }
        [StringLength(100)]
        public string PhoneNumber { get; set; }
        [StringLength(150)]
        public string EmailAddress { get; set; }

        public virtual IList<UserDetail> UserDetails { get; set; }
        public virtual IList<UserAttachment> UserAttachments { get; set; }
        public virtual IList<DeviceUserAssignment> DeviceUserAssignments { get; set; }
        [InverseProperty("UserId")]
        public virtual IList<Job> Jobs { get; set; }

        //#region Helper Members
        //[NotMapped, XmlIgnore, ScriptIgnore]
        //public List<DeviceUserAssignment> CurrentDeviceUserAssignments
        //{
        //    get
        //    {
        //        return this.DeviceUserAssignments.Where(dua => !dua.UnassignedDate.HasValue).ToList();
        //    }
        //}
        //#endregion

        public override string ToString()
        {
            return string.Format("{0} ({1})", this.DisplayName, this.Id);
        }

        public void UpdateSelf(User u)
        {
            if (!this.Id.Equals(u.Id, StringComparison.InvariantCultureIgnoreCase))
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
            if (this.Type != u.Type)
                this.Type = u.Type;
        }

        public static class Types
        {
            public const string Admin = "Admin";
            public const string Computer = "Computer";
            public const string Staff = "Staff";
            public const string Student = "Student";
        }

    }
}
