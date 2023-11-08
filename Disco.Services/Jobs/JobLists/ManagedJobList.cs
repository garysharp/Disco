using Disco.Data.Repository;
using Disco.Data.Repository.Monitor;
using Disco.Models.Repository;
using Disco.Models.Services.Jobs.JobLists;
using Disco.Services.Authorization;
using Disco.Services.Jobs.JobQueues;
using Disco.Services.Logging;
using Disco.Services.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace Disco.Services.Jobs.JobLists
{
    using FilterFunc = Func<IQueryable<Job>, IQueryable<Job>>;
    using OpenFilterFunc = Func<IEnumerable<JobTableStatusItemModel>, IEnumerable<JobTableStatusItemModel>>;
    using SortFunc = Func<IEnumerable<JobTableItemModel>, IEnumerable<JobTableItemModel>>;

    public class ManagedJobList : JobTableModel, IDisposable
    {
        private static ManagedJobList openJobs;

        private IDisposable unsubscribeToken;
        private object updateLock = new object();

        public string Name { get; private set; }
        public FilterFunc FilterFunction { get; private set; }
        public SortFunc SortFunction { get; private set; }

        public static OpenFilterFunc AwaitingTechnicianActionFilter = q => q.Where(j => j.ClosedDate == null && !j.WaitingForUserAction.HasValue
                && !(j.JobTypeId == JobType.JobTypeIds.HNWar && (j.JobMetaNonWarranty_RepairerLoggedDate.HasValue && j.JobMetaNonWarranty_IsInsuranceClaim.Value && !j.JobMetaInsurance_ClaimFormSentDate.HasValue))
                && !(j.JobTypeId == JobType.JobTypeIds.HNWar && (j.JobMetaNonWarranty_RepairerLoggedDate.HasValue && !j.JobMetaNonWarranty_RepairerCompletedDate.HasValue))
                && !(j.JobTypeId == JobType.JobTypeIds.HNWar && (j.JobMetaNonWarranty_RepairerLoggedDate.HasValue && j.JobMetaNonWarranty_AccountingChargeAddedDate.HasValue && !j.JobMetaNonWarranty_AccountingChargePaidDate.HasValue))
                && !(j.JobTypeId == JobType.JobTypeIds.HWar && (j.JobMetaWarranty_ExternalLoggedDate.HasValue && !j.JobMetaWarranty_ExternalCompletedDate.HasValue))
                && (j.DeviceHeld == null || j.DeviceReturnedDate != null || j.DeviceReadyForReturn == null));

        static ManagedJobList()
        {
            using (var Database = new DiscoDataContext())
            {
                openJobs = new ManagedJobList(
                    Name: "Open Jobs",
                    FilterFunction: q => q.Where(j => !j.ClosedDate.HasValue),
                    SortFunction: q => q)
                {
                    ShowDeviceAddress = true,
                    ShowLocation = true,
                    ShowStatus = true
                }.Initialize(Database, false);
            }
        }

        public static JobTableModel OpenJobsTable(OpenFilterFunc FilterFunction)
        {
            return new JobTableModel()
            {
                ShowStatus = true,
                Items = FilterFunction(openJobs.Items.Cast<JobTableStatusItemModel>()).PermissionsFiltered(UserService.CurrentAuthorization)
            };
        }

        public static JobTableModel MyJobsTable(AuthorizationToken AuthToken)
        {
            var openJobs = ManagedJobList.openJobs.Items.PermissionsFiltered(AuthToken).Cast<JobTableStatusItemModel>();
            IEnumerable<Tuple<JobTableStatusItemModel, byte, byte, DateTime?>> allJobs = null;

            if (AuthToken.Has(Claims.Job.Lists.MyJobsOrphaned))
            {
                allJobs = AwaitingTechnicianActionFilter(
                    openJobs.Where(i => i.ActiveJobQueues == null || i.ActiveJobQueues.Count() == 0)
                    ).Select(i => new Tuple<JobTableStatusItemModel, byte, byte, DateTime?>(i, (byte)JobQueuePriority.Normal, (byte)JobQueuePriority.Normal, null));
            }

            var usersQueues = JobQueueService.UsersQueues(AuthToken).ToDictionary(q => q.JobQueue.Id);

            var queueJobs = openJobs
                .Where(i => i.ActiveJobQueues != null && i.ActiveJobQueues.Any(jqj => usersQueues.ContainsKey(jqj.QueueId)))
                .Select(i => new Tuple<JobTableStatusItemModel, byte, byte, DateTime?>(
                    i,
                    (byte)i.ActiveJobQueues.Where(q => usersQueues.ContainsKey(q.QueueId)).Max(q => (int)usersQueues[q.QueueId].JobQueue.Priority),
                    (byte)i.ActiveJobQueues.Where(q => usersQueues.ContainsKey(q.QueueId)).Max(q => (int)q.Priority),
                    i.ActiveJobQueues.Where(q => usersQueues.ContainsKey(q.QueueId) && q.SLAExpiresDate.HasValue).Min(q => q.SLAExpiresDate)
                    ));

            if (allJobs != null)
                allJobs = allJobs.Concat(queueJobs).ToList();
            else
                allJobs = queueJobs.ToList();

            var allJobsSorted = allJobs
                .OrderByDescending(i => i.Item2).ThenByDescending(i => i.Item3).ThenBy(i => i.Item4).ThenBy(i => i.Item1.OpenedDate).Select(q => q.Item1);

            return new JobTableModel()
            {
                ShowStatus = true,
                Items = allJobsSorted.ToList()
            };
        }

        public override IEnumerable<JobTableItemModel> Items
        {
            get
            {
                return base.Items.PermissionsFiltered(UserService.CurrentAuthorization);
            }
            set
            {
                throw new InvalidOperationException("Items cannot be manually set in a Managed Job List");
            }
        }

        public ManagedJobList(string Name, FilterFunc FilterFunction, SortFunc SortFunction)
        {
            this.Name = Name;
            this.FilterFunction = FilterFunction;
            this.SortFunction = SortFunction;
        }

        public ManagedJobList Initialize(DiscoDataContext Database, bool FilterAuthorization)
        {
            // Can only Initialize once
            if (base.Items != null)
                return ReInitialize(Database, FilterAuthorization);

            lock (updateLock)
            {
                // Subscribe for Changes
                // - Job (or Job Meta) Changes
                // - Device Profile Address Changes (for multi-campus Schools)
                // - Device Model Description Changes
                // - Device's Profile or Model Changes
                unsubscribeToken = RepositoryMonitor.StreamAfterCommit
                    .Where(n => n.EntityType == typeof(Job) ||
                        n.EntityType == typeof(JobLog) ||
                        n.EntityType == typeof(JobAttachment) ||
                        n.EntityType == typeof(JobQueueJob) ||
                        n.EntityType == typeof(JobMetaWarranty) ||
                        n.EntityType == typeof(JobMetaNonWarranty) ||
                        n.EntityType == typeof(JobMetaInsurance) ||
                        (n.EventType == RepositoryMonitorEventType.Modified && (
                            (n.EntityType == typeof(DeviceProfile) && n.ModifiedProperties.Contains("DefaultOrganisationAddress")) ||
                            (n.EntityType == typeof(DeviceModel) && n.ModifiedProperties.Contains("Description")) ||
                            (n.EntityType == typeof(Device) && n.ModifiedProperties.Contains("DeviceProfileId") || n.ModifiedProperties.Contains("DeviceModelId"))
                        )))
                    .Subscribe(JobNotification, NotificationError);

                // Initially fill table
                base.Items = SortFunction(this.DetermineItems(Database, FilterFunction(Database.Jobs), FilterAuthorization)).ToList();
            }
            return this;
        }

        public ManagedJobList ReInitialize(DiscoDataContext Database, bool FilterAuthorization)
        {
            return ReInitialize(Database, null, null, FilterAuthorization);
        }
        public ManagedJobList ReInitialize(DiscoDataContext Database, FilterFunc FilterFunction, bool FilterAuthorization)
        {
            return ReInitialize(Database, FilterFunction, null, FilterAuthorization);
        }
        public ManagedJobList ReInitialize(DiscoDataContext Database, FilterFunc FilterFunction, SortFunc SortFunction, bool FilterAuthorization)
        {
            if (Database == null)
                throw new ArgumentNullException("Database");

            lock (updateLock)
            {
                if (FilterFunction != null)
                    this.FilterFunction = FilterFunction;
                if (SortFunction != null)
                    this.SortFunction = SortFunction;

                base.Items = this.SortFunction(this.DetermineItems(Database, this.FilterFunction(Database.Jobs), FilterAuthorization)).ToList();
            }
            return this;
        }

        private void NotificationError(Exception ex)
        {
            SystemLog.LogException(string.Format("Disco.Services.Jobs.JobLists.ManagedJobList: [{0}]", Name), ex);
        }

        private void JobNotification(RepositoryMonitorEvent e)
        {
            List<int> jobIds = null;
            JobTableItemModel[] existingItems = null;

            if (e.EntityType == typeof(Job))
                jobIds = new List<int>() { ((Job)e.Entity).Id };
            else if (e.EntityType == typeof(JobLog))
            {
                if (e.EventType == RepositoryMonitorEventType.Added)
                {
                    var jobLog = ((JobLog)e.Entity);
                    var job = base.Items.FirstOrDefault(i => i.JobId == jobLog.JobId);
                    if (job != null && job.LastActivityDate < jobLog.Timestamp)
                        job.LastActivityDate = jobLog.Timestamp;
                    return;
                }
                else
                    jobIds = new List<int>() { ((JobLog)e.Entity).JobId };
            }
            else if (e.EntityType == typeof(JobAttachment))
            {
                if (e.EventType == RepositoryMonitorEventType.Added)
                {
                    var jobAttachment = ((JobAttachment)e.Entity);
                    var job = base.Items.FirstOrDefault(i => i.JobId == jobAttachment.JobId);
                    if (job != null && job.LastActivityDate < jobAttachment.Timestamp)
                        job.LastActivityDate = jobAttachment.Timestamp;
                    return;
                }
                else
                    jobIds = new List<int>() { ((JobAttachment)e.Entity).JobId };
            }
            else if (e.EntityType == typeof(JobQueueJob))
                jobIds = new List<int>() { ((JobQueueJob)e.Entity).JobId };
            else if (e.EntityType == typeof(JobMetaWarranty))
                jobIds = new List<int>() { ((JobMetaWarranty)e.Entity).JobId };
            else if (e.EntityType == typeof(JobMetaNonWarranty))
                jobIds = new List<int>() { ((JobMetaNonWarranty)e.Entity).JobId };
            else if (e.EntityType == typeof(JobMetaInsurance))
                jobIds = new List<int>() { ((JobMetaInsurance)e.Entity).JobId };
            else if (e.EntityType == typeof(DeviceProfile))
            {
                int deviceProfileId = ((DeviceProfile)e.Entity).Id;
                existingItems = base.Items.Where(i => i.DeviceProfileId == deviceProfileId).ToArray();
            }
            else if (e.EntityType == typeof(DeviceModel))
            {
                int deviceModelId = ((DeviceModel)e.Entity).Id;
                existingItems = base.Items.Where(i => i.DeviceModelId == deviceModelId).ToArray();
            }
            else if (e.EntityType == typeof(Device))
            {
                string deviceSerialNumber = ((Device)e.Entity).SerialNumber;
                existingItems = base.Items.Where(i => i.DeviceSerialNumber == deviceSerialNumber).ToArray();
            }
            else
                return;  // Subscription should never reach

            if (jobIds == null)
            {
                if (existingItems == null)
                    throw new InvalidOperationException("Notification algorithm didn't indicate any Jobs for update");
                else
                    jobIds = existingItems.Select(i => i.JobId).ToList();
            }

            if (jobIds.Count == 0)
                return;
            else
                UpdateJobs(e.Database, jobIds, existingItems);
        }

        private void UpdateJobs(DiscoDataContext Database, List<int> jobIds, JobTableItemModel[] existingItems = null)
        {
            lock (updateLock)
            {
                // Check for existing items, if not handed them
                if (existingItems == null)
                    existingItems = base.Items.Where(i => jobIds.Contains(i.JobId)).ToArray();

                var updatedItems = this.DetermineItems(Database, FilterFunction(Database.Jobs.Where(j => jobIds.Contains(j.Id))), false);

                var refreshedList = base.Items.ToList();

                // Remove Existing
                if (existingItems.Length > 0)
                    foreach (var existingItem in existingItems)
                        refreshedList.Remove(existingItem);

                // Add Updated Items
                if (updatedItems.Count() > 0)
                    foreach (var updatedItem in updatedItems)
                        refreshedList.Add(updatedItem);

                // Reorder
                base.Items = SortFunction(refreshedList).ToList();
            }
        }

        public void Dispose()
        {
            if (unsubscribeToken != null)
            {
                unsubscribeToken.Dispose();
                unsubscribeToken = null;
            }
        }
    }
}
