using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Services.Authorization;
using Disco.Services.Tasks;
using Disco.Services.Users;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Disco.Services.Jobs.JobQueues
{
    public static class JobQueueService
    {
        private const string _cacheHttpRequestKey = "Disco_UserQueuesToken_{0}";
        private static Cache _cache;

        public static void Initialize(DiscoDataContext Database)
        {
            _cache = new Cache(Database);
        }

        public static ReadOnlyCollection<KeyValuePair<string, string>> IconColours { get { return _cache.IconColours; } }
        public static ReadOnlyCollection<KeyValuePair<string, string>> Icons { get { return _cache.Icons; } }
        public static ReadOnlyCollection<KeyValuePair<int, string>> SlaOptions { get { return _cache.SlaOptions; } }

        public static ReadOnlyCollection<JobQueueToken> GetQueues() { return _cache.GetQueues(); }
        public static JobQueueToken GetQueue(int JobQueueId) { return _cache.GetQueue(JobQueueId); }

        #region Job Queues Maintenance
        public static JobQueueToken CreateJobQueue(DiscoDataContext Database, JobQueue JobQueue)
        {
            // Verify
            if (string.IsNullOrWhiteSpace(JobQueue.Name))
                throw new ArgumentException("The Job Queue Name is required");

            // Name Unique
            if (_cache.GetQueues().Count(q => q.JobQueue.Name == JobQueue.Name) > 0)
                throw new ArgumentException("Another Job Queue already exists with that name", "JobQueue");

            // Clone to break reference
            var queue = new JobQueue()
            {
                Name = JobQueue.Name,
                Description = JobQueue.Description,
                Icon = JobQueue.Icon,
                IconColour = JobQueue.IconColour,
                DefaultSLAExpiry = JobQueue.DefaultSLAExpiry,
                Priority = JobQueue.Priority,
                SubjectIds = JobQueue.SubjectIds
            };

            Database.JobQueues.Add(queue);
            Database.SaveChanges();

            return _cache.UpdateQueue(queue);
        }
        public static JobQueueToken UpdateJobQueue(DiscoDataContext Database, JobQueue JobQueue)
        {
            // Verify
            if (string.IsNullOrWhiteSpace(JobQueue.Name))
                throw new ArgumentException("The Job Queue Name is required");

            // Name Unique
            if (_cache.GetQueues().Count(q => q.JobQueue.Id != JobQueue.Id && q.JobQueue.Name == JobQueue.Name) > 0)
                throw new ArgumentException("Another Job Queue already exists with that name", "JobQueue");

            Database.SaveChanges();

            return _cache.UpdateQueue(JobQueue);
        }
        public static void DeleteJobQueue(DiscoDataContext Database, int JobQueueId, ScheduledTaskStatus Status)
        {
            JobQueue queue = Database.JobQueues.Find(JobQueueId);

            // Validate: Current Jobs?
            int currentJobCount = Database.JobQueueJobs.Count(jqj => jqj.JobQueueId == queue.Id && !jqj.RemovedDate.HasValue);
            if (currentJobCount > 0)
                throw new InvalidOperationException("The Job Queue cannot be deleted because it contains jobs");

            // Delete History
            Status.UpdateStatus(0, string.Format("Removing '{0}' [{1}] Job Queue", queue.Name, queue.Id), "Starting");
            var jobQueueJobs = Database.JobQueueJobs.Include("Job").Where(jsj => jsj.JobQueueId == queue.Id).ToList();
            if (jobQueueJobs.Count > 0)
            {
                double progressInterval = 90 / jobQueueJobs.Count;
                for (int jqjIndex = 0; jqjIndex < jobQueueJobs.Count; jqjIndex++)
                {
                    var jqj = jobQueueJobs[jqjIndex];

                    Status.UpdateStatus(jqjIndex * progressInterval, string.Format("Merging history into job #{0} logs", jqj.JobId));

                    // Write Logs
                    Database.JobLogs.Add(new JobLog()
                    {
                        JobId = jqj.JobId,
                        TechUserId = jqj.AddedUserId,
                        Timestamp = jqj.AddedDate,
                        Comments = string.Format("Added to Job Queue: {1}{0}Priority: {2}{0}Comment: {3}", Environment.NewLine, queue.Name, jqj.Priority.ToString(), string.IsNullOrWhiteSpace(jqj.AddedComment) ? "<none>" : jqj.AddedComment)
                    });
                    Database.JobLogs.Add(new JobLog()
                    {
                        JobId = jqj.JobId,
                        TechUserId = jqj.RemovedUserId,
                        Timestamp = jqj.RemovedDate.Value,
                        Comments = string.Format("Removed from Job Queue: {1}{0}Comment: {2}", Environment.NewLine, queue.Name, string.IsNullOrWhiteSpace(jqj.RemovedComment) ? "<none>" : jqj.RemovedComment)
                    });

                    // Delete JQJ
                    Database.JobQueueJobs.Remove(jqj);

                    // Save Changes
                    Database.SaveChanges();
                }
            }

            // Delete Queue
            Status.UpdateStatus(90, "Deleting Queue");
            Database.JobQueues.Remove(queue);
            Database.SaveChanges();

            // Remove from Cache
            _cache.RemoveQueue(JobQueueId);

            Status.Finished(string.Format("Successfully Deleted Job Queue: '{0}' [{1}]", queue.Name, queue.Id));
        }
        #endregion

        public static ReadOnlyCollection<JobQueueToken> UsersQueues(User User)
        {
            return UsersQueues(User.UserId);
        }
        public static ReadOnlyCollection<JobQueueToken> UsersQueues(string UserId)
        {
            var userAuth = UserService.GetAuthorization(UserId);

            return UsersQueues(userAuth);
        }
        public static ReadOnlyCollection<JobQueueToken> UsersQueues(AuthorizationToken UserAuthorization)
        {
            string cacheKey = string.Format(_cacheHttpRequestKey, UserAuthorization.User.UserId);
            ReadOnlyCollection<JobQueueToken> tokens = null;

            // Check for ASP.NET
            if (HttpContext.Current != null)
            {
                tokens = (ReadOnlyCollection<JobQueueToken>)HttpContext.Current.Items[_cacheHttpRequestKey];
            }

            if (tokens == null)
            {
                var subjectIds = (new string[] { UserAuthorization.User.UserId }).Concat(UserAuthorization.GroupMembership);
                tokens = _cache.GetQueuesForSubject(subjectIds);

                HttpContext.Current.Items[_cacheHttpRequestKey] = tokens;
            }

            return tokens;
        }

        public static string RandomIcon()
        {
            var rnd = new Random();
            var unusedIcons = _cache.Icons.Select(i => i.Key).Except(_cache.GetQueues().Select(q => q.JobQueue.Icon)).ToList();
            if (unusedIcons.Count > 0)
                return unusedIcons[rnd.Next(unusedIcons.Count - 1)];
            else
                return _cache.Icons[rnd.Next(_cache.Icons.Count - 1)].Key;
        }
        public static string RandomIconColour()
        {
            var rnd = new Random();
            var unusedColours = _cache.IconColours.Select(i => i.Key).Except(_cache.GetQueues().Select(q => q.JobQueue.IconColour)).ToList();
            if (unusedColours.Count > 0)
                return unusedColours[rnd.Next(unusedColours.Count - 1)];
            else
                return _cache.IconColours[rnd.Next(_cache.IconColours.Count - 1)].Key;
        }
    }
}
