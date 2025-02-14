using Disco.Data.Repository;
using Disco.Models.Exporting;
using Disco.Models.Repository;
using Disco.Models.Services.Devices;
using Disco.Models.Services.Devices.DeviceFlag;
using Disco.Models.Services.Documents;
using Disco.Models.Services.Exporting;
using Disco.Models.Services.Jobs;
using Disco.Models.Services.Users.UserFlags;
using Disco.Services.Authorization;
using Disco.Services.Devices;
using Disco.Services.Devices.DeviceFlags;
using Disco.Services.Documents;
using Disco.Services.Jobs;
using Disco.Services.Logging;
using Disco.Services.Tasks;
using Disco.Services.Users.UserFlags;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Disco.Services.Exporting
{
    public static class SavedExports
    {
        private static Dictionary<string, (Type Type, string Name, Func<IExport, DiscoDataContext, IScheduledTaskStatus, ExportResult> ExporterDelegate)> exportTypes = new Dictionary<string, (Type, string, Func<IExport, DiscoDataContext, IScheduledTaskStatus, ExportResult>)>();

        static SavedExports()
        {
            RegisterExportType<DeviceFlagExport, DeviceFlagExportOptions, DeviceFlagExportRecord>();
            RegisterExportType<DeviceExport, DeviceExportOptions, DeviceExportRecord>();
            RegisterExportType<JobExport, JobExportOptions, JobExportRecord>();
            RegisterExportType<UserFlagExport, UserFlagExportOptions, UserFlagExportRecord>();
            RegisterExportType<DocumentExport, DocumentExportOptions, DocumentExportRecord>();
        }

        internal static void RegisterExportType<T, E, R>()
            where T : IExport<E, R>, new()
            where E : IExportOptions, new()
            where R : IExportRecord
        {
            var type = typeof(T);

            if (exportTypes.TryGetValue(type.Name, out var existing))
            {
                if (existing.Type != type)
                    throw new InvalidOperationException($"Export type already registered ({type.FullName})");
            }
            else
            {
                var name = new T().Name;
                exportTypes[type.Name] = (type, name, (i, d, s) => Exporter.Export((T)i, d, s));
            }
        }

        public static SavedExport SaveExport<T, R>(IExport<T, R> export, DiscoDataContext database, User createdBy)
            where T : IExportOptions, new()
            where R : IExportRecord
        {
            var exportType = export.GetType();
            if (!exportTypes.TryGetValue(exportType.Name, out var exportTypeRef) || exportType != exportTypeRef.Type)
                throw new InvalidOperationException($"Export type not registered for saving ({exportType.FullName})");

            var saved = new SavedExport()
            {
                Version = 1,
                Id = Guid.NewGuid(),
                CreatedOn = DateTime.Now,
                CreatedBy = createdBy.UserId,
                Type = exportType.Name,
                Config = Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(export, Formatting.None))),
                Enabled = false,
            };

            var exports = database.DiscoConfiguration.SavedExports;
            exports.Add(saved);
            database.DiscoConfiguration.SavedExports = exports;
            database.SaveChanges();

            return saved;
        }

        public static void DeleteSavedExport(DiscoDataContext database, Guid id)
        {
            var exports = database.DiscoConfiguration.SavedExports;
            var existing = exports.FirstOrDefault(e => e.Id == id);
            if (existing == null)
                return;
            exports.Remove(existing);
            database.DiscoConfiguration.SavedExports = exports;
            database.SaveChanges();
        }

        public static void UpdateSavedExport(DiscoDataContext database, SavedExport export)
        {
            var exports = database.DiscoConfiguration.SavedExports;
            var existing = exports.First(e => e.Id == export.Id);

            if (string.IsNullOrWhiteSpace(export.Name))
                throw new InvalidOperationException("Export name is required");

            existing.Name = export.Name;
            existing.Description = export.Description;

            if (string.IsNullOrWhiteSpace(export.FilePath) || export.Schedule == null || export.Schedule.WeekDays == 0)
            {
                existing.Schedule = null;
                existing.FilePath = null;
            }
            else
            {
                // new file path - file cannot exist
                if (existing.FilePath == null && File.Exists(export.FilePath))
                    throw new InvalidOperationException("Export file path already exists, delete the file before configuring the saved export");

                // directory must exist
                if (!Directory.Exists(Path.GetDirectoryName(export.FilePath)))
                    throw new InvalidOperationException("Invalid export file path, the directory does not exist");

                existing.FilePath = export.FilePath;
                existing.TimestampSuffix = export.TimestampSuffix;

                existing.Schedule = new SavedExportSchedule()
                {
                    Version = 1,
                    WeekDays = export.Schedule.WeekDays,
                    StartHour = export.Schedule.EndHour.HasValue ? Math.Min(export.Schedule.StartHour, export.Schedule.EndHour.Value) : export.Schedule.StartHour,
                    EndHour = !export.Schedule.EndHour.HasValue ? null : (export.Schedule.EndHour.Value == export.Schedule.StartHour ? (byte?)null : Math.Max(export.Schedule.StartHour, export.Schedule.EndHour.Value)),
                };
            }

            if (existing.FilePath != null && existing.Schedule == null)
                throw new InvalidOperationException("Export file path requires a schedule");

            if (export.OnDemandPrincipals == null || export.OnDemandPrincipals.Count == 0)
                existing.OnDemandPrincipals = null;
            else
                existing.OnDemandPrincipals = new List<string>(export.OnDemandPrincipals);

            existing.Enabled = true;

            database.DiscoConfiguration.SavedExports = exports;
            database.SaveChanges();
        }

        public static SavedExport GetSavedExport(DiscoDataContext database, Guid id, out string exportTypeName)
        {
            var export = database.DiscoConfiguration.SavedExports.FirstOrDefault(e => e.Id == id);

            if (export == null)
            {
                exportTypeName = null;
                return null;
            }

            exportTypeName = exportTypes[export.Type].Name;
            return export;
        }

        public static List<SavedExport> GetSavedExports(DiscoDataContext database, string type, out string exportTypeName)
        {
            var exports = database.DiscoConfiguration.SavedExports.Where(e => e.Type == type).ToList();

            if (exports.Count == 0)
            {
                exportTypeName = null;
                return null;
            }

            exportTypeName = exportTypes[type].Name;
            return exports;
        }

        public static List<SavedExport> GetSavedExports(DiscoDataContext database)
            => database.DiscoConfiguration.SavedExports
            .Where(e => e.Enabled && exportTypes.ContainsKey(e.Type))
            .ToList();

        public static Dictionary<string, string> GetSavedExportTypeNames()
            => exportTypes.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Name, StringComparer.Ordinal);

        public static bool IsAuthorized(SavedExport savedExport, AuthorizationToken authorization)
        {
            if (authorization.Has(Claims.Config.ManageSavedExports))
                return true;

            if (savedExport.OnDemandPrincipals == null || savedExport.OnDemandPrincipals.Count == 0)
                return false;

            if (savedExport.OnDemandPrincipals.Contains(authorization.User.UserId, StringComparer.OrdinalIgnoreCase))
                return true;

            if (savedExport.OnDemandPrincipals.Any(p => authorization.GroupMembership.Contains(p, StringComparer.OrdinalIgnoreCase)))
                return true;

            return false;
        }

        public static void EvaluateSavedExportSchedules()
        {
            using (var database = new DiscoDataContext())
            {
                CleanupSavedExports(database);

                var scheduledExports = GetScheduledExports(database).ToList();

                foreach (var scheduledExport in scheduledExports)
                {
                    EvaluateSavedExportSchedule(database, scheduledExport);
                }
            }
        }

        public static void EvaluateSavedExportSchedule(DiscoDataContext database, SavedExport scheduledExport)
        {
            ExportResult exportResult = null;
            try
            {
                exportResult = EvaluateSavedExport(database, scheduledExport);
            }
            catch (Exception ex)
            {
                SystemLog.LogException($"Failed to generate saved '{scheduledExport.Name}' [{scheduledExport.Id}]", ex);
                return;
            }

            var filePath = scheduledExport.FilePath;
            if (scheduledExport.TimestampSuffix)
            {
                var timestamp = DateTime.Now.ToString("yyyyMMdd-HH");
                var extension = Path.GetExtension(filePath);
                filePath = Path.Combine(Path.GetDirectoryName(filePath), $"{Path.GetFileNameWithoutExtension(filePath)}-{timestamp}{extension}");
            }

            try
            {
                using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    exportResult.Result.CopyTo(fileStream);
                }

                var allExports = database.DiscoConfiguration.SavedExports;
                var configExport = allExports.First(e => e.Id == scheduledExport.Id);
                configExport.LastRunOn = DateTime.Now;
                database.DiscoConfiguration.SavedExports = allExports;
                database.SaveChanges();

                SystemLog.LogInformation($"Saved '{scheduledExport.Name}' [{scheduledExport.Name}] wrote to '{filePath}'");
            }
            catch (Exception ex)
            {
                SystemLog.LogException($"Failed to write saved '{scheduledExport.Name}' [{scheduledExport.Id}] to '{filePath}'", ex);
            }
        }

        public static ExportResult EvaluateSavedExport(DiscoDataContext database, SavedExport savedExport)
        {
            var (exportType, _, exportDelegate) = exportTypes[savedExport.Type];
            var export = (IExport)JsonConvert.DeserializeObject(Encoding.UTF8.GetString(Convert.FromBase64String(savedExport.Config)), exportType);

            return exportDelegate(export, database, ScheduledTaskMockStatus.Create(export.Name));
        }

        private static void CleanupSavedExports(DiscoDataContext database)
        {
            var exports = database.DiscoConfiguration.SavedExports;
            var changed = false;

            for (int i = 0; i < exports.Count; i++)
            {
                var export = exports[i];
                if (export.Enabled)
                    continue;

                if (export.CreatedOn.AddDays(1) < DateTime.Now)
                {
                    exports.RemoveAt(i);
                    i--;
                    changed = true;
                }
            }

            if (changed)
            {
                database.DiscoConfiguration.SavedExports = exports;
                database.SaveChanges();
            }
        }

        private static IEnumerable<SavedExport> GetScheduledExports(DiscoDataContext database)
        {
            var exports = database.DiscoConfiguration.SavedExports;
            var now = DateTime.Now;
            var hour = now.Hour;
            var day = now.DayOfWeek;

            foreach (var export in exports)
            {
                if (!export.Enabled)
                    continue;

                if (string.IsNullOrEmpty(export.FilePath))
                    continue;

                // skip unknown export types
                if (!exportTypes.ContainsKey(export.Type))
                    continue;

                var schedule = export.Schedule;
                if (schedule == null)
                    continue;

                // scheduled for today?
                if (!schedule.IncludesDay(day))
                    continue;

                // always run if scheduled earlier today? (potentially missed)
                if (schedule.StartHour >= hour || export.LastRunOn.GetValueOrDefault().Date == DateTime.Today)
                {
                    // are we beyond the end hour?
                    if (schedule.EndHour.HasValue && hour > schedule.EndHour.Value)
                        continue;

                    // if no end hour and not the start hour, skip
                    if (!schedule.EndHour.HasValue && schedule.StartHour != hour)
                        continue;

                    // before the start hour, skip
                    if (hour < schedule.StartHour)
                        continue;
                }

                yield return export;
            }
        }

    }
}
