﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Disco.Models.Repository
{
    public class Job : IAttachmentTarget
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string JobTypeId { get; set; }

        public string DeviceSerialNumber { get; set; }
        public string UserId { get; set; }

        [Required]
        public string OpenedTechUserId { get; set; }

        public DateTime OpenedDate { get; set; }
        public DateTime? ExpectedClosedDate { get; set; }
        public string ClosedTechUserId { get; set; }
        public DateTime? ClosedDate { get; set; }

        public UserManagementFlags? Flags { get; set; }

        [Display(Name = "Technician Held Device")]
        public DateTime? DeviceHeld { get; set; }
        public string DeviceHeldTechUserId { get; set; }
        [StringLength(100)]
        public string DeviceHeldLocation { get; set; }

        public DateTime? DeviceReadyForReturn { get; set; }
        public string DeviceReadyForReturnTechUserId { get; set; }
        public DateTime? DeviceReturnedDate { get; set; }
        public string DeviceReturnedTechUserId { get; set; }

        public DateTime? WaitingForUserAction { get; set; }

        [ForeignKey("JobTypeId")]
        public virtual JobType JobType { get; set; }
        public virtual IList<JobSubType> JobSubTypes { get; set; }

        [ForeignKey("DeviceSerialNumber")]
        public virtual Device Device { get; set; }
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [ForeignKey("OpenedTechUserId")]
        public virtual User OpenedTechUser { get; set; }
        [ForeignKey("ClosedTechUserId")]
        public virtual User ClosedTechUser { get; set; }

        [ForeignKey("DeviceHeldTechUserId")]
        public virtual User DeviceHeldTechUser { get; set; }
        [ForeignKey("DeviceReadyForReturnTechUserId")]
        public virtual User DeviceReadyForReturnTechUser { get; set; }
        [ForeignKey("DeviceReturnedTechUserId")]
        public virtual User DeviceReturnedTechUser { get; set; }

        //// Added 2012-10-23 G# - DBv5 Migration
        //public virtual IList<JobAssignment> JobAssignments { get; set; }
        //// End Added 2012-10-23 G# - DBv5 Migration

        public virtual IList<JobAttachment> JobAttachments { get; set; }
        public virtual IList<JobComponent> JobComponents { get; set; }
        public virtual IList<JobLog> JobLogs { get; set; }

        public virtual IList<JobQueueJob> JobQueues { get; set; }

        public virtual JobMetaInsurance JobMetaInsurance { get; set; }
        public virtual JobMetaWarranty JobMetaWarranty { get; set; }
        public virtual JobMetaNonWarranty JobMetaNonWarranty { get; set; }

        [NotMapped]
        public string AttachmentReferenceId { get { return Id.ToString(); } }

        [NotMapped]
        public AttachmentTypes HasAttachmentType { get { return AttachmentTypes.Job; } }

        #region Helper Members
        public decimal JobComponentsTotalCost()
        {
            if (this.JobComponents != null)
            {
                return this.JobComponents.Sum(jc => jc.Cost);
            }
            return decimal.Zero;
        }
        #endregion

        public override string ToString()
        {
            return $"Job #{Id}";
        }

        public static class JobStatusIds
        {
            public const string AwaitingAccountingPayment = nameof(AwaitingAccountingPayment);
            public const string AwaitingAccountingCharge = nameof(AwaitingAccountingCharge);
            public const string AwaitingDeviceReturn = nameof(AwaitingDeviceReturn);
            public const string AwaitingInsuranceProcessing = nameof(AwaitingInsuranceProcessing);
            public const string AwaitingRepairs = nameof(AwaitingRepairs);
            public const string AwaitingUserAction = nameof(AwaitingUserAction);
            public const string AwaitingWarrantyRepair = nameof(AwaitingWarrantyRepair);
            public const string Closed = nameof(Closed);
            public const string Open = nameof(Open);

            public static readonly Dictionary<string, string> StatusDescriptions = new Dictionary<string, string>
            {
                { AwaitingAccountingPayment, "Awaiting Accounting Payment" },
                { AwaitingAccountingCharge, "Awaiting Accounting Charge" },
                { AwaitingDeviceReturn, "Awaiting Device Return" },
                { AwaitingInsuranceProcessing, "Awaiting Insurance Processing" },
                { AwaitingRepairs, "Awaiting Repairs" },
                { AwaitingUserAction, "Awaiting User Action" },
                { AwaitingWarrantyRepair, "Awaiting Warranty Repair" },
                { Closed, "Closed" },
                { Open, "Open" }
            };
        }

        [Flags]
        public enum UserManagementFlags : long
        {
            [Display(GroupName = JobSubType.UserManagementJobSubTypes.Infringement, Name = "Content - Games")]
            Infringement_ContentGames = 1,
            [Display(GroupName = JobSubType.UserManagementJobSubTypes.Infringement, Name = "Content - Illegal")]
            Infringement_ContentIllegal = 2,
            [Display(GroupName = JobSubType.UserManagementJobSubTypes.Infringement, Name = "Content - Violence")]
            Infringement_ContentViolence = 4,
            [Display(GroupName = JobSubType.UserManagementJobSubTypes.Infringement, Name = "Content - Pornography")]
            Infringement_ContentPornography = 8,
            [Display(GroupName = JobSubType.UserManagementJobSubTypes.Infringement, Name = "Hacking")]
            Infringement_Hacking = 16,
            [Display(GroupName = JobSubType.UserManagementJobSubTypes.Infringement, Name = "Proxy Bypass")]
            Infringement_ProxyBypass = 32,
            [Display(GroupName = JobSubType.UserManagementJobSubTypes.Infringement, Name = "Breach Usage Agreement")]
            Infringement_BreachUsageAgreement = 64,
            [Display(GroupName = JobSubType.UserManagementJobSubTypes.Infringement, Name = "Breach Financial Agreement")]
            Infringement_BreachFinancialAgreement = 128,
            [Display(GroupName = JobSubType.UserManagementJobSubTypes.Contact, Name = "Phone")]
            Contact_Phone = 4294967296,
            [Display(GroupName = JobSubType.UserManagementJobSubTypes.Contact, Name = "Email")]
            Contact_Email = 8589934592,
            [Display(GroupName = JobSubType.UserManagementJobSubTypes.Contact, Name = "In Person")]
            Contact_InPerson = 17179869184,
            [Display(GroupName = JobSubType.UserManagementJobSubTypes.Contact, Name = "SMS")]
            Contact_SMS = 34359738368,
            [Display(GroupName = JobSubType.UserManagementJobSubTypes.Contact, Name = "Mail")]
            Contact_Mail = 68719476736,
        }
    }
}
