using Disco.Models.Services.Exporting;
using Disco.Services.Tasks;

namespace Disco.Services.Exporting
{
    public class ExportTaskContext<T> where T : IExportOptions
    {
        public T Options { get; private set; }

        public ScheduledTaskStatus TaskStatus { get; set; }

        public ExportResult Result { get; set; }

        public ExportTaskContext(T Options)
        {
            this.Options = Options;
        }
    }
}
