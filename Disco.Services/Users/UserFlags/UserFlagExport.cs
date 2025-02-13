using Disco.Data.Repository;
using Disco.Models.Exporting;
using Disco.Models.Services.Exporting;
using Disco.Models.Services.Users.UserFlags;
using Disco.Services.Exporting;
using Disco.Services.Plugins.Features.DetailsProvider;
using Disco.Services.Tasks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace Disco.Services.Users.UserFlags
{
    public sealed class UserFlagExport : IExport<UserFlagExportOptions, UserFlagExportRecord>
    {
        public Guid Id { get; set; }
        public string Name { get; } = "User Flag Export";
        public string Description { get; set; }
        public UserFlagExportOptions Options { get; set; }

        public string FilenamePrefix { get; } = "UserFlagExport";
        public string ExcelWorksheetName { get; } = "UserFlagExport";
        public string ExcelTableName { get; } = "UserFlags";

        public UserFlagExport(UserFlagExportOptions options)
        {
            Id = Guid.NewGuid();
            Options = options;
        }

        [JsonConstructor]
        public UserFlagExport()
            : this(UserFlagExportOptions.DefaultOptions())
        {
        }

        public static UserFlagExport Create()
            => new UserFlagExport(new UserFlagExportOptions());

        public ExportResult Export(DiscoDataContext database, IScheduledTaskStatus status)
            => Exporter.Export(this, database, status);

        public List<UserFlagExportRecord> BuildRecords(DiscoDataContext database, IScheduledTaskStatus status)
        {
            var query = database.UserFlagAssignments
                .Include(a => a.User.UserDetails)
                .Include(a => a.UserFlag)
                .Where(a => Options.UserFlagIds.Contains(a.UserFlagId));

            if (Options.CurrentOnly)
                query = query.Where(a => !a.RemovedDate.HasValue);

            // Update Users
            if (Options.HasAssignedUserDetails())
            {
                status.UpdateStatus(5, "Refreshing user details from Active Directory");
                var userIds = query.Select(d => d.UserId).Distinct().ToList();
                foreach (var userId in userIds)
                {
                    try
                    {
                        UserService.GetUser(userId, database, true);
                    }
                    catch (Exception) { } // Ignore Errors
                }
            }

            status.UpdateStatus(15, "Extracting records from the database");
            var assignments = query.ToList();
            var records = assignments.Select(a => new UserFlagExportRecord()
            {
                Assignment = a
            }).ToList();

            if (Options.UserDetailCustom)
            {
                status.UpdateStatus(50, "Extracting custom user detail records");

                var detailsService = new DetailsProviderService(database);
                var cache = new Dictionary<string, Dictionary<string, string>>(StringComparer.Ordinal);
                foreach (var record in records)
                {
                    if (!cache.TryGetValue(record.Assignment.UserId, out var details))
                        details = detailsService.GetDetails(record.Assignment.User);
                    record.UserCustomDetails = details;
                }
            }

            return records;
        }

        public ExportMetadata<UserFlagExportRecord> BuildMetadata(DiscoDataContext database, List<UserFlagExportRecord> records, IScheduledTaskStatus status)
        {
            status.UpdateStatus(80, "Building metadata");

            var metadata = new ExportMetadata<UserFlagExportRecord>();
            metadata.IgnoreShortNames.Add("User Flag");

            // User Flag
            metadata.Add(Options, o => o.Id, r => r.Assignment.UserFlagId);
            metadata.Add(Options, o => o.Name, r => r.Assignment.UserFlag.Name);
            metadata.Add(Options, o => o.Description, r => r.Assignment.UserFlag.Description);
            metadata.Add(Options, o => o.Icon, r => r.Assignment.UserFlag.Icon);
            metadata.Add(Options, o => o.IconColour, r => r.Assignment.UserFlag.IconColour);
            metadata.Add(Options, o => o.AssignmentId, r => r.Assignment.Id);
            metadata.Add(Options, o => o.AddedDate, r => r.Assignment.AddedDate);
            metadata.Add(Options, o => o.AddedUserId, r => r.Assignment.AddedUserId);
            metadata.Add(Options, o => o.RemovedUserId, r => r.Assignment.RemovedUserId);
            metadata.Add(Options, o => o.RemovedDate, r => r.Assignment.RemovedDate);
            metadata.Add(Options, o => o.Comments, r => r.Assignment.Comments);

            // User
            metadata.Add(Options, o => o.UserId, r => r.Assignment.User?.UserId);
            metadata.Add(Options, o => o.UserDisplayName, r => r.Assignment.User?.DisplayName);
            metadata.Add(Options, o => o.UserSurname, r => r.Assignment.User?.Surname);
            metadata.Add(Options, o => o.UserGivenName, r => r.Assignment.User?.GivenName);
            metadata.Add(Options, o => o.UserPhoneNumber, r => r.Assignment.User?.PhoneNumber);
            metadata.Add(Options, o => o.UserEmailAddress, r => r.Assignment.User?.EmailAddress);

            // User Custom Details
            if (Options.UserDetailCustom)
            {
                var keys = records.Where(r => r.UserCustomDetails != null).SelectMany(r => r.UserCustomDetails.Keys).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
                foreach (var key in keys.OrderBy(k => k, StringComparer.OrdinalIgnoreCase))
                {
                    metadata.Add(key, r => r.UserCustomDetails != null && r.UserCustomDetails.TryGetValue(key, out var value) ? value : null);
                }
            }

            return metadata;
        }
    }
}
