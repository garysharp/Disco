using Disco.Data.Repository;
using Disco.Services.Tasks;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Disco.Services.Interop.ActiveDirectory
{
    public class ADManagedGroupsSyncTask : ScheduledTask
    {
        public override string TaskName { get { return "Active Directory - Synchronise Managed Groups"; } }
        public override bool SingleInstanceTask { get { return true; } }
        public override bool CancelInitiallySupported { get { return false; } }

        public override void InitalizeScheduledTask(DiscoDataContext Database)
        {
            // ADManagedGroupsSyncTask @ 11:00pm
            TriggerBuilder triggerBuilder = TriggerBuilder.Create().
                WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(23, 0));

            ScheduleTask(triggerBuilder);
        }

        protected override void ExecuteTask()
        {
            int changeCount;

            List<ADManagedGroup> managedGroups = ExecutionContext.JobDetail.JobDataMap["ManagedGroups"] as List<ADManagedGroup>;
            if (managedGroups == null)
                managedGroups = ActiveDirectory.Context.ManagedGroups.Values;

            Status.UpdateStatus(0, "Synchronising Active Directory Managed Groups", "Starting");

            changeCount = ActiveDirectory.Context.ManagedGroups.SyncManagedGroups(managedGroups, Status);

            Status.LogInformation($"Synchronised Active Directory Managed Groups, {changeCount:N0} changes made");
            Status.SetFinishedMessage($"Made {changeCount} Changes to Active Directory Groups");
        }

        public static ScheduledTaskStatus ScheduleSync(ADManagedGroup ManagedGroup)
        {
            if (ManagedGroup == null)
                throw new ArgumentNullException("ManagedGroup");

            JobDataMap taskData = new JobDataMap() {
                {"ManagedGroups", new List<ADManagedGroup> { ManagedGroup } }
            };

            var instance = new ADManagedGroupsSyncTask();
            return instance.ScheduleTask(taskData);
        }
        public static ScheduledTaskStatus ScheduleSync(IEnumerable<ADManagedGroup> ManagedGroups)
        {
            if (ManagedGroups == null)
                throw new ArgumentNullException("ManagedGroups");

            JobDataMap taskData = new JobDataMap() {
                {"ManagedGroups", ManagedGroups.ToList() }
            };

            var instance = new ADManagedGroupsSyncTask();
            return instance.ScheduleTask(taskData);
        }
        public static ScheduledTaskStatus ScheduleSyncAll()
        {
            var instance = new ADManagedGroupsSyncTask();
            return instance.ScheduleTask();
        }
    }
}
