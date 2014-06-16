using Disco.Data.Repository;
using Disco.Data.Repository.Monitor;
using Disco.Models.Repository;
using System;
using System.Linq;
using System.Reactive.Linq;

namespace Disco.BI.DocumentTemplateBI.ManagedGroups
{
    public static class DocumentTemplateManagedGroups
    {
        internal static Lazy<IObservable<RepositoryMonitorEvent>> DeviceScopeRepositoryEvents;
        internal static Lazy<IObservable<RepositoryMonitorEvent>> JobScopeRepositoryEvents;
        internal static Lazy<IObservable<RepositoryMonitorEvent>> UserScopeRepositoryEvents;

        internal static Lazy<IObservable<RepositoryMonitorEvent>> DeviceRenameRepositoryEvents;
        internal static Lazy<IObservable<RepositoryMonitorEvent>> JobCloseRepositoryEvents;
        internal static Lazy<IObservable<RepositoryMonitorEvent>> DeviceAssignmentRepositoryEvents;

        static DocumentTemplateManagedGroups()
        {
            DeviceScopeRepositoryEvents =
                new Lazy<IObservable<RepositoryMonitorEvent>>(() =>
                    RepositoryMonitor.StreamAfterCommit.Where(e =>
                        e.EntityType == typeof(DeviceAttachment) &&
                        ((DeviceAttachment)e.Entity).DocumentTemplateId != null && (
                        (e.EventType == RepositoryMonitorEventType.Added) ||
                        (e.EventType == RepositoryMonitorEventType.Deleted)
                        )
                    ));
            JobScopeRepositoryEvents =
                new Lazy<IObservable<RepositoryMonitorEvent>>(() =>
                    RepositoryMonitor.StreamAfterCommit.Where(e =>
                        e.EntityType == typeof(JobAttachment) && (
                        (e.EventType == RepositoryMonitorEventType.Added) ||
                        (e.EventType == RepositoryMonitorEventType.Deleted)
                        )
                    ));
            UserScopeRepositoryEvents =
                new Lazy<IObservable<RepositoryMonitorEvent>>(() =>
                    RepositoryMonitor.StreamAfterCommit.Where(e =>
                        e.EntityType == typeof(UserAttachment) && (
                        (e.EventType == RepositoryMonitorEventType.Added) ||
                        (e.EventType == RepositoryMonitorEventType.Deleted)
                        )
                    ));

            DeviceRenameRepositoryEvents =
                new Lazy<IObservable<RepositoryMonitorEvent>>(() =>
                    RepositoryMonitor.StreamBeforeCommit.Where(e =>
                        e.EntityType == typeof(Device) &&
                        e.EventType == RepositoryMonitorEventType.Modified &&
                        e.ModifiedProperties.Contains("DeviceDomainId")
                        ));
            JobCloseRepositoryEvents =
                new Lazy<IObservable<RepositoryMonitorEvent>>(() =>
                    RepositoryMonitor.StreamAfterCommit.Where(e =>
                        e.EntityType == typeof(Job) &&
                        (((Job)e.Entity).DeviceSerialNumber != null || ((Job)e.Entity).UserId != null) &&
                        e.EventType == RepositoryMonitorEventType.Modified &&
                        e.ModifiedProperties.Contains("ClosedDate")
                        ));
            DeviceAssignmentRepositoryEvents =
                new Lazy<IObservable<RepositoryMonitorEvent>>(() =>
                    RepositoryMonitor.StreamBeforeCommit.Where(e =>
                        e.EntityType == typeof(Device) &&
                        e.EventType == RepositoryMonitorEventType.Modified &&
                        e.ModifiedProperties.Contains("AssignedUserId")
                        ));
        }

        public static void Initialize(DiscoDataContext Database)
        {
            Database.DocumentTemplates
                .Where(dp => dp.DevicesLinkedGroup != null || dp.UsersLinkedGroup != null)
                .ToList()
                .ForEach(dp =>
                {
                    DocumentTemplateDevicesManagedGroup.Initialize(dp);
                    DocumentTemplateUsersManagedGroup.Initialize(dp);
                });
        }
    }
}
