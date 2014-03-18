using Disco.Models.Repository;
using Disco.Models.Services.Jobs.JobLists;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Services
{
    public static class JobExtensions
    {
        public static JobTableStatusItemModel ToJobTableStatusItemModel(this Job j)
        {
            var i = new JobTableStatusItemModel()
            {
                JobId = j.Id,
                OpenedDate = j.OpenedDate,
                ClosedDate = j.ClosedDate,
                JobTypeId = j.JobTypeId,
                JobTypeDescription = j.JobType.Description,
                DeviceHeldLocation = j.DeviceHeldLocation,
                Flags = j.Flags,

                WaitingForUserAction = j.WaitingForUserAction,
                DeviceReadyForReturn = j.DeviceReadyForReturn,
                DeviceHeld = j.DeviceHeld,
                DeviceReturnedDate = j.DeviceReturnedDate
            };

            if (j.Device != null)
            {
                i.DeviceSerialNumber = j.DeviceSerialNumber;
                i.DeviceModelDescription = j.Device.DeviceModel.Description;
                i.DeviceAddressId = j.Device.DeviceProfile.DefaultOrganisationAddress;

                if (j.JobMetaWarranty != null)
                {
                    i.JobMetaWarranty_ExternalReference = j.JobMetaWarranty.ExternalReference;
                    i.JobMetaWarranty_ExternalLoggedDate = j.JobMetaWarranty.ExternalLoggedDate;
                    i.JobMetaWarranty_ExternalCompletedDate = j.JobMetaWarranty.ExternalCompletedDate;
                    i.JobMetaWarranty_ExternalName = j.JobMetaWarranty.ExternalName;
                }
                if (j.JobMetaNonWarranty != null)
                {
                    i.JobMetaNonWarranty_RepairerLoggedDate = j.JobMetaNonWarranty.RepairerLoggedDate;
                    i.JobMetaNonWarranty_RepairerCompletedDate = j.JobMetaNonWarranty.RepairerCompletedDate;
                    i.JobMetaNonWarranty_AccountingChargeAddedDate = j.JobMetaNonWarranty.AccountingChargeAddedDate;
                    i.JobMetaNonWarranty_AccountingChargePaidDate = j.JobMetaNonWarranty.AccountingChargePaidDate;
                    i.JobMetaNonWarranty_AccountingChargeRequiredDate = j.JobMetaNonWarranty.AccountingChargeRequiredDate;
                    i.JobMetaNonWarranty_IsInsuranceClaim = j.JobMetaNonWarranty.IsInsuranceClaim;
                    i.JobMetaNonWarranty_RepairerName = j.JobMetaNonWarranty.RepairerName;
                    if (j.JobMetaInsurance != null)
                    {
                        i.JobMetaInsurance_ClaimFormSentDate = j.JobMetaInsurance.ClaimFormSentDate;
                    }
                }

            }
            if (j.User != null)
            {
                i.UserId = j.UserId;
                i.UserDisplayName = j.User.DisplayName;
            }
            if (j.OpenedTechUser != null)
            {
                i.OpenedTechUserId = j.OpenedTechUserId;
                i.OpenedTechUserDisplayName = j.OpenedTechUser.DisplayName;
            }

            return i;
        }

        public static string JobStatusDescription(string StatusId, Job j = null)
        {
            switch (StatusId)
            {
                case Job.JobStatusIds.Open:
                    return "Open";
                case Job.JobStatusIds.Closed:
                    return "Closed";
                case Job.JobStatusIds.AwaitingWarrantyRepair:
                    if (j == null)
                        return "Awaiting Warranty Repair";
                    else
                        if (j.DeviceHeld.HasValue)
                            return string.Format("Awaiting Warranty Repair ({0})", j.JobMetaWarranty.ExternalName);
                        else
                            return string.Format("Awaiting Warranty Repair - Not Held ({0})", j.JobMetaWarranty.ExternalName);
                case Job.JobStatusIds.AwaitingRepairs:
                    if (j == null)
                        return "Awaiting Repairs";
                    else
                        if (j.DeviceHeld.HasValue)
                            return string.Format("Awaiting Repairs ({0})", j.JobMetaNonWarranty.RepairerName);
                        else
                            return string.Format("Awaiting Repairs - Not Held ({0})", j.JobMetaNonWarranty.RepairerName);
                case Job.JobStatusIds.AwaitingDeviceReturn:
                    return "Awaiting Device Return";
                case Job.JobStatusIds.AwaitingUserAction:
                    return "Awaiting User Action";
                case Job.JobStatusIds.AwaitingAccountingPayment:
                    return "Awaiting Accounting Payment";
                case Job.JobStatusIds.AwaitingAccountingCharge:
                    return "Awaiting Accounting Charge";
                case Job.JobStatusIds.AwaitingInsuranceProcessing:
                    return "Awaiting Insurance Processing";
                default:
                    return "Unknown";
            }
        }

        public static string JobStatusDescription(string StatusId, JobTableStatusItemModel j = null)
        {
            switch (StatusId)
            {
                case Job.JobStatusIds.Open:
                    return "Open";
                case Job.JobStatusIds.Closed:
                    return "Closed";
                case Job.JobStatusIds.AwaitingWarrantyRepair:
                    if (j == null)
                        return "Awaiting Warranty Repair";
                    else
                        if (j.DeviceHeld.HasValue)
                            return string.Format("Awaiting Warranty Repair ({0})", j.JobMetaWarranty_ExternalName);
                        else
                            return string.Format("Awaiting Warranty Repair - Not Held ({0})", j.JobMetaWarranty_ExternalName);
                case Job.JobStatusIds.AwaitingRepairs:
                    if (j == null)
                        return "Awaiting Repairs";
                    else
                        if (j.DeviceHeld.HasValue)
                            return string.Format("Awaiting Repairs ({0})", j.JobMetaNonWarranty_RepairerName);
                        else
                            return string.Format("Awaiting Repairs - Not Held ({0})", j.JobMetaNonWarranty_RepairerName);
                case Job.JobStatusIds.AwaitingDeviceReturn:
                    return "Awaiting Device Return";
                case Job.JobStatusIds.AwaitingUserAction:
                    return "Awaiting User Action";
                case Job.JobStatusIds.AwaitingAccountingPayment:
                    return "Awaiting Accounting Payment";
                case Job.JobStatusIds.AwaitingAccountingCharge:
                    return "Awaiting Accounting Charge";
                case Job.JobStatusIds.AwaitingInsuranceProcessing:
                    return "Awaiting Insurance Processing";
                default:
                    return "Unknown";
            }
        }

        public static string CalculateStatusId(this Job j)
        {
            return j.ToJobTableStatusItemModel().CalculateStatusId();
        }

        public static string CalculateStatusId(this JobTableStatusItemModel j)
        {
            if (j.ClosedDate.HasValue)
                return Job.JobStatusIds.Closed;

            if (j.JobTypeId == JobType.JobTypeIds.HWar)
            {
                if (j.JobMetaWarranty_ExternalLoggedDate.HasValue && !j.JobMetaWarranty_ExternalCompletedDate.HasValue)
                    return Job.JobStatusIds.AwaitingWarrantyRepair; // Job Logged - but not marked as completed
            }

            if (j.JobTypeId == JobType.JobTypeIds.HNWar)
            {
                if (j.JobMetaNonWarranty_RepairerLoggedDate.HasValue && !j.JobMetaNonWarranty_RepairerCompletedDate.HasValue)
                    return Job.JobStatusIds.AwaitingRepairs; // Repairs logged - but not complete
                if (j.JobMetaNonWarranty_AccountingChargeAddedDate.HasValue && !j.JobMetaNonWarranty_AccountingChargePaidDate.HasValue)
                    return Job.JobStatusIds.AwaitingAccountingPayment; // Accounting Charge Added, but not paid
                if (j.JobMetaNonWarranty_AccountingChargeRequiredDate.HasValue && (!j.JobMetaNonWarranty_AccountingChargePaidDate.HasValue || !j.JobMetaNonWarranty_AccountingChargeAddedDate.HasValue))
                    return Job.JobStatusIds.AwaitingAccountingCharge; // Accounting Charge Required, but not added or paid
                if (j.JobMetaNonWarranty_RepairerLoggedDate.HasValue && j.JobMetaNonWarranty_IsInsuranceClaim.Value && !j.JobMetaInsurance_ClaimFormSentDate.HasValue)
                    return Job.JobStatusIds.AwaitingInsuranceProcessing; // Is insurance claim, but no Claim Form Sent
            }

            if (j.WaitingForUserAction.HasValue)
                return Job.JobStatusIds.AwaitingUserAction; // Awaiting for User

            if (j.DeviceReadyForReturn.HasValue && !j.DeviceReturnedDate.HasValue)
                return Job.JobStatusIds.AwaitingDeviceReturn; // Device not returned to User

            return Job.JobStatusIds.Open;
        }

        public static Tuple<string, string> Status(this Job j)
        {
            var statusId = j.CalculateStatusId();
            return new Tuple<string, string>(statusId, JobStatusDescription(statusId, j));
        }
    }
}
