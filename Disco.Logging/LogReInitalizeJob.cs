using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quartz;
using Disco.Data.Repository;

namespace Disco.Logging
{
    class LogReInitalizeJob : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            using (DiscoDataContext DiscoContext = new DiscoDataContext())
            {
                LogContext.ReInitalize(DiscoContext);
            }
        }
    }
}
