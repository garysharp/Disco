using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Objects;

namespace Disco.Data.Repository.Monitor
{
    public class RepositoryMonitorEvent
    {
        [JsonIgnore]
        internal ObjectStateEntry objectEntryState;
        [JsonIgnore]
        internal DbEntityEntry dbEntityState;
        [JsonIgnore]
        internal bool afterCommit;
        [JsonIgnore]
        internal List<Action<RepositoryMonitorEvent>> executeAfterCommit;

        [JsonIgnore]
        public DiscoDataContext Database { get; set; }

        public RepositoryMonitorEventType EventType { get; set; }

        [JsonIgnore]
        public Type EntityType { get; set; }

        public string EntityTypeName
        {
            get
            {
                return EntityType.Name;
            }
        }

        [JsonIgnore]
        public object Entity { get; set; }

        public Dictionary<string, object> EntityKey { get; set; }

        public string[] ModifiedProperties { get; set; }

        public T GetPreviousPropertyValue<T>(string PropertyName)
        {
            if (afterCommit)
                throw new InvalidOperationException("Unable to determine property values after repository commit, use a deferred action instead");
            else
                return (T)dbEntityState.OriginalValues[PropertyName];
        }
        public T GetCurrentPropertyValue<T>(string PropertyName)
        {
            return (T)dbEntityState.CurrentValues[PropertyName];
        }

        public void ExecuteAfterCommit(Action<RepositoryMonitorEvent> action){
            if (afterCommit)
            {
                // Execute Immediately
                action.Invoke(this);
            }
            else
            {
                // Defer Execution
                if (executeAfterCommit == null)
                    executeAfterCommit = new List<Action<RepositoryMonitorEvent>>();
                executeAfterCommit.Add(action);
            }
        }
    }
}
