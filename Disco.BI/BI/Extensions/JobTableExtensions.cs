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

        public static List<JobTableModel.JobTableItemModel> DetermineItems(this JobTableModel model, DiscoDataContext dbContext, IQueryable<Job> Jobs)
        {
            List<JobTableModel.JobTableItemModel> items;

            if (model.ShowStatus)
            {

                var jobItems = Jobs.Select(j => new JobTableModel.JobTableItemModelIncludeStatus()
                {
                    Id = j.Id,
                    OpenedDate = j.OpenedDate,
                    ClosedDate = j.ClosedDate,
                    TypeId = j.JobTypeId,
                    TypeDescription = j.JobType.Description,
                    DeviceSerialNumber = j.Device.SerialNumber,
                    DeviceProfileId = j.Device.DeviceProfileId,
                    DeviceModelId = j.Device.DeviceModelId,
                    DeviceModelDescription = j.Device.DeviceModel.Description,
                    DeviceAddressId = j.Device.DeviceProfile.DefaultOrganisationAddress,
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

                items = new List<JobTableModel.JobTableItemModel>();
                foreach (var j in jobItems)
                {
                    j.StatusId = j.CalculateStatusId();
                    j.StatusDescription = JobBI.Utilities.JobStatusDescription(j.StatusId, j);

                    items.Add(j);
                }
            }
            else
            {
                items = Jobs.Select(j => new JobTableModel.JobTableItemModel()
                {
                    Id = j.Id,
                    OpenedDate = j.OpenedDate,
                    ClosedDate = j.ClosedDate,
                    TypeId = j.JobTypeId,
                    TypeDescription = j.JobType.Description,
                    DeviceSerialNumber = j.Device.SerialNumber,
                    DeviceProfileId = j.Device.DeviceProfileId,
                    DeviceModelId = j.Device.DeviceModelId,
                    DeviceModelDescription = j.Device.DeviceModel.Description,
                    DeviceAddressId = j.Device.DeviceProfile.DefaultOrganisationAddress,
                    UserId = j.UserId,
                    UserDisplayName = j.User.DisplayName,
                    OpenedTechUserId = j.OpenedTechUserId,
                    OpenedTechUserDisplayName = j.OpenedTechUser.DisplayName,
                    Location = j.DeviceHeldLocation
                }).ToList();
            }

            if (!model.ShowDeviceAddress.HasValue)
                model.ShowDeviceAddress = dbContext.DiscoConfiguration.MultiSiteMode;

            foreach (var j in items)
                if (j.DeviceAddressId.HasValue)
                    j.DeviceAddress = dbContext.DiscoConfiguration.OrganisationAddresses.GetAddress(j.DeviceAddressId.Value).Name;

            return items;
        }

        public static void Fill(this JobTableModel model, DiscoDataContext dbContext, IQueryable<Job> Jobs)
        {
            model.Items = model.DetermineItems(dbContext, Jobs);
        }
    }
}
