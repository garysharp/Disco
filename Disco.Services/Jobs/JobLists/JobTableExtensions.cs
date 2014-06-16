using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Models.Services.Jobs.JobLists;
using Disco.Services.Authorization;
using Disco.Services.Interop.ActiveDirectory;
using Disco.Services.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Services
{
    public static class JobTableExtensions
    {
        private static JobTableModel CloneEmptyJobTableModel(JobTableModel Model)
        {
            return new JobTableModel()
            {
                ShowId = Model.ShowId,
                ShowDeviceAddress = Model.ShowDeviceAddress,
                ShowDates = Model.ShowDates,
                ShowType = Model.ShowType,
                ShowDevice = Model.ShowDevice,
                ShowUser = Model.ShowUser,
                ShowTechnician = Model.ShowTechnician,
                ShowLocation = Model.ShowLocation,
                ShowStatus = Model.ShowStatus,
                ShowLastActivityDate = Model.ShowLastActivityDate,
                IsSmallTable = Model.IsSmallTable,
                HideClosedJobs = Model.HideClosedJobs,
                EnablePaging = Model.EnablePaging,
                EnableFilter = Model.EnableFilter
            };
        }

        public static IDictionary<string, JobTableModel> MultiCampusModels(this JobTableModel Model)
        {
            var items = Model.Items;
            if (items == null || items.Count() > 0)
            {
                return items.OrderBy(i => i.DeviceAddress).GroupBy(i => i.DeviceAddress).ToDictionary(
                    ig => ig.Key ?? string.Empty,
                    ig =>
                    {
                        var jtm = CloneEmptyJobTableModel(Model);
                        jtm.Items = ig.ToList();
                        return jtm;
                    }
                );
            }
            else
            {
                return null;
            }
        }

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

        public static IEnumerable<JobTableItemModel> PermissionsFiltered(this IEnumerable<JobTableItemModel> Items, AuthorizationToken Authorization)
        {
            if (Items != null && Items.Count() > 0)
            {
                var allowedTypes = FilterAllowedTypes(Authorization);

                if (allowedTypes != null)
                {
                    return Items.Where(j => allowedTypes.Contains(j.JobTypeId)).ToList();
                }
            }

            return Items;
        }

        public static IEnumerable<JobTableItemModel> DetermineItems(this JobTableModel model, DiscoDataContext Database, IQueryable<Job> Jobs, bool FilterAuthorization)
        {
            List<JobTableItemModel> items;

            // Permissions
            if (FilterAuthorization)
                Jobs = model.FilterPermissions(Jobs, UserService.CurrentAuthorization);

            if (model.ShowStatus || model.ShowLastActivityDate)
            {

                var jobItems = Jobs.Select(j => new JobTableStatusItemModel()
                {
                    JobId = j.Id,
                    OpenedDate = j.OpenedDate,
                    ClosedDate = j.ClosedDate,
                    JobTypeId = j.JobTypeId,
                    JobTypeDescription = j.JobType.Description,
                    DeviceSerialNumber = j.Device.SerialNumber,
                    DeviceProfileId = j.Device.DeviceProfileId,
                    DeviceModelId = j.Device.DeviceModelId,
                    DeviceModelDescription = j.Device.DeviceModel.Description,
                    DeviceAddressId = j.Device.DeviceProfile.DefaultOrganisationAddress,
                    UserId = j.UserId,
                    UserDisplayName = j.User.DisplayName,
                    OpenedTechUserId = j.OpenedTechUserId,
                    OpenedTechUserDisplayName = j.OpenedTechUser.DisplayName,
                    DeviceHeldLocation = j.DeviceHeldLocation,
                    Flags = j.Flags,

                    JobMetaWarranty_ExternalReference = j.JobMetaWarranty.ExternalReference,
                    JobMetaWarranty_ExternalLoggedDate = j.JobMetaWarranty.ExternalLoggedDate,
                    JobMetaWarranty_ExternalCompletedDate = j.JobMetaWarranty.ExternalCompletedDate,
                    JobMetaNonWarranty_RepairerLoggedDate = j.JobMetaNonWarranty.RepairerLoggedDate,
                    JobMetaNonWarranty_RepairerCompletedDate = j.JobMetaNonWarranty.RepairerCompletedDate,
                    JobMetaNonWarranty_AccountingChargeAddedDate = j.JobMetaNonWarranty.AccountingChargeAddedDate,
                    JobMetaNonWarranty_AccountingChargePaidDate = j.JobMetaNonWarranty.AccountingChargePaidDate,
                    JobMetaNonWarranty_AccountingChargeRequiredDate = j.JobMetaNonWarranty.AccountingChargeRequiredDate,
                    JobMetaNonWarranty_IsInsuranceClaim = j.JobMetaNonWarranty.IsInsuranceClaim,
                    JobMetaInsurance_ClaimFormSentDate = j.JobMetaInsurance.ClaimFormSentDate,
                    JobMetaNonWarranty_InvoiceReceivedDate = j.JobMetaNonWarranty.InvoiceReceivedDate,
                    JobMetaNonWarranty_PurchaseOrderRaisedDate = j.JobMetaNonWarranty.PurchaseOrderRaisedDate,
                    JobMetaNonWarranty_PurchaseOrderSentDate = j.JobMetaNonWarranty.PurchaseOrderSentDate,

                    RecentAttachmentDate = j.JobAttachments.Max(ja => ja.Timestamp),
                    RecentLogDate = j.JobLogs.Max(jl => jl.Timestamp),

                    WaitingForUserAction = j.WaitingForUserAction,
                    DeviceReadyForReturn = j.DeviceReadyForReturn,
                    DeviceHeld = j.DeviceHeld,
                    DeviceReturnedDate = j.DeviceReturnedDate,
                    JobMetaWarranty_ExternalName = j.JobMetaWarranty.ExternalName,
                    JobMetaNonWarranty_RepairerName = j.JobMetaNonWarranty.RepairerName,
                    ActiveJobQueues = j.JobQueues.Where(jq => !jq.RemovedDate.HasValue).Select(jq => new JobTableStatusQueueItemModel()
                    {
                        Id = jq.Id,
                        QueueId = jq.JobQueueId,
                        AddedDate = jq.AddedDate,
                        SLAExpiresDate = jq.SLAExpiresDate,
                        Priority = jq.Priority
                    })
                });

                items = new List<JobTableItemModel>();
                foreach (var j in jobItems)
                {
                    j.StatusId = j.CalculateStatusId();
                    j.StatusDescription = JobExtensions.JobStatusDescription(j.StatusId, j);

                    var activityDates = new DateTime?[] {
                        j.ActiveJobQueues.Max<JobTableStatusQueueItemModel, DateTime?>(jq => jq.AddedDate),
                        j.ClosedDate,
                        j.DeviceHeld,
                        j.DeviceReadyForReturn,
                        j.DeviceReturnedDate,
                        j.JobMetaInsurance_ClaimFormSentDate,
                        j.JobMetaNonWarranty_AccountingChargeAddedDate,
                        j.JobMetaNonWarranty_AccountingChargePaidDate,
                        j.JobMetaNonWarranty_AccountingChargeRequiredDate,
                        j.JobMetaNonWarranty_InvoiceReceivedDate,
                        j.JobMetaNonWarranty_PurchaseOrderRaisedDate,
                        j.JobMetaNonWarranty_PurchaseOrderSentDate,
                        j.JobMetaNonWarranty_RepairerCompletedDate,
                        j.JobMetaNonWarranty_RepairerLoggedDate,
                        j.JobMetaWarranty_ExternalCompletedDate,
                        j.JobMetaWarranty_ExternalLoggedDate,
                        j.OpenedDate,
                        j.RecentAttachmentDate,
                        j.RecentLogDate,
                        j.WaitingForUserAction
                    };

                    j.LastActivityDate = activityDates.Max().Value;

                    items.Add(j);
                }
            }
            else
            {
                items = Jobs.Select(j => new JobTableItemModel()
                {
                    JobId = j.Id,
                    OpenedDate = j.OpenedDate,
                    ClosedDate = j.ClosedDate,
                    JobTypeId = j.JobTypeId,
                    JobTypeDescription = j.JobType.Description,
                    DeviceSerialNumber = j.Device.SerialNumber,
                    DeviceProfileId = j.Device.DeviceProfileId,
                    DeviceModelId = j.Device.DeviceModelId,
                    DeviceModelDescription = j.Device.DeviceModel.Description,
                    DeviceAddressId = j.Device.DeviceProfile.DefaultOrganisationAddress,
                    UserId = j.UserId,
                    UserDisplayName = j.User.DisplayName,
                    OpenedTechUserId = j.OpenedTechUserId,
                    OpenedTechUserDisplayName = j.OpenedTechUser.DisplayName,
                    DeviceHeldLocation = j.DeviceHeldLocation,
                    Flags = j.Flags
                }).ToList();
            }

            if (!model.ShowDeviceAddress.HasValue)
                model.ShowDeviceAddress = Database.DiscoConfiguration.MultiSiteMode;

            foreach (var j in items)
            {
                j.UserFriendlyId =j.UserId == null ? null : ActiveDirectory.FriendlyAccountId(j.UserId);
                j.OpenedTechUserFriendlyId = ActiveDirectory.FriendlyAccountId(j.OpenedTechUserId);

                if (j.DeviceAddressId.HasValue)
                    j.DeviceAddress = Database.DiscoConfiguration.OrganisationAddresses.GetAddress(j.DeviceAddressId.Value).Name;
            }

            return items;
        }

        public static JobTableModel Fill(this JobTableModel model, DiscoDataContext Database, IQueryable<Job> Jobs, bool FilterAuthorization)
        {
            model.Items = model.DetermineItems(Database, Jobs, FilterAuthorization);

            return model;
        }

        public static JobTableModel Score(this JobTableModel model, string Test, double Fuzziness = 0)
        {
            model.Items = model.Items.OrderByDescending(i => i.ScoreValues.Score(Test)).ToList();

            return model;
        }

        public static double? SlaPrecentageRemaining(this IEnumerable<JobTableStatusQueueItemModel> queueItems)
        {
            return queueItems.Where(i => i.SLAExpiresDate.HasValue).Min<JobTableStatusQueueItemModel, double?>(i =>
            {
                var total = (i.SLAExpiresDate.Value - i.AddedDate).Ticks;
                var remaining = (i.SLAExpiresDate.Value - DateTime.Now).Ticks;
                return ((double)remaining / total);
            });
        }

        public static IEnumerable<JobTableStatusQueueItemModel> UsersQueueItems(this IEnumerable<JobTableStatusQueueItemModel> queueItems, AuthorizationToken Authorization)
        {
            var usersQueues = Jobs.JobQueues.JobQueueService.UsersQueues(Authorization).ToDictionary(q => q.JobQueue.Id);
            return queueItems.Where(qi => usersQueues.ContainsKey(qi.QueueId));
        }

        public static IEnumerable<JobLocationReference> JobLocationReferences(this IEnumerable<JobTableStatusItemModel> Items, IEnumerable<string> IncludeLocations)
        {
            var innerItems = Items.Where(i => !string.IsNullOrWhiteSpace(i.DeviceHeldLocation) && i.DeviceHeld.HasValue && !i.DeviceReturnedDate.HasValue);

            return IncludeLocations.GroupJoin(innerItems, o => o, i => i.DeviceHeldLocation,
                (i, o) => new JobLocationReference
                {
                    Location = i,
                    References = o.ToList()
                },
                StringComparer.OrdinalIgnoreCase);
        }
        public static IEnumerable<JobLocationReference> JobLocationReferences(this IEnumerable<JobTableStatusItemModel> Items)
        {
            return Items.Where(i => !string.IsNullOrWhiteSpace(i.DeviceHeldLocation) && i.DeviceHeld.HasValue && !i.DeviceReturnedDate.HasValue)
                .GroupBy(i => i.DeviceHeldLocation, StringComparer.OrdinalIgnoreCase)
                .Select(i => new JobLocationReference()
                {
                    Location = i.Key,
                    References = i.ToList()
                });
        }
    }
}
