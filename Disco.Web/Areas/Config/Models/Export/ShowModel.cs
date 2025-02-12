using Disco.Models.Services.Exporting;
using Disco.Services.Interop.ActiveDirectory;
using Disco.Web.Areas.API.Models.Shared;
using System;
using System.Linq;

namespace Disco.Web.Areas.Config.Models.Export
{
    public class ShowModel : EditModel
    {
        public bool WasSaved { get; set; }
        public bool WasExported { get; set; }

        public static EditModel FromSavedExport(SavedExport savedExport, string exportTypeName, bool wasSaved, bool wasExported)
        {
            var model = new ShowModel
            {
                Id = savedExport.Id,
                ExportTypeName = exportTypeName,
                Name = savedExport.Name,
                Description = savedExport.Description,
                IsEnabled = savedExport.Enabled,
                FilePath = savedExport.FilePath,
                TimestampSuffix = savedExport.TimestampSuffix,
                ScheduleEnabled = savedExport.Schedule != null,
                ScheduleSunday = savedExport.Schedule?.IncludesDay(DayOfWeek.Sunday) ?? false,
                ScheduleMonday = savedExport.Schedule?.IncludesDay(DayOfWeek.Monday) ?? true,
                ScheduleTuesday = savedExport.Schedule?.IncludesDay(DayOfWeek.Tuesday) ?? true,
                ScheduleWednesday = savedExport.Schedule?.IncludesDay(DayOfWeek.Wednesday) ?? true,
                ScheduleThursday = savedExport.Schedule?.IncludesDay(DayOfWeek.Thursday) ?? true,
                ScheduleFriday = savedExport.Schedule?.IncludesDay(DayOfWeek.Friday) ?? true,
                ScheduleSaturday = savedExport.Schedule?.IncludesDay(DayOfWeek.Saturday) ?? false,
                ScheduleStartHour = savedExport.Schedule?.StartHour ?? 0,
                ScheduleEndHour = savedExport.Schedule?.EndHour,
                WasSaved = wasSaved,
                WasExported = wasExported,
            };
            if (savedExport.OnDemandPrincipals != null)
            {
                model.OnDemandPrincipals = savedExport.OnDemandPrincipals;
                model.OnDemandSubjects = savedExport.OnDemandPrincipals
                    .Where(p => !string.IsNullOrWhiteSpace(p))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Select(p => ActiveDirectory.RetrieveADObject(p, true))
                    .Where(ad => ad is ADUserAccount || ad is ADGroup)
                    .Select(ad => SubjectDescriptorModel.FromActiveDirectoryObject(ad))
                    .ToList();
            }
            return model;
        }
    }
}
