using Disco.Data.Repository;
using Quartz;

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
