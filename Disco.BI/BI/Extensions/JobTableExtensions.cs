using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Disco.Models.BI.Job;
using Disco.Models.Repository;
using Disco.Data.Repository;
using Disco.Services.Users;
using Disco.Services.Authorization;

namespace Disco.BI.Extensions
{
    public static class JobTableExtensions
    {
        private static List<string> FilterAllowedTypes(AuthorizationToken Authorization)
        {
            if (!Authorization.HasAll(Claims.Job.Types.ShowHMisc, Claims.Job.Types.ShowHNWar, Claims.Job.Types.ShowHWar, Claims.Job.Types.ShowSApp, Claims.Job.Types.ShowSImg, Claims.Job.Types.ShowSOS, Claims.Job.Types.ShowUMgmt))
            {
                // Must Filter
                List<string> allowedTypes = new List<string>(6);
                if (Authorization.Has(Claims.Job.Types.ShowHMisc))
                    allowedTypes.Add(JobType.JobTypeIds.HMisc);
                if (Authorization.Has(Claims.Job.Types.ShowHNWar))
                    allowedTypes.Add(JobType.JobTypeIds.HNWar);
                if (Authorization.Has(Claims.Job.Types.ShowHWar))
                    allowedTypes.Add(JobType.JobTypeIds.HWar);
                if (Authorization.Has(Claims.Job.Types.ShowSApp))
                    allowedTypes.Add(JobType.JobTypeIds.SApp);
                if (Authorization.Has(Claims.Job.Types.ShowSImg))
                    allowedTypes.Add(JobType.JobTypeIds.SImg);
                if (Authorization.Has(Claims.Job.Types.ShowSOS))
                    allowedTypes.Add(JobType.JobTypeIds.SOS);
                if (Authorization.Has(Claims.Job.Types.ShowUMgmt))
                    allowedTypes.Add(JobType.JobTypeIds.UMgmt);

                return allowedTypes;
            }
            return null;
        }

        public static IQueryable<Job> FilterPermissions(this JobTableModel model, IQueryable<Job> Jobs, AuthorizationToken Authorization)
        {
            var allowedTypes = FilterAllowedTypes(Authorization);

            if (allowedTypes != null)
            {
                return Jobs.Where(j => allowedTypes.Contains(j.JobTypeId));
            }

            return Jobs;
        }

        public static List<JobTableModel.JobTableItemModel> PermissionsFiltered(this List<JobTableModel.JobTableItemModel> Items, AuthorizationToken Authorization)
        {
            if (Items != null && Items.Count > 0)
            {
                var allowedTypes = FilterAllowedTypes(Authorization);

                if (allowedTypes != null)
                {
                    return Items.Where(j => allowedTypes.Contains(j.TypeId)).ToList();
                }
            }

            return Items;
        }

        public static List<JobTableModel.JobTableItemModel> DetermineItems(this JobTableModel model, DiscoDataContext Database, IQueryable<Job> Jobs)
        {
            List<JobTableModel.JobTableItemModel> items;

            // Permissions
            var auth = UserService.CurrentAuthorization;
            if (!auth.HasAll(Claims.Job.Types.ShowHMisc, Claims.Job.Types.ShowHNWar, Claims.Job.Types.ShowHWar, Claims.Job.Types.ShowSApp, Claims.Job.Types.ShowSImg, Claims.Job.Types.ShowSOS, Claims.Job.Types.ShowUMgmt))
            {
                // Must Filter
                List<string> allowedTypes = new List<string>(6);
                if (auth.Has(Claims.Job.Types.ShowHMisc))
                    allowedTypes.Add(JobType.JobTypeIds.HMisc);
                if (auth.Has(Claims.Job.Types.ShowHNWar))
                    allowedTypes.Add(JobType.JobTypeIds.HNWar);
                if (auth.Has(Claims.Job.Types.ShowHWar))
                    allowedTypes.Add(JobType.JobTypeIds.HWar);
                if (auth.Has(Claims.Job.Types.ShowSApp))
                    allowedTypes.Add(JobType.JobTypeIds.SApp);
                if (auth.Has(Claims.Job.Types.ShowSImg))
                    allowedTypes.Add(JobType.JobTypeIds.SImg);
                if (auth.Has(Claims.Job.Types.ShowSOS))
                    allowedTypes.Add(JobType.JobTypeIds.SOS);
                if (auth.Has(Claims.Job.Types.ShowUMgmt))
                    allowedTypes.Add(JobType.JobTypeIds.UMgmt);

                Jobs = Jobs.Where(j => allowedTypes.Contains(j.JobTypeId));
            }

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
                model.ShowDeviceAddress = Database.DiscoConfiguration.MultiSiteMode;

            foreach (var j in items)
                if (j.DeviceAddressId.HasValue)
                    j.DeviceAddress = Database.DiscoConfiguration.OrganisationAddresses.GetAddress(j.DeviceAddressId.Value).Name;

            return items;
        }

        public static void Fill(this JobTableModel model, DiscoDataContext Database, IQueryable<Job> Jobs)
        {
            model.Items = model.DetermineItems(Database, Jobs);
        }
    }
}
