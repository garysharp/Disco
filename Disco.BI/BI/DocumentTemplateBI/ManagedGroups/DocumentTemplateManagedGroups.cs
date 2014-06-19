using Disco.Data.Repository;
using Disco.Data.Repository.Monitor;
using Disco.Models.Repository;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Disco.BI.DocumentTemplateBI.ManagedGroups
{
    public static class DocumentTemplateManagedGroups
    {
        internal static Lazy<IObservable<RepositoryMonitorEvent>> DeviceAttachmentAddRepositoryEvents;
        internal static Lazy<IObservable<RepositoryMonitorEvent>> JobAttachmentAddRepositoryEvents;
        internal static Lazy<IObservable<RepositoryMonitorEvent>> UserAttachmentAddRepositoryEvents;

        internal static Lazy<Subject<Tuple<DiscoDataContext, int, string, string>>> DeviceAttachmentRemoveEvents;
        internal static Lazy<Subject<Tuple<DiscoDataContext, int, string, int>>> JobAttachmentRemoveEvents;
        internal static Lazy<Subject<Tuple<DiscoDataContext, int, string, string>>> UserAttachmentRemoveEvents;

        internal static Lazy<IObservable<RepositoryMonitorEvent>> DeviceRenameRepositoryEvents;
        internal static Lazy<IObservable<RepositoryMonitorEvent>> JobCloseRepositoryEvents;
        internal static Lazy<IObservable<RepositoryMonitorEvent>> DeviceAssignmentRepositoryEvents;

        static DocumentTemplateManagedGroups()
        {
            DeviceAttachmentAddRepositoryEvents =
                new Lazy<IObservable<RepositoryMonitorEvent>>(() =>
                    RepositoryMonitor.StreamAfterCommit.Where(e =>
                        e.EntityType == typeof(DeviceAttachment) &&
                        ((DeviceAttachment)e.Entity).DocumentTemplateId != null &&
                        e.EventType == RepositoryMonitorEventType.Added
                    ));
            JobAttachmentAddRepositoryEvents =
                new Lazy<IObservable<RepositoryMonitorEvent>>(() =>
                    RepositoryMonitor.StreamAfterCommit.Where(e =>
                        e.EntityType == typeof(JobAttachment) &&
                        ((JobAttachment)e.Entity).DocumentTemplateId != null &&
                        e.EventType == RepositoryMonitorEventType.Added
                    ));
            UserAttachmentAddRepositoryEvents =
                new Lazy<IObservable<RepositoryMonitorEvent>>(() =>
                    RepositoryMonitor.StreamAfterCommit.Where(e =>
                        e.EntityType == typeof(UserAttachment) &&
                        ((UserAttachment)e.Entity).DocumentTemplateId != null &&
                        e.EventType == RepositoryMonitorEventType.Added
                    ));

            DeviceAttachmentRemoveEvents =
                new Lazy<Subject<Tuple<DiscoDataContext, int, string, string>>>(() => new Subject<Tuple<DiscoDataContext, int, string, string>>());
            JobAttachmentRemoveEvents =
                new Lazy<Subject<Tuple<DiscoDataContext, int, string, int>>>(() => new Subject<Tuple<DiscoDataContext, int, string, int>>());
            UserAttachmentRemoveEvents =
                new Lazy<Subject<Tuple<DiscoDataContext, int, string, string>>>(() => new Subject<Tuple<DiscoDataContext, int, string, string>>());

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

        public static void TriggerDeviceAttachmentDeleted(DiscoDataContext Database, int AttachmentId, string DocumentTemplateId, string DeviceSerialNumber)
        {
            if (DocumentTemplateId != null)
                DeviceAttachmentRemoveEvents.Value.OnNext(Tuple.Create(Database, AttachmentId, DocumentTemplateId, DeviceSerialNumber));
        }

        public static void TriggerJobAttachmentDeleted(DiscoDataContext Database, int AttachmentId, string DocumentTemplateId, int JobId)
        {
            if (DocumentTemplateId != null)
                JobAttachmentRemoveEvents.Value.OnNext(Tuple.Create(Database, AttachmentId, DocumentTemplateId, JobId));
        }

        public static void TriggerUserAttachmentDeleted(DiscoDataContext Database, int AttachmentId, string DocumentTemplateId, string UserId)
        {
            if (DocumentTemplateId != null)
                UserAttachmentRemoveEvents.Value.OnNext(Tuple.Create(Database, AttachmentId, DocumentTemplateId, UserId));
        }
    }
}
