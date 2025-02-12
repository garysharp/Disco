using Disco.Models.Services.Exporting;
using Disco.Models.UI.Config.Export;
using Disco.Web.Areas.API.Models.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Disco.Web.Areas.Config.Models.Export
{
    public class EditModel : ConfigExportCreateModel, ConfigExportShowModel
    {
        [Required]
        public Guid Id { get; set; }
        public string ExportTypeName { get; set; }
        [Required]
        public string Name { get; set; }
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }
        public bool IsEnabled { get; set; }
        public string FilePath { get; set; }
        public bool TimestampSuffix { get; set; }

        public bool ScheduleEnabled { get; set; }
        public bool ScheduleMonday { get; set; }
        public bool ScheduleTuesday { get; set; }
        public bool ScheduleWednesday { get; set; }
        public bool ScheduleThursday { get; set; }
        public bool ScheduleFriday { get; set; }
        public bool ScheduleSaturday { get; set; }
        public bool ScheduleSunday { get; set; }

        [Range(0, 23)]
        public byte ScheduleStartHour { get; set; }
        [Range(0, 23)]
        public byte? ScheduleEndHour { get; set; }

        public List<SubjectDescriptorModel> OnDemandSubjects { get; set; }
        public List<string> OnDemandPrincipals { get; set; }

        public static EditModel FromNewSavedExport(SavedExport savedExport, string exportTypeName)
        {
            return new EditModel
            {
                Id = savedExport.Id,
                IsEnabled = false,
                ExportTypeName = exportTypeName,
                ScheduleMonday = true,
                ScheduleTuesday = true,
                ScheduleWednesday = true,
                ScheduleThursday = true,
                ScheduleFriday = true,
            };
        }

        public SavedExport ToSavedExport()
        {
            var weekDays = default(byte);
            if (ScheduleMonday) weekDays |= 1 << (int)DayOfWeek.Monday;
            if (ScheduleTuesday) weekDays |= 1 << (int)DayOfWeek.Tuesday;
            if (ScheduleWednesday) weekDays |= 1 << (int)DayOfWeek.Wednesday;
            if (ScheduleThursday) weekDays |= 1 << (int)DayOfWeek.Thursday;
            if (ScheduleFriday) weekDays |= 1 << (int)DayOfWeek.Friday;
            if (ScheduleSaturday) weekDays |= 1 << (int)DayOfWeek.Saturday;
            if (ScheduleSunday) weekDays |= 1 << (int)DayOfWeek.Sunday;

            return new SavedExport
            {
                Id = Id,
                Name = Name,
                Description = Description,
                FilePath = FilePath,
                TimestampSuffix = TimestampSuffix,
                Schedule = ScheduleEnabled ? new SavedExportSchedule
                {
                    WeekDays = weekDays,
                    StartHour = ScheduleStartHour,
                    EndHour = ScheduleEndHour,
                } : null,
                OnDemandPrincipals = OnDemandSubjects?.Select(s => s.Id).ToList(),
            };
        }
    }
}
