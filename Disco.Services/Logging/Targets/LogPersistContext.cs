using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace Disco.Services.Logging.Targets
{
    public class LogPersistContext : DbContext
    {
        public LogPersistContext(string ConnectionString) : base(ConnectionString) { }

        static LogPersistContext()
        {
            Database.SetInitializer<LogPersistContext>(new LogPersistContextInitializer());
        }

        public DbSet<Models.LogModule> Modules { get; set; }
        public DbSet<Models.LogEventType> EventTypes { get; set; }
        public DbSet<Models.LogEvent> Events { get; set; }
    }
}
