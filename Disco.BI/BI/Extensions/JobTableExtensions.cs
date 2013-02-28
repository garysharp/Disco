using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Disco.Models.BI.Job;
using Disco.Models.Repository;
using Disco.Data.Repository;

namespace Disco.BI.Extensions
{
    public static class JobTableExtensions
    {

        public static void Fill(this JobTableModel model, DiscoDataContext dbContext, IQueryable<Job> Jobs)
        {
            if (model.ShowStatus)
            {

                var jobItems = Jobs.Select(j => new JobTableModel.JobTableItemModelIncludeStatus()
                {
                    Id = j.Id,
                    DeviceAddressId = j.Device.DeviceProfile.DefaultOrganisationAddress,
                    OpenedDate = j.OpenedDate,
                    ClosedDate = j.ClosedDate,
                    TypeId = j.JobTypeId,
                    TypeDescription = j.JobType.Description,
                    DeviceSerialNumber = j.Device.SerialNumber,
                    DeviceModelDescription = j.Device.DeviceModel.Description,
                    UserId = j.UserId,
                    UserDisplayName = j.User.DisplayName,
                    OpenedTechUserId = j.OpenedTechUserId,
                    OpenedTechUserDisplayName = j.OpenedTechUser.DisplayName,
                    Location = j.DeviceHeldLocation,

                    JobMetaWarranty_ExternalReference = j.JobMetaWarranty.ExternalReference,
                    JobMetaWarranty_ExternalCompletedDate = j.JobMetaWarranty.ExternalCompletedDate,
                    JobMetaNonWarranty_RepairerLoggedDate = j.JobMetaNonWarranty.RepairerLoggedDate,
                    JobMetaNonWarranty_RepairerCompletedDate = j.JobMetaNonWarranty.RepairerCompletedDate,
                    JobMetaNonWarranty_AccountingChargeAddedDate = j.JobMetaNonWarranty.AccountingChargeAddedDate,
                    JobMetaNonWarranty_AccountingChargePaidDate = j.JobMetaNonWarranty.AccountingChargePaidDate,
                    JobMetaNonWarranty_AccountingChargeRequiredDate = j.JobMetaNonWarranty.AccountingChargeRequiredDate,
                    JobMetaNonWarranty_IsInsuranceClaim = j.JobMetaNonWarranty.IsInsuranceClaim,
                    JobMetaInsurance_ClaimFormSentDate = j.JobMetaInsurance.ClaimFormSentDate,

                    WaitingForUserAction = j.WaitingForUserAction,
                    DeviceReadyForReturn = j.DeviceReadyForReturn,
                    DeviceHeld = j.DeviceHeld,
                    DeviceReturnedDate = j.DeviceReturnedDate,
                    JobMetaWarranty_ExternalName = j.JobMetaWarranty.ExternalName,
                    JobMetaNonWarranty_RepairerName = j.JobMetaNonWarranty.RepairerName
                });

                model.Items = new List<JobTableModel.JobTableItemModel>();
                foreach (var j in jobItems)
                {
                    j.StatusId = j.CalculateStatusId();
                    j.StatusDescription = JobBI.Utilities.JobStatusDescription(j.StatusId, j);

                    model.Items.Add(j);
                }
            }
            else
            {
                model.Items = Jobs.Select(j => new JobTableModel.JobTableItemModel()
                {
                    Id = j.Id,
                    DeviceAddressId = j.Device.DeviceProfile.DefaultOrganisationAddress,
                    OpenedDate = j.OpenedDate,
                    ClosedDate = j.ClosedDate,
                    TypeId = j.JobTypeId,
                    TypeDescription = j.JobType.Description,
                    DeviceSerialNumber = j.Device.SerialNumber,
                    DeviceModelDescription = j.Device.DeviceModel.Description,
                    UserId = j.UserId,
                    UserDisplayName = j.User.DisplayName,
                    OpenedTechUserId = j.OpenedTechUserId,
                    OpenedTechUserDisplayName = j.OpenedTechUser.DisplayName,
                    Location = j.DeviceHeldLocation
                }).ToList();
            }

            if (!model.ShowDeviceAddress.HasValue)
                model.ShowDeviceAddress = dbContext.DiscoConfiguration.MultiSiteMode;

            if (model.ShowDeviceAddress.Value)
            {
                foreach (var j in model.Items)
                    if (j.DeviceAddressId.HasValue)
                        j.DeviceAddress = dbContext.DiscoConfiguration.OrganisationAddresses.GetAddress(j.DeviceAddressId.Value).Name;
            }
        }

    }
}
