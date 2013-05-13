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

namespace Disco.BI.JobBI
{
    public class ManagedJobList : JobTableModel, IDisposable
    {
        public string Name { get; set; }
        public Func<IQueryable<Job>, IQueryable<Job>> FilterFunction { get; set; }
        public Func<IEnumerable<JobTableItemModel>, IEnumerable<JobTableItemModel>> SortFunction { get; set; }
        private IDisposable unsubscribeToken;
        private object updateLock = new object();

        public ManagedJobList Initialize(DiscoDataContext dbContext)
        {
            // Initially fill table
            this.Items = this.SortFunction(this.DetermineItems(dbContext, this.FilterFunction(dbContext.Jobs))).ToList();

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
                .Subscribe(JobNotification);

            return this;
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
                                existingItems = this.Items.Where(i => i.DeviceProfileId == deviceProfileId).ToArray();
                            }
                            else
                                if (e.EntityType == typeof(DeviceModel))
                                {
                                    int deviceModelId = ((DeviceModel)e.Entity).Id;
                                    existingItems = this.Items.Where(i => i.DeviceModelId == deviceModelId).ToArray();
                                }
                                else
                                    if (e.EntityType == typeof(Device))
                                    {
                                        string deviceSerialNumber = ((Device)e.Entity).SerialNumber;
                                        existingItems = this.Items.Where(i => i.DeviceSerialNumber == deviceSerialNumber).ToArray();
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
                UpdateJobs(e.dbContext, jobIds, existingItems);
        }

        private void UpdateJobs(DiscoDataContext dbContext, List<int> jobIds, JobTableItemModel[] existingItems = null)
        {
            lock (updateLock)
            {
                // Check for existing items, if not handed them
                if (existingItems == null)
                    existingItems = this.Items.Where(i => jobIds.Contains(i.Id)).ToArray();

                var updatedItems = this.DetermineItems(dbContext, this.FilterFunction(dbContext.Jobs.Where(j => jobIds.Contains(j.Id))));

                var refreshedList = this.Items.ToList();

                // Remove Existing
                if (existingItems.Length > 0)
                    foreach (var existingItem in existingItems)
                        refreshedList.Remove(existingItem);

                // Add Updated Items
                if (updatedItems.Count > 0)
                    foreach (var updatedItem in updatedItems)
                        refreshedList.Add(updatedItem);

                // Reorder
                this.Items = this.SortFunction(refreshedList).ToList();
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
