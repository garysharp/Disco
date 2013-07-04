using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive;
using System.Reactive.Subjects;
using System.Data.Entity.Infrastructure;
using System.Collections.Concurrent;
using System.Data.Objects;
using Disco.Models.Repository;

namespace Disco.Data.Repository.Monitor
{
    public static class RepositoryMonitor
    {
        private static Subject<RepositoryMonitorEvent> streamBefore = new Subject<RepositoryMonitorEvent>();
        private static Subject<RepositoryMonitorEvent> streamAfter = new Subject<RepositoryMonitorEvent>();
        private static ConcurrentDictionary<Type, Type> entityProxyTypeCache = new ConcurrentDictionary<Type, Type>();

        public static Subject<RepositoryMonitorEvent> StreamBeforeCommit { get { return streamBefore; } }
        public static Subject<RepositoryMonitorEvent> StreamAfterCommit { get { return streamAfter; } }

        internal static RepositoryMonitorEvent[] BeforeSaveChanges(DiscoDataContext dbContext)
        {
            var contextStateManager = ((IObjectContextAdapter)dbContext).ObjectContext.ObjectStateManager;

            dbContext.ChangeTracker.DetectChanges();
            var changes = dbContext.ChangeTracker.Entries().Where(entry => entry.State == System.Data.EntityState.Added || entry.State == System.Data.EntityState.Deleted || entry.State == System.Data.EntityState.Modified);

            var events = changes.Select(entryState =>
            {
                ObjectStateEntry stateEntry = contextStateManager.GetObjectStateEntry(entryState.Entity);
                var monitorEvent = EventFromEntryState(dbContext, entryState, stateEntry);

                // Push to Stream
                streamBefore.OnNext(monitorEvent);

                return monitorEvent;
            }).ToArray();

            return events;
        }
        internal static void AfterSaveChanges(DiscoDataContext dbContext, IEnumerable<RepositoryMonitorEvent> changes)
        {
            foreach (var change in changes)
            {
                UpdateAfterEventFromEntryState(change);
                streamAfter.OnNext(change);
            }
        }

        private static Type EntityTypeFromProxy(Type EntityProxyType)
        {
            Type EntityType;

            if (entityProxyTypeCache.TryGetValue(EntityProxyType, out EntityType))
                return EntityType;

            EntityType = EntityProxyType;
            do
            {
                if (EntityType.Namespace.StartsWith("Disco.Models.Repository"))
                {
                    entityProxyTypeCache.TryAdd(EntityProxyType, EntityType);
                    return EntityType;
                }

                EntityType = EntityType.BaseType;
            } while (EntityType != null);

            throw new ArgumentException("The EntryProxyType does not inherit from any Repository Models", "EntityProxyType");
        }

        internal static void UpdateAfterEventFromEntryState(RepositoryMonitorEvent monitorEvent)
        {
            monitorEvent.afterCommit = true;

            if (monitorEvent.EventType == RepositoryMonitorEventType.Added)
            {
                // Update Entity Key for Added Events
                monitorEvent.EntityKey = DetermineEntityKey(monitorEvent.objectEntryState);
            }

            // Execute Deferred Actions
            if (monitorEvent.executeAfterCommit != null)
                foreach (var deferredAction in monitorEvent.executeAfterCommit)
                    deferredAction.Invoke(monitorEvent);
        }

        internal static RepositoryMonitorEvent EventFromEntryState(DiscoDataContext dbContext, DbEntityEntry entityEntry, ObjectStateEntry entryState)
        {
            RepositoryMonitorEventType eventType;
            string[] modifiedProperties = null;
            Dictionary<string, object> entityKey = null;
            Type entityType;

            switch (entryState.State)
            {
                case System.Data.EntityState.Added:
                    eventType = RepositoryMonitorEventType.Added;
                    break;
                case System.Data.EntityState.Deleted:
                    eventType = RepositoryMonitorEventType.Deleted;
                    break;
                case System.Data.EntityState.Detached:
                    eventType = RepositoryMonitorEventType.Detached;
                    break;
                case System.Data.EntityState.Modified:
                    eventType = RepositoryMonitorEventType.Modified;
                    break;
                case System.Data.EntityState.Unchanged:
                    eventType = RepositoryMonitorEventType.Unchanged;
                    break;
                default:
                    throw new NotSupportedException(string.Format("Database Entry State not supported: {0}", entryState.State.ToString()));
            }

            entityType = EntityTypeFromProxy(entryState.Entity.GetType());

            // Only pass modified properties on Modified Event
            if (eventType == RepositoryMonitorEventType.Modified)
                modifiedProperties = entryState.GetModifiedProperties().ToArray();
            else
                modifiedProperties = new string[] { }; // Empty array for Added/Deleted.

            // Don't pass entity key when entity newly added
            if (eventType != RepositoryMonitorEventType.Added)
                entityKey = DetermineEntityKey(entryState);

            return new RepositoryMonitorEvent()
            {
                EventType = eventType,
                Entity = entryState.Entity,
                EntityKey = entityKey,
                EntityType = entityType,
                ModifiedProperties = modifiedProperties,
                dbContext = dbContext,
                dbEntityState = entityEntry,
                objectEntryState = entryState
            };
        }

        internal static Dictionary<string, object> DetermineEntityKey(ObjectStateEntry entryState)
        {
            Dictionary<string, object> key = entryState.EntityKey.EntityKeyValues.ToDictionary(kv => kv.Key, kv => kv.Value);

            if (entryState.Entity is DeviceAttachment)
            {
                key["DeviceSerialNumber"] = ((DeviceAttachment)entryState.Entity).DeviceSerialNumber;
            }
            if (entryState.Entity is DeviceCertificate)
            {
                key["DeviceSerialNumber"] = ((DeviceCertificate)entryState.Entity).DeviceSerialNumber;
            }
            if (entryState.Entity is DeviceComponent)
            {
                key["DeviceModelId"] = ((DeviceComponent)entryState.Entity).DeviceModelId;
            }
            if (entryState.Entity is DeviceUserAssignment)
            {
                key["AssignedUserId"] = ((DeviceUserAssignment)entryState.Entity).AssignedUserId;
            }
            if (entryState.Entity is JobAttachment)
            {
                key["JobId"] = ((JobAttachment)entryState.Entity).JobId;
            }
            if (entryState.Entity is JobComponent)
            {
                key["JobId"] = ((JobComponent)entryState.Entity).JobId;
            }
            if (entryState.Entity is JobLog)
            {
                key["JobId"] = ((JobLog)entryState.Entity).JobId;
            }
            if (entryState.Entity is UserAttachment)
            {
                key["UserId"] = ((UserAttachment)entryState.Entity).UserId;
            }

            return key;
        }
    }
}
