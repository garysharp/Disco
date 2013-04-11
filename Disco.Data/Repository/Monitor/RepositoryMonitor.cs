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
            var changes = contextStateManager.GetObjectStateEntries(System.Data.EntityState.Added).Concat(contextStateManager.GetObjectStateEntries(System.Data.EntityState.Deleted)).Concat(contextStateManager.GetObjectStateEntries(System.Data.EntityState.Modified));

            var events = changes.Select(entryState =>
            {
                var monitorEvent = EventFromEntryState(entryState);

                // Push to Stream
                streamBefore.OnNext(monitorEvent);

                return monitorEvent;
            }).ToArray();

            return events;
        }
        internal static void AfterSaveChanges(DiscoDataContext dbContext, IEnumerable<RepositoryMonitorEvent> changes)
        {
            var contextStateManager = ((IObjectContextAdapter)dbContext).ObjectContext.ObjectStateManager;

            foreach (var change in changes)
            {
                UpdateAfterEventFromEntryState(change, contextStateManager);

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

        internal static void UpdateAfterEventFromEntryState(RepositoryMonitorEvent monitorEvent, ObjectStateManager stateManager)
        {
            if (monitorEvent.EventType == RepositoryMonitorEventType.Added)
            {
                // Update Entity Key for Added Events
                var entryState = stateManager.GetObjectStateEntry(monitorEvent.Entity);
                monitorEvent.EntityKey = entryState.EntityKey.EntityKeyValues.Select(kv => kv.Value).ToArray();
            }
        }

        internal static RepositoryMonitorEvent EventFromEntryState(ObjectStateEntry entryState)
        {
            RepositoryMonitorEventType eventType;
            string[] modifiedProperties = null;
            object[] entityKey = null;
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

            // Only pass modified properties on Modified Event (Ignore Added/Deleted)
            if (eventType == RepositoryMonitorEventType.Modified)
                modifiedProperties = entryState.GetModifiedProperties().ToArray();

            // Don't pass entity key when entity newly added
            if (eventType != RepositoryMonitorEventType.Added)
                entityKey = entryState.EntityKey.EntityKeyValues.Select(kv => kv.Value).ToArray();

            return new RepositoryMonitorEvent()
            {
                EventType = eventType,
                Entity = entryState.Entity,
                EntityKey = entityKey,
                EntityType = entityType,
                ModifiedProperties = modifiedProperties
            };
        }
    }
}
