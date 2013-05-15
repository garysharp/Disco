using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Services.Logging.Targets
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
