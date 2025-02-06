﻿using Disco.Data.Repository;
using Disco.Models.Exporting;
using Disco.Models.Services.Exporting;
using Disco.Models.Services.Users.UserFlags;
using Disco.Services.Exporting;
using Disco.Services.Plugins.Features.DetailsProvider;
using Disco.Services.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;

namespace Disco.Services.Users.UserFlags
{
    using Metadata = ExportFieldMetadata<UserFlagExportRecord>;

    public class UserFlagExportContext : IExportContext<UserFlagExportOptions, UserFlagExportRecord>
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool TimestampSuffix { get; set; }
        public UserFlagExportOptions Options { get; set; }

        public string SuggestedFilenamePrefix { get; } = "UserFlagExport";
        public string ExcelWorksheetName { get; } = "UserFlagExport";
        public string ExcelTableName { get; } = "UserFlags";

        [JsonConstructor]
        private UserFlagExportContext()
        {
        }

        public UserFlagExportContext(string name, string description, bool timestampSuffix, UserFlagExportOptions options)
        {
            Id = Guid.NewGuid();
            Name = name;
            Description = description;
            TimestampSuffix = timestampSuffix;
            Options = options;
        }

        public UserFlagExportContext(UserFlagExportOptions options)
            : this("User Flag Export", null, true, options)
        {
        }

        public ExportResult Export(DiscoDataContext database, IScheduledTaskStatus status)
            => Exporter.Export(database, this, status);

        public List<UserFlagExportRecord> BuildRecords(DiscoDataContext database, IScheduledTaskStatus status)
        {
            var query = database.UserFlagAssignments
                .Include(a => a.User.UserDetails)
                .Include(a => a.UserFlag)
                .Where(a => Options.UserFlagIds.Contains(a.UserFlagId));

            if (Options.CurrentOnly)
                query = query.Where(a => !a.RemovedDate.HasValue);

            // Update Users
            if (Options.UserDisplayName ||
                Options.UserSurname ||
                Options.UserGivenName ||
                Options.UserPhoneNumber ||
                Options.UserEmailAddress)
            {
                status.UpdateStatus(5, "Refreshing user details from Active Directory");
                var userIds = query.Select(d => d.UserId).Distinct().ToList();
                foreach (var userId in userIds)
                {
                    try
                    {
                        UserService.GetUser(userId, database);
                    }
                    catch (Exception) { } // Ignore Errors
                }
            }

            status.UpdateStatus(15, "Extracting records from the database");

            var records = query.Select(a => new UserFlagExportRecord()
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

        public List<Metadata> BuildMetadata(DiscoDataContext database, List<UserFlagExportRecord> records, IScheduledTaskStatus status)
        {
            status.UpdateStatus(80, "Building metadata");

            IEnumerable<string> userDetailCustomKeys = null;
            if (Options.UserDetailCustom)
                userDetailCustomKeys = records.Where(r => r.UserCustomDetails != null).SelectMany(r => r.UserCustomDetails.Keys).Distinct(StringComparer.OrdinalIgnoreCase).ToList();

            var accessors = BuildAccessors(userDetailCustomKeys);

            return typeof(UserFlagExportOptions).GetProperties()
                .Where(p => p.PropertyType == typeof(bool))
                .Select(p => new
                {
                    property = p,
                    details = (DisplayAttribute)p.GetCustomAttributes(typeof(DisplayAttribute), false).FirstOrDefault()
                })
            .Where(p => p.details != null && p.property.Name != nameof(Options.CurrentOnly) && (bool)p.property.GetValue(Options))
            .SelectMany(p =>
            {
                    var fieldMetadata = accessors[p.property.Name];
                    fieldMetadata.ForEach(f =>
                    {
                        if (f.ColumnName == null)
                            f.ColumnName = (p.details.ShortName == "User Flag") ? p.details.Name : $"{p.details.ShortName} {p.details.Name}";
                    });
                    return fieldMetadata;
                }).ToList();
        }

        private static Dictionary<string, List<Metadata>> BuildAccessors(IEnumerable<string> userDetailsCustomKeys)
        {
            const string DateFormat = "yyyy-MM-dd";
            const string DateTimeFormat = DateFormat + " HH:mm:ss";

            Func<object, string> csvStringEncoded = (o) => o == null ? null : $"\"{((string)o).Replace("\"", "\"\"")}\"";
            Func<object, string> csvToStringEncoded = (o) => o == null ? null : o.ToString();
            Func<object, string> csvCurrencyEncoded = (o) => ((decimal?)o).HasValue ? ((decimal?)o).Value.ToString("C") : null;
            Func<object, string> csvDateEncoded = (o) => ((DateTime)o).ToString(DateFormat);
            Func<object, string> csvDateTimeEncoded = (o) => ((DateTime)o).ToString(DateTimeFormat);
            Func<object, string> csvNullableDateEncoded = (o) => ((DateTime?)o).HasValue ? csvDateEncoded(o) : null;
            Func<object, string> csvNullableDateTimeEncoded = (o) => ((DateTime?)o).HasValue ? csvDateTimeEncoded(o) : null;

            var metadata = new Dictionary<string, List<Metadata>>();

            // User Flag
            metadata.Add(nameof(UserFlagExportOptions.Id), new List<Metadata>() { new Metadata(nameof(UserFlagExportOptions.Id), typeof(string), r => r.Assignment.UserFlagId, csvToStringEncoded) });
            metadata.Add(nameof(UserFlagExportOptions.Name), new List<Metadata>() { new Metadata(nameof(UserFlagExportOptions.Name), typeof(string), r => r.Assignment.UserFlag.Name, csvStringEncoded) });
            metadata.Add(nameof(UserFlagExportOptions.Description), new List<Metadata>() { new Metadata(nameof(UserFlagExportOptions.Description), typeof(string), r => r.Assignment.UserFlag.Description, csvStringEncoded) });
            metadata.Add(nameof(UserFlagExportOptions.Icon), new List<Metadata>() { new Metadata(nameof(UserFlagExportOptions.Icon), typeof(string), r => r.Assignment.UserFlag.Icon, csvStringEncoded) });
            metadata.Add(nameof(UserFlagExportOptions.IconColour), new List<Metadata>() { new Metadata(nameof(UserFlagExportOptions.IconColour), typeof(string), r => r.Assignment.UserFlag.IconColour, csvStringEncoded) });
            metadata.Add(nameof(UserFlagExportOptions.AssignmentId), new List<Metadata>() { new Metadata(nameof(UserFlagExportOptions.AssignmentId), typeof(string), r => r.Assignment.Id, csvToStringEncoded) });
            metadata.Add(nameof(UserFlagExportOptions.AddedDate), new List<Metadata>() { new Metadata(nameof(UserFlagExportOptions.AddedDate), typeof(string), r => r.Assignment.AddedDate, csvDateTimeEncoded) });
            metadata.Add(nameof(UserFlagExportOptions.AddedUserId), new List<Metadata>() { new Metadata(nameof(UserFlagExportOptions.AddedUserId), typeof(string), r => r.Assignment.AddedUserId, csvStringEncoded) });
            metadata.Add(nameof(UserFlagExportOptions.RemovedUserId), new List<Metadata>() { new Metadata(nameof(UserFlagExportOptions.RemovedUserId), typeof(string), r => r.Assignment.RemovedUserId, csvStringEncoded) });
            metadata.Add(nameof(UserFlagExportOptions.RemovedDate), new List<Metadata>() { new Metadata(nameof(UserFlagExportOptions.RemovedDate), typeof(string), r => r.Assignment.RemovedDate, csvNullableDateTimeEncoded) });
            metadata.Add(nameof(UserFlagExportOptions.Comments), new List<Metadata>() { new Metadata(nameof(UserFlagExportOptions.Comments), typeof(string), r => r.Assignment.Comments, csvStringEncoded) });

            // User
            metadata.Add(nameof(UserFlagExportOptions.UserId), new List<Metadata>() { new Metadata(nameof(UserFlagExportOptions.UserId), typeof(string), r => r.Assignment.User?.UserId, csvStringEncoded) });
            metadata.Add(nameof(UserFlagExportOptions.UserDisplayName), new List<Metadata>() { new Metadata(nameof(UserFlagExportOptions.UserDisplayName), typeof(string), r => r.Assignment.User?.DisplayName, csvStringEncoded) });
            metadata.Add(nameof(UserFlagExportOptions.UserSurname), new List<Metadata>() { new Metadata(nameof(UserFlagExportOptions.UserSurname), typeof(string), r => r.Assignment.User?.Surname, csvStringEncoded) });
            metadata.Add(nameof(UserFlagExportOptions.UserGivenName), new List<Metadata>() { new Metadata(nameof(UserFlagExportOptions.UserGivenName), typeof(string), r => r.Assignment.User?.GivenName, csvStringEncoded) });
            metadata.Add(nameof(UserFlagExportOptions.UserPhoneNumber), new List<Metadata>() { new Metadata(nameof(UserFlagExportOptions.UserPhoneNumber), typeof(string), r => r.Assignment.User?.PhoneNumber, csvStringEncoded) });
            metadata.Add(nameof(UserFlagExportOptions.UserEmailAddress), new List<Metadata>() { new Metadata(nameof(UserFlagExportOptions.UserEmailAddress), typeof(string), r => r.Assignment.User?.EmailAddress, csvStringEncoded) });
            if (userDetailsCustomKeys != null)
            {
                var userDetailCustomFields = new List<Metadata>();
                foreach (var detailKey in userDetailsCustomKeys.OrderBy(k => k, StringComparer.OrdinalIgnoreCase))
                {
                    var key = detailKey;
                    userDetailCustomFields.Add(new Metadata(detailKey, detailKey, typeof(string), r => r.UserCustomDetails != null && r.UserCustomDetails.TryGetValue(key, out var value) ? value : null, csvStringEncoded));
                }
                metadata.Add(nameof(UserFlagExportOptions.UserDetailCustom), userDetailCustomFields);
            }

            return metadata;
        }
    }
}
