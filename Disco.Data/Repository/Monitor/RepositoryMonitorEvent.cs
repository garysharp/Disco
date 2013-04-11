using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Data.Repository.Monitor
{
    public class RepositoryMonitorEvent
    {
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
    }
}
