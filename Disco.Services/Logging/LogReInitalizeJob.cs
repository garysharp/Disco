using Quartz;
using Disco.Data.Repository;

namespace Disco.Services.Logging
{
    class LogReInitalizeJob : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            using (DiscoDataContext database = new DiscoDataContext())
            {
                LogContext.ReInitalize(database);
            }
        }
    }
}
