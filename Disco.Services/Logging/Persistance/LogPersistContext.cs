using System.Data.Entity;

namespace Disco.Services.Logging.Persistance
{
    public class LogPersistContext : DbContext
    {
        public LogPersistContext(string ConnectionString) : base(ConnectionString) { }

        static LogPersistContext()
        {
            Database.SetInitializer(new LogPersistContextInitializer());
        }

        public DbSet<Models.LogModule> Modules { get; set; }
        public DbSet<Models.LogEventType> EventTypes { get; set; }
        public DbSet<Models.LogEvent> Events { get; set; }
    }
}
