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
        public Func<IQueryable<Job>, IQueryable<Job>> FilterFunction {get;set;}
        public Func<IEnumerable<JobTableItemModel>, IEnumerable<JobTableItemModel>> SortFunction { get; set; }
        private IDisposable unsubscribeToken;

        public ManagedJobList Initialize(DiscoDataContext dbContext)
        {
            // Initially fill table
            this.Items = this.SortFunction(this.DetermineItems(dbContext, this.FilterFunction(dbContext.Jobs))).ToList();

            // Subscribe for Changes
            unsubscribeToken = RepositoryMonitor.StreamAfterCommit
                .Where(n => n.EntityType == typeof(Job) ||
                    n.EntityType == typeof(JobMetaWarranty) ||
                    n.EntityType == typeof(JobMetaNonWarranty) ||
                    n.EntityType == typeof(JobMetaInsurance))
                .Subscribe(JobNotification);

            return this;
        }

        private void JobNotification(RepositoryMonitorEvent e)
        {
            int jobId;

            if (e.EntityType == typeof(Job))
                jobId = ((Job)e.Entity).Id;
            else
                if (e.EntityType == typeof(JobMetaWarranty))
                    jobId = ((JobMetaWarranty)e.Entity).JobId;
                else
                    if (e.EntityType == typeof(JobMetaNonWarranty))
                        jobId = ((JobMetaNonWarranty)e.Entity).JobId;
                    else
                        if (e.EntityType == typeof(JobMetaInsurance))
                            jobId = ((JobMetaInsurance)e.Entity).JobId;
                        else
                            return;  // Subscription should never reach

            var existingItem = this.Items.FirstOrDefault(i => i.Id == jobId);
            var updatedItem = this.DetermineItems(e.dbContext, this.FilterFunction(e.dbContext.Jobs.Where(j => j.Id == jobId)));

            var updatedItems = this.Items.ToList();

            // Remove Existing
            if (existingItem != null)
                updatedItems.Remove(existingItem);

            if (updatedItem.Count > 0)
            {
                // Add Item
                updatedItems.Add(updatedItem.First());
            }

            // Reorder
            this.Items = this.SortFunction(updatedItems).ToList();
        }

        public void Dispose()
        {
            unsubscribeToken.Dispose();
        }
    }
}
