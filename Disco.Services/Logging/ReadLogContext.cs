using Disco.Data.Repository;
using Disco.Services.Logging.Models;
using Disco.Services.Logging.Persistance;
using System;
using System.Collections.Generic;
using System.Data.SqlServerCe;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Disco.Services.Logging
{
    public class ReadLogContext
    {
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
        public int? Take { get; set; }
        public int? Module { get; set; }
        public List<int> EventTypes { get; set; }

        public bool Validate()
        {
            if (Start.HasValue && End.HasValue && End.Value < Start.Value)
                throw new ArgumentOutOfRangeException("End", "End must be greater than Start");
            if (Start.HasValue && !End.HasValue && Start > DateTime.Now)
                throw new ArgumentOutOfRangeException("Start", "Start must be less than current time");

            return true;
        }

        public List<LogLiveEvent> Query(DiscoDataContext Database)
        {
            List<LogLiveEvent> results = new List<LogLiveEvent>();

            // Validate Options
            Validate();

            var relevantLogFiles = RelevantLogFiles(Database);
            relevantLogFiles.Reverse();
            foreach (var logFile in relevantLogFiles)
            {
                SqlCeConnectionStringBuilder sqlCeCSB = new SqlCeConnectionStringBuilder();
                sqlCeCSB.DataSource = logFile.Item1;

                var logModules = LogContext.LogModules;

                using (var context = new LogPersistContext(sqlCeCSB.ToString()))
                {
                    var query = BuildQuery(context, logFile.Item2, results.Count);
                    IEnumerable<LogEvent> queryResults = query; // Run the Query
                    results.AddRange(queryResults.Select(le => LogLiveEvent.Create(logModules[le.ModuleId], logModules[le.ModuleId].EventTypes[le.EventTypeId], le.Timestamp, le.Arguments)));
                }
                if (Take.HasValue && Take.Value < results.Count)
                    break;
            }
            return results;
        }

        private static Regex LogFileDateRegex = new Regex("DiscoLog_([0-9]{4})-([0-9]{2})-([0-9]{2}).sdf", RegexOptions.IgnoreCase);
        private static DateTime? LogFileDate(string LogFilePath)
        {
            var fileNameMatch = LogFileDateRegex.Match(LogFilePath);
            if (fileNameMatch.Success)
            {
                return new DateTime(int.Parse(fileNameMatch.Groups[1].Value),
                    int.Parse(fileNameMatch.Groups[2].Value),
                    int.Parse(fileNameMatch.Groups[3].Value));
            }
            else
            {
                return null;
            }
        }

        private List<Tuple<string, DateTime>> RelevantLogFiles(DiscoDataContext Database)
        {
            var relevantFiles = new List<Tuple<string, DateTime>>();
            var logDirectoryBase = LogContext.LogFileBasePath(Database);
            var logDirectoryBaseInfo = new DirectoryInfo(logDirectoryBase);
            var endDate = End.HasValue ? End.Value : DateTime.Now;
            var endDateYear = endDate.Year.ToString();

            // Try Shortcut ( < 31 Days in Query)
            if (Start.HasValue)
            {
                if ((End.HasValue && End.Value.Subtract(Start.Value).Days < 31) ||
                    (!End.HasValue && DateTime.Now.Subtract(Start.Value).Days < 31))
                {
                    // Less than 31 Days in Query - Just evaluate each Path
                    var queryDate = Start.Value.Date;
                    while (queryDate <= endDate)
                    {
                        var fileName = LogContext.LogFilePath(Database, queryDate, false);
                        if (File.Exists(fileName))
                            relevantFiles.Add(Tuple.Create(fileName, LogFileDate(fileName).Value));

                        queryDate = queryDate.AddDays(1);
                    }
                    return relevantFiles;
                }
            }

            List<string> logYears = new List<string>();
            foreach (var directoryName in logDirectoryBaseInfo.GetDirectories())
            {
                if (int.TryParse(directoryName.Name, out _))
                {
                    logYears.Add(directoryName.Name);
                }
            }
            logYears.Sort();

            foreach (var logYear in logYears)
            {
                List<string> logFiles = Directory.EnumerateFiles(Path.Combine(logDirectoryBase, logYear), "DiscoLog_*.sdf").ToList();
                logFiles.Sort();
                if (logYear != endDateYear)
                {
                    foreach (var logFile in logFiles)
                    {
                        relevantFiles.Add(Tuple.Create(logFile, LogFileDate(logFile).Value));
                    }
                }
                else
                {
                    foreach (var logFile in logFiles)
                    {
                        var fileNameDate = LogFileDate(logFile);
                        if (fileNameDate != null)
                        {
                            if (fileNameDate.Value < endDate)
                            {
                                relevantFiles.Add(Tuple.Create(logFile, fileNameDate.Value));
                            }
                            else
                            {
                                break; // Files are sorted, must be no more...
                            }
                        }
                    }
                    break; // Years are sorted, must be no more...
                }
            }
            return relevantFiles;
        }

        private IQueryable<LogEvent> BuildQuery(LogPersistContext LogContext, DateTime LogDate, int Taken)
        {
            IQueryable<LogEvent> query = LogContext.Events.OrderByDescending(le => le.Timestamp);
            if (Module.HasValue)
            {
                query = query.Where(le => le.ModuleId == Module.Value);
            }
            if (EventTypes != null && EventTypes.Count > 0)
            {
                query = query.Where(le => EventTypes.Contains(le.EventTypeId));
            }
            if (Start.HasValue && Start.Value > LogDate)
            {
                var startValue = DateTime.SpecifyKind(Start.Value, DateTimeKind.Local);
                query = query.Where(le => le.Timestamp > startValue);
            }
            if (End.HasValue && End.Value <= LogDate.AddDays(1))
            {
                var endValue = DateTime.SpecifyKind(End.Value, DateTimeKind.Local);
                query = query.Where(le => le.Timestamp < endValue);
            }
            if (Take.HasValue && Take.Value > 0)
            {
                var take = Take.Value - Taken;
                query = query.Take(take);
            }
            return query;
        }

    }
}
