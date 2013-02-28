using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace Disco.Logging.Targets
{
    public class LogPersistContext : DbContext
    {
        public LogPersistContext(string ConnectionString) : base(ConnectionString) { }

        public DbSet<Models.LogModule> Modules { get; set; }
        public DbSet<Models.LogEventType> EventTypes { get; set; }
        public DbSet<Models.LogEvent> Events { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //modelBuilder.Conventions.Remove<IncludeMetadataConvention>();
        }
    }
}
