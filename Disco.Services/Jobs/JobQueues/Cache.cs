using Disco.Data.Repository;
using Disco.Models.Repository;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Disco.Services.Jobs.JobQueues
{
    internal class Cache
    {
        private ConcurrentDictionary<int, JobQueueToken> _Cache;
        private Dictionary<string, List<JobQueueToken>> _SubjectCache;

        private ReadOnlyCollection<KeyValuePair<int, string>> _SlaOptions;

        public Cache(DiscoDataContext Database)
        {
            Initialize(Database);
        }

        private void Initialize(DiscoDataContext Database)
        {
            // Queues from Database
            var queues = Database.JobQueues.ToList();

            // Add Queues to In-Memory Cache
            _Cache = new ConcurrentDictionary<int, JobQueueToken>(queues.Select(q => new KeyValuePair<int, JobQueueToken>(q.Id, JobQueueToken.FromJobQueue(q))));

            // Calculate Queue Subject Cache
            CalculateSubjectCache();

            #region Predefined Options
            // SLA Options
            if (_SlaOptions == null)
            {
                _SlaOptions = new List<KeyValuePair<int, string>>()
                {
                    new KeyValuePair<int, string>(0, "<None>"),
                    new KeyValuePair<int, string>(15, "15 minutes"),
                    new KeyValuePair<int, string>(30, "30 minutes"),
                    new KeyValuePair<int, string>(60, "1 hour"),
                    new KeyValuePair<int, string>(60 * 2, "2 hours"),
                    new KeyValuePair<int, string>(60 * 4, "4 hours"),
                    new KeyValuePair<int, string>(60 * 8, "8 hours"),
                    new KeyValuePair<int, string>(60 * 24, "1 day"),
                    new KeyValuePair<int, string>(60 * 24 * 2, "2 days"),
                    new KeyValuePair<int, string>(60 * 24 * 3, "3 days"),
                    new KeyValuePair<int, string>(60 * 24 * 4, "4 days"),
                    new KeyValuePair<int, string>(60 * 24 * 5, "5 days"),
                    new KeyValuePair<int, string>(60 * 24 * 6, "6 days"),
                    new KeyValuePair<int, string>(60 * 24 * 7, "1 week"),
                    new KeyValuePair<int, string>(60 * 24 * 7 * 2, "2 weeks"),
                    new KeyValuePair<int, string>(60 * 24 * 7 * 3, "3 weeks"),
                    new KeyValuePair<int, string>(60 * 24 * 7 * 4, "4 weeks"),
                    new KeyValuePair<int, string>(60 * 24 * 7 * 4 * 2, "2 months"),
                    new KeyValuePair<int, string>(60 * 24 * 7 * 4 * 3, "3 months"),
                    new KeyValuePair<int, string>(60 * 24 * 7 * 4 * 4, "4 months"),
                    new KeyValuePair<int, string>(60 * 24 * 7 * 4 * 5, "5 months"),
                    new KeyValuePair<int, string>(60 * 24 * 7 * 4 * 6, "6 months")
                }.AsReadOnly();
            }
            #endregion
        }
        private void CalculateSubjectCache()
        {
            _SubjectCache = _Cache.Values.ToList()
                .SelectMany(t => t.SubjectIds, (t, s) => new { t, s })
                .GroupBy(i => i.s, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.Select(i => i.t).ToList(), StringComparer.OrdinalIgnoreCase);
        }

        public ReadOnlyCollection<KeyValuePair<int, string>> SlaOptions { get { return _SlaOptions; } }

        public JobQueueToken UpdateQueue(JobQueue JobQueue)
        {
            var token = JobQueueToken.FromJobQueue(JobQueue);
            JobQueueToken existingToken;

            if (_Cache.TryGetValue(JobQueue.Id, out existingToken))
            {
                if (_Cache.TryUpdate(JobQueue.Id, token, existingToken))
                {
                    if (existingToken.JobQueue.SubjectIds != token.JobQueue.SubjectIds)
                        CalculateSubjectCache();

                    return token;
                }
                else
                    return null;
            }
            else
            {
                if (_Cache.TryAdd(JobQueue.Id, token))
                {
                    CalculateSubjectCache();
                    return token;
                }
                else
                    return null;
            }
        }
        public bool RemoveQueue(int JobQueueId)
        {
            JobQueueToken token;
            if (_Cache.TryRemove(JobQueueId, out token))
            {
                CalculateSubjectCache();
                return true;
            }
            else
            {
                return false;
            }
        }
        public JobQueueToken GetQueue(int JobQueueId)
        {
            JobQueueToken token;
            if (_Cache.TryGetValue(JobQueueId, out token))
                return token;
            else
                return null;
        }
        public ReadOnlyCollection<JobQueueToken> GetQueues()
        {
            return _Cache.Values.ToList().AsReadOnly();
        }
        private IEnumerable<JobQueueToken> GetQueuesForSubject(string SubjectId)
        {
            List<JobQueueToken> tokens;
            if (_SubjectCache.TryGetValue(SubjectId, out tokens))
                return tokens;
            else
                return Enumerable.Empty<JobQueueToken>();
        }
        public ReadOnlyCollection<JobQueueToken> GetQueuesForSubject(IEnumerable<string> SubjectIds)
        {
            return SubjectIds.SelectMany(sid => GetQueuesForSubject(sid)).Distinct().ToList().AsReadOnly();
        }

        public void ReInitializeCache(DiscoDataContext Database)
        {
            Initialize(Database);
        }
    }
}
