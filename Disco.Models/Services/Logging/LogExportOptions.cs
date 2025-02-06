using Disco.Models.Exporting;
using Disco.Models.Services.Exporting;
using System;
using System.Collections.Generic;

namespace Disco.Models.Services.Logging
{
    public class LogExportOptions : IExportOptions
    {
        public int Version { get; set; } = 1;
        public ExportFormat Format { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? ModuleId { get; set; }
        public List<int> EventTypeIds { get; set; }
        public int? Take { get; set; }
    }
}
