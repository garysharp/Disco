using System.Data.Entity;

namespace Disco.Services.Logging.Persistance
{
    public class LogPersistContextInitializer : IDatabaseInitializer<LogPersistContext>
    {
        public void InitializeDatabase(LogPersistContext context)
        {
            if (!context.Database.Exists())
            {
                context.Database.Create();
            }
        }
    }
}
