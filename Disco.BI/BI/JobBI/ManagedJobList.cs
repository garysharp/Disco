using Disco.Data.Repository;
using Disco.Data.Repository.Monitor;
using Disco.Models.BI.Job;
using Disco.Models.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Disco.BI.Extensions;
using System.Reactive.Linq;
using Disco.Services.Users;

namespace Disco.BI.JobBI
{
    using FilterFunc = Func<IQueryable<Job>, IQueryable<Job>>;
    using SortFunc = Func<IEnumerable<Disco.Models.BI.Job.JobTableModel.JobTableItemModel>, IEnumerable<Disco.Models.BI.Job.JobTableModel.JobTableItemModel>>;
    using Disco.Services.Logging;

    public class ManagedJobList : JobTableModel, IDisposable
    {
        public string Name { get; set; }
        public FilterFunc FilterFunction { get; set; }
        public SortFunc SortFunction { get; set; }
        private IDisposable unsubscribeToken;
        private object updateLock = new object();
        
        public override List<JobTableItemModel> Items
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

        public ManagedJobList Initialize(DiscoDataContext Database)
        {
            // Can only Initialize once
            if (base.Items != null)
                return ReInitialize(Database);

            lock (updateLock)
            {
                // Subscribe for Changes
                // - Job (or Job Meta) Changes
                // - Device Profile Address Changes (for multi-campus Schools)
                // - Device Model Description Changes
                // - Device's Profile or Model Changes
                unsubscribeToken = RepositoryMonitor.StreamAfterCommit
                    .Where(n => n.EntityType == typeof(Job) ||
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
                base.Items = this.SortFunction(this.DetermineItems(Database, this.FilterFunction(Database.Jobs))).ToList();
            }
            return this;
        }

        public ManagedJobList ReInitialize(DiscoDataContext Database)
        {
            return ReInitialize(Database, null, null);
        }
        public ManagedJobList ReInitialize(DiscoDataContext Database, FilterFunc FilterFunction)
        {
            return ReInitialize(Database, FilterFunction, null);
        }
        public ManagedJobList ReInitialize(DiscoDataContext Database, FilterFunc FilterFunction, SortFunc SortFunction)
        {
            if (Database == null)
                throw new ArgumentNullException("Database");

            lock (updateLock)
            {
                if (FilterFunction != null)
                    this.FilterFunction = FilterFunction;
                if (SortFunction != null)
                    this.SortFunction = SortFunction;

                base.Items = this.SortFunction(this.DetermineItems(Database, this.FilterFunction(Database.Jobs))).ToList();
            }
            return this;
        }

        private void NotificationError(Exception ex)
        {
            SystemLog.LogException(string.Format("Disco.BI.JobBI.ManagedJobList: [{0}]", this.Name), ex);
        }

        private void JobNotification(RepositoryMonitorEvent e)
        {
            List<int> jobIds = null;
            JobTableItemModel[] existingItems = null;

            if (e.EntityType == typeof(Job))
                jobIds = new List<int>() { ((Job)e.Entity).Id };
            else
                if (e.EntityType == typeof(JobMetaWarranty))
                    jobIds = new List<int>() { ((JobMetaWarranty)e.Entity).JobId };
                else
                    if (e.EntityType == typeof(JobMetaNonWarranty))
                        jobIds = new List<int>() { ((JobMetaNonWarranty)e.Entity).JobId };
                    else
                        if (e.EntityType == typeof(JobMetaInsurance))
                            jobIds = new List<int>() { ((JobMetaInsurance)e.Entity).JobId };
                        else
                            if (e.EntityType == typeof(DeviceProfile))
                            {
                                int deviceProfileId = ((DeviceProfile)e.Entity).Id;
                                existingItems = base.Items.Where(i => i.DeviceProfileId == deviceProfileId).ToArray();
                            }
                            else
                                if (e.EntityType == typeof(DeviceModel))
                                {
                                    int deviceModelId = ((DeviceModel)e.Entity).Id;
                                    existingItems = base.Items.Where(i => i.DeviceModelId == deviceModelId).ToArray();
                                }
                                else
                                    if (e.EntityType == typeof(Device))
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
                    jobIds = existingItems.Select(i => i.Id).ToList();
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
                    existingItems = base.Items.Where(i => jobIds.Contains(i.Id)).ToArray();
                
                var updatedItems = this.DetermineItems(Database, this.FilterFunction(Database.Jobs.Where(j => jobIds.Contains(j.Id))));

                var refreshedList = base.Items.ToList();

                // Remove Existing
                if (existingItems.Length > 0)
                    foreach (var existingItem in existingItems)
                        refreshedList.Remove(existingItem);

                // Add Updated Items
                if (updatedItems.Count > 0)
                    foreach (var updatedItem in updatedItems)
                        refreshedList.Add(updatedItem);

                // Reorder
                base.Items = this.SortFunction(refreshedList).ToList();
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
