using Disco.Models.Services.Exporting;
using Disco.Services.Tasks;
using System;

namespace Disco.Services.Exporting
{
    public class ExportTaskContext
    {
        public IExport ExportContext { get; }
        public ScheduledTaskStatus TaskStatus { get; internal set; }
        public ExportResult Result { get; internal set; }

        public Guid Id => ExportContext.Id;

        public ExportTaskContext(IExport context)
        {
            ExportContext = context;
        }
    }
}
