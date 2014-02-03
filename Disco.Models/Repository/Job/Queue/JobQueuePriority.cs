using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Models.Repository
{
    public enum JobQueuePriority : byte
    {
        High = 2,
        Normal = 1,
        Low = 0
    }
}
