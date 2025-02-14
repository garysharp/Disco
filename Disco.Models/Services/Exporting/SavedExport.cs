using System;
using System.Collections.Generic;

namespace Disco.Models.Services.Exporting
{
    public class SavedExport
    {
        public int Version { get; set; } = 1;
        public Guid Id { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public SavedExportSchedule Schedule { get; set; }
        public bool TimestampSuffix { get; set; }
        public string FilePath { get; set; }
        public string Config { get; set; }
        public List<string> OnDemandPrincipals { get; set; }
        public bool Enabled { get; set; }
        public DateTime? LastRunOn { get; set; }
    }
}
