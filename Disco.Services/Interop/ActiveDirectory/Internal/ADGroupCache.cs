using Disco.Data.Repository;
using Disco.Models.Interop.ActiveDirectory;
using Disco.Services.Tasks;
using Quartz;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Disco.Services.Interop.ActiveDirectory.Internal
{
    public class ADGroupCache : ScheduledTask
    {
        private static ConcurrentDictionary<string, Tuple<ActiveDirectoryGroup, DateTime>> _SecurityIdentifierCache = new ConcurrentDictionary<string, Tuple<ActiveDirectoryGroup, DateTime>>();
        private static ConcurrentDictionary<string, Tuple<ActiveDirectoryGroup, DateTime>> _DistinguishedNameCache = new ConcurrentDictionary<string, Tuple<ActiveDirectoryGroup, DateTime>>();
        private const long CacheTimeoutTicks = 6000000000; // 10 Minutes

        public static IEnumerable<string> GetGroups(IEnumerable<string> DistinguishedNames)
        {
            List<ActiveDirectoryGroup> groups = new List<ActiveDirectoryGroup>();

            foreach (var distinguishedName in DistinguishedNames)
                foreach (var group in GetGroupsRecursive(distinguishedName, new Stack<ActiveDirectoryGroup>()))
                    if (!groups.Contains(group))
                    {
                        groups.Add(group);
                        yield return group.NetBiosId;
                    }
        }
        public static IEnumerable<string> GetGroups(string DistinguishedName)
        {
            foreach (var group in GetGroupsRecursive(DistinguishedName, new Stack<ActiveDirectoryGroup>()))
                yield return group.NetBiosId;
        }
        public static string GetGroupsDistinguishedNameForSecurityIdentifier(string SecurityIdentifier)
        {
            var group = GetGroupBySecurityIdentifier(SecurityIdentifier);
            if (group == null)
                return null;
            else
                return group.DistinguishedName;
        }
        private static IEnumerable<ActiveDirectoryGroup> GetGroupsRecursive(string DistinguishedName, Stack<ActiveDirectoryGroup> RecursiveTree)
        {
            var group = GetGroup(DistinguishedName);

            if (group != null && !RecursiveTree.Contains(group))
            {
                yield return group;

                if (group.MemberOf != null)
                {
                    RecursiveTree.Push(group);

                    foreach (var parentDistinguishedName in group.MemberOf)
                        foreach (var parentGroup in GetGroupsRecursive(parentDistinguishedName, RecursiveTree))
                            yield return parentGroup;

                    RecursiveTree.Pop();
                }
            }
        }

        private static ActiveDirectoryGroup GetGroup(string DistinguishedName)
        {
            // Check Cache
            Tuple<ActiveDirectoryGroup, DateTime> groupRecord = TryCache(DistinguishedName);

            if (groupRecord == null)
            {
                // Load from AD
                var group = ActiveDirectory.RetrieveGroupWithDistinguishedName(DistinguishedName);
                SetValue(group);

                return group;
            }
            else
            {
                // Return from Cache
                return groupRecord.Item1;
            }
        }
        private static ActiveDirectoryGroup GetGroupBySecurityIdentifier(string SecurityIdentifier)
        {
            // Check Cache
            Tuple<ActiveDirectoryGroup, DateTime> groupRecord = TrySecurityIdentifierCache(SecurityIdentifier);

            if (groupRecord == null)
            {
                // Load from AD
                var group = ActiveDirectory.RetrieveGroupWithSecurityIdentifier(SecurityIdentifier);
                SetValue(group);

                return group;
            }
            else
            {
                // Return from Cache
                return groupRecord.Item1;
            }
        }

        private static Tuple<ActiveDirectoryGroup, DateTime> TryCache(string DistinguishedName)
        {
            string distinguishedName = DistinguishedName.ToLower();
            Tuple<ActiveDirectoryGroup, DateTime> groupRecord;
            if (_DistinguishedNameCache.TryGetValue(distinguishedName, out groupRecord))
            {
                if (groupRecord.Item2 > DateTime.Now)
                    return groupRecord;
                else
                {
                    if (_DistinguishedNameCache.TryRemove(distinguishedName, out groupRecord))
                        _SecurityIdentifierCache.TryRemove(groupRecord.Item1.SecurityIdentifier, out groupRecord);
                }
            }
            return null;
        }
        private static Tuple<ActiveDirectoryGroup, DateTime> TrySecurityIdentifierCache(string SecurityIdentifier)
        {
            Tuple<ActiveDirectoryGroup, DateTime> groupRecord;
            if (_SecurityIdentifierCache.TryGetValue(SecurityIdentifier, out groupRecord))
            {
                if (groupRecord.Item2 > DateTime.Now)
                    return groupRecord;
                else
                {
                    if (_SecurityIdentifierCache.TryRemove(SecurityIdentifier, out groupRecord))
                        _DistinguishedNameCache.TryRemove(groupRecord.Item1.DistinguishedName.ToLower(), out groupRecord);
                }
            }
            return null;
        }
        private static bool SetValue(ActiveDirectoryGroup Group)
        {
            Tuple<ActiveDirectoryGroup, DateTime> groupRecord = new Tuple<ActiveDirectoryGroup, DateTime>(Group, DateTime.Now.AddTicks(CacheTimeoutTicks));
            Tuple<ActiveDirectoryGroup, DateTime> oldGroupRecord;

            string key = Group.DistinguishedName.ToLower();
            if (_DistinguishedNameCache.ContainsKey(key))
            {
                if (_DistinguishedNameCache.TryGetValue(key, out oldGroupRecord))
                {
                    _DistinguishedNameCache.TryUpdate(key, groupRecord, oldGroupRecord);
                }
            }
            else
            {
                _DistinguishedNameCache.TryAdd(key, groupRecord);
            }

            string securityIdentifier = Group.SecurityIdentifier;
            if (_SecurityIdentifierCache.ContainsKey(securityIdentifier))
            {
                if (_SecurityIdentifierCache.TryGetValue(securityIdentifier, out oldGroupRecord))
                {
                    _SecurityIdentifierCache.TryUpdate(securityIdentifier, groupRecord, oldGroupRecord);
                }
            }
            else
            {
                _SecurityIdentifierCache.TryAdd(securityIdentifier, groupRecord);
            }
            return true;
        }

        private static void CleanStaleCache()
        {
            // Clean Cache
            var groupKeys = _DistinguishedNameCache.Keys.ToArray();
            foreach (string groupKey in groupKeys)
            {
                Tuple<ActiveDirectoryGroup, DateTime> groupRecord;
                if (_DistinguishedNameCache.TryGetValue(groupKey, out groupRecord))
                {
                    if (groupRecord.Item2 <= DateTime.Now)
                    {
                        _DistinguishedNameCache.TryRemove(groupKey, out groupRecord);
                    }
                }
            }

            // Clean SID Cache
            groupKeys = _SecurityIdentifierCache.Keys.ToArray();
            foreach (string groupKey in groupKeys)
            {
                Tuple<ActiveDirectoryGroup, DateTime> groupRecord;
                if (_SecurityIdentifierCache.TryGetValue(groupKey, out groupRecord))
                {
                    if (groupRecord.Item2 <= DateTime.Now)
                    {
                        _SecurityIdentifierCache.TryRemove(groupKey, out groupRecord);
                    }
                }
            }
        }

        public override string TaskName { get { return "AD Group Cache - Clean Stale Cache"; } }

        public override bool SingleInstanceTask { get { return true; } }
        public override bool CancelInitiallySupported { get { return false; } }
        public override bool LogExceptionsOnly { get { return true; } }

        public override void InitalizeScheduledTask(DiscoDataContext Database)
        {
            // Run @ every 15mins

            // Next 15min interval
            DateTime now = DateTime.Now;
            int mins = (15 - (now.Minute % 15));
            if (mins < 10)
                mins += 15;
            DateTimeOffset startAt = new DateTimeOffset(now).AddMinutes(mins).AddSeconds(now.Second * -1).AddMilliseconds(now.Millisecond * -1);

            TriggerBuilder triggerBuilder = TriggerBuilder.Create().StartAt(startAt).
                WithSchedule(SimpleScheduleBuilder.RepeatMinutelyForever(15));

            this.ScheduleTask(triggerBuilder);
        }

        protected override void ExecuteTask()
        {
            CleanStaleCache();
        }
    }
}
