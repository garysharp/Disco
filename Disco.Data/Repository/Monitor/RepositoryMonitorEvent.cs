using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Objects;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Data.Repository.Monitor
{
    public class RepositoryMonitorEvent
    {
        [JsonIgnore]
        internal ObjectStateEntry objectEntryState { get; set; }
        [JsonIgnore]
        internal DbEntityEntry dbEntityState { get; set; }
        [JsonIgnore]
        internal bool afterCommit { get; set; }

        [JsonIgnore]
        public DiscoDataContext dbContext { get; set; }

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

        public object[] EntityKey { get; set; }

        public string[] ModifiedProperties { get; set; }

        public T GetPreviousPropertyValue<T>(string PropertyName)
        {
            if (afterCommit)
                throw new InvalidOperationException("Unable to determine property values after repository commit");
            else
                return (T)dbEntityState.OriginalValues[PropertyName];
        }
        public T GetCurrentPropertyValue<T>(string PropertyName)
        {
            return (T)dbEntityState.CurrentValues[PropertyName];
        }
    }
}
