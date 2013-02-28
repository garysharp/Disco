using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Disco.Services.Logging.Targets;
using Disco.Data.Repository;
using System.IO;
using System.Text.RegularExpressions;
using System.Data.SqlServerCe;
using Disco.Services.Logging.Models;

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
            if (this.Start.HasValue && this.End.HasValue && this.End.Value < this.Start.Value)
                throw new ArgumentOutOfRangeException("End", "End must be greater than Start");
            if (this.Start.HasValue && !this.End.HasValue && this.Start > DateTime.Now)
                throw new ArgumentOutOfRangeException("Start", "Start must be less than current time");

            return true;
        }

        public List<Models.LogLiveEvent> Query(DiscoDataContext DiscoContext)
        {
            List<Models.LogLiveEvent> results = new List<LogLiveEvent>();

            // Validate Options
            this.Validate();

            var relevantLogFiles = RelevantLogFiles(DiscoContext);
            relevantLogFiles.Reverse();
            foreach (var logFile in relevantLogFiles)
            {
                SqlCeConnectionStringBuilder sqlCeCSB = new SqlCeConnectionStringBuilder();
                sqlCeCSB.DataSource = logFile.Item1;

                var logModules = LogContext.LogModules;

                using (var context = new Targets.LogPersistContext(sqlCeCSB.ToString()))
                {
                    var query = this.BuildQuery(context, logFile.Item2, results.Count);
                    IEnumerable<LogEvent> queryResults = query; // Run the Query
                    results.AddRange(queryResults.Select(le => Models.LogLiveEvent.Create(logModules[le.ModuleId], logModules[le.ModuleId].EventTypes[le.EventTypeId], le.Timestamp, le.Arguments)));
                }
                if (this.Take.HasValue && this.Take.Value < results.Count)
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

        private List<Tuple<string, DateTime>> RelevantLogFiles(DiscoDataContext DiscoContext)
        {
            List<Tuple<string, DateTime>> relevantFiles = new List<Tuple<string, DateTime>>();
            var logDirectoryBase = LogContext.LogFileBasePath(DiscoContext);
            var logDirectoryBaseInfo = new DirectoryInfo(logDirectoryBase);
            var endDate = this.End.HasValue ? this.End.Value : DateTime.Now;
            var endDateYear = endDate.Year.ToString();

            // Try Shortcut ( < 31 Days in Query)
            if (this.Start.HasValue)
            {
                if ((this.End.HasValue && this.End.Value.Subtract(this.Start.Value).Days < 31) ||
                    (!this.End.HasValue && DateTime.Now.Subtract(this.Start.Value).Days < 31))
                {
                    // Less than 31 Days in Query - Just evaluate each Path
                    var queryDate = this.Start.Value.Date;
                    while (queryDate <= endDate)
                    {
                        var fileName = LogContext.LogFilePath(DiscoContext, queryDate, false);
                        if (File.Exists(fileName))
                            relevantFiles.Add(new Tuple<string, DateTime>(fileName, LogFileDate(fileName).Value));

                        queryDate = queryDate.AddDays(1);
                    }
                    return relevantFiles;
                }
            }

            List<string> logYears = new List<string>();
            foreach (var directoryName in logDirectoryBaseInfo.GetDirectories())
            {
                int directoryYear;
                if (int.TryParse(directoryName.Name, out directoryYear))
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
                        relevantFiles.Add(new Tuple<string, DateTime>(logFile, LogFileDate(logFile).Value));
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
                                relevantFiles.Add(new Tuple<string, DateTime>(logFile, fileNameDate.Value));
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
            if (this.Module.HasValue)
            {
                query = query.Where(le => le.ModuleId == this.Module.Value);
            }
            if (this.EventTypes != null && this.EventTypes.Count > 0)
            {
                query = query.Where(le => this.EventTypes.Contains(le.EventTypeId));
            }
            if (this.Start.HasValue && this.Start.Value > LogDate)
            {
                var startValue = DateTime.SpecifyKind(this.Start.Value, DateTimeKind.Local);
                query = query.Where(le => le.Timestamp > startValue);
            }
            if (this.End.HasValue && this.End.Value <= LogDate.AddDays(1))
            {
                var endValue = DateTime.SpecifyKind(this.End.Value, DateTimeKind.Local);
                query = query.Where(le => le.Timestamp < endValue);
            }
            if (this.Take.HasValue && this.Take.Value > 0)
            {
                var take = this.Take.Value - Taken;
                query = query.Take(take);
            }
            return query;
        }

    }
}
