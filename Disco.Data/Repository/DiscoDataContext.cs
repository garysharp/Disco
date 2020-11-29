using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using Disco.Models.Repository;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace Disco.Data.Repository
{
    public class DiscoDataContext : DbContext
    {
        private Lazy<Configuration.SystemConfiguration> _Configuration;

        public DiscoDataContext()
        {
            this._Configuration = new Lazy<Configuration.SystemConfiguration>(() => new Configuration.SystemConfiguration(this));
        }

        public virtual DbSet<ConfigurationItem> ConfigurationItems { get; set; }

        public virtual DbSet<DocumentTemplate> DocumentTemplates { get; set; }

        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<UserAttachment> UserAttachments { get; set; }
        public virtual DbSet<UserFlag> UserFlags { get; set; }
        public virtual DbSet<UserFlagAssignment> UserFlagAssignments { get; set; }
        public virtual DbSet<AuthorizationRole> AuthorizationRoles { get; set; }

        public virtual DbSet<DeviceUserAssignment> DeviceUserAssignments { get; set; }

        public virtual DbSet<Device> Devices { get; set; }
        public virtual DbSet<DeviceDetail> DeviceDetails { get; set; }
        public virtual DbSet<DeviceModel> DeviceModels { get; set; }
        public virtual DbSet<DeviceProfile> DeviceProfiles { get; set; }
        public virtual DbSet<DeviceBatch> DeviceBatches { get; set; }
        public virtual DbSet<DeviceBatchAttachment> DeviceBatchAttachments { get; set; }
        public virtual DbSet<DeviceComponent> DeviceComponents { get; set; }
        public virtual DbSet<DeviceAttachment> DeviceAttachments { get; set; }

        public virtual DbSet<DeviceCertificate> DeviceCertificates { get; set; }

        public virtual DbSet<Job> Jobs { get; set; }
        public virtual DbSet<JobType> JobTypes { get; set; }
        public virtual DbSet<JobSubType> JobSubTypes { get; set; }
        public virtual DbSet<JobLog> JobLogs { get; set; }
        public virtual DbSet<JobAttachment> JobAttachments { get; set; }
        public virtual DbSet<JobComponent> JobComponents { get; set; }

        public virtual DbSet<JobMetaWarranty> JobMetaWarranties { get; set; }
        public virtual DbSet<JobMetaNonWarranty> JobMetaNonWarranties { get; set; }
        public virtual DbSet<JobMetaInsurance> JobMetaInsurances { get; set; }

        public virtual DbSet<JobQueue> JobQueues { get; set; }
        public virtual DbSet<JobQueueJob> JobQueueJobs { get; set; }

        public Configuration.SystemConfiguration DiscoConfiguration
        {
            get
            {
                return this._Configuration.Value;
            }
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();

            modelBuilder.Entity<DeviceComponent>().HasMany(m => m.JobSubTypes).WithMany(m => m.DeviceComponents).Map(m => m.ToTable("DeviceComponents_JobSubTypes"));
            modelBuilder.Entity<DocumentTemplate>().HasMany(m => m.JobSubTypes).WithMany(m => m.AttachmentTypes).Map(m => m.ToTable("DocumentTemplates_JobSubTypes"));
            modelBuilder.Entity<JobQueue>().HasMany(m => m.JobSubTypes).WithMany(m => m.JobQueues).Map(m => m.ToTable("JobQueues_JobSubTypes"));

            modelBuilder.Entity<Job>().HasMany(m => m.JobSubTypes).WithMany(m => m.Jobs).Map(m => m.ToTable("Jobs_JobSubTypes"));
            modelBuilder.Entity<User>().HasMany(m => m.Jobs).WithOptional(m => m.User);
            modelBuilder.Entity<Device>().HasMany(m => m.Jobs).WithOptional(m => m.Device);
        }

        // Hook for Repository Monitor
        public override int SaveChanges()
        {
            int changeCount = 0;

            // Notify before changes are committed
            var changes = Monitor.RepositoryMonitor.BeforeSaveChanges(this);

            changeCount = base.SaveChanges();

            if (changes.Length > 0)
            {
                // Notify after changes are committed
                Monitor.RepositoryMonitor.AfterSaveChanges(this, changes);
            }

            return changeCount;
        }

    }
}
