using Newtonsoft.Json;
using System;
using System.Collections.Generic;
<<<<<<< HEAD
using System.Data.Entity.Infrastructure;
using System.Data.Objects;
=======
>>>>>>> origin/Repository-Monitor
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Data.Repository.Monitor
{
    public class RepositoryMonitorEvent
    {
<<<<<<< HEAD
        [JsonIgnore]
        internal ObjectStateEntry objectEntryState { get; set; }
        [JsonIgnore]
        internal DbEntityEntry dbEntityState { get; set; }
        [JsonIgnore]
        internal bool afterCommit { get; set; }

        [JsonIgnore]
        public DiscoDataContext dbContext { get; set; }

=======
>>>>>>> origin/Repository-Monitor
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
<<<<<<< HEAD

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
=======
>>>>>>> origin/Repository-Monitor
    }
}
