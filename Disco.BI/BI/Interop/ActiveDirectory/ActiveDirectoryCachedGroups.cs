using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Disco.Data.Repository;
using Disco.Services.Tasks;
using Quartz;

namespace Disco.BI.Interop.ActiveDirectory
{
    public class ActiveDirectoryCachedGroups : ScheduledTask
    {
        private static ConcurrentDictionary<string, Tuple<ADCachedGroup, DateTime>> _Cache = new ConcurrentDictionary<string, Tuple<ADCachedGroup, DateTime>>();
        private const long CacheTimeoutTicks = 6000000000; // 10 Minutes

        public static IEnumerable<string> GetGroups(IEnumerable<string> GroupCNs)
        {
            List<ADCachedGroup> groups = new List<ADCachedGroup>();

            foreach (var groupCN in GroupCNs)
                foreach (var group in GetGroupsRecursive(groupCN, new Stack<ADCachedGroup>()))
                    if (!groups.Contains(group))
                    {
                        groups.Add(group);
                        yield return group.FriendlyName;
                    }
        }
        public static IEnumerable<string> GetGroups(string GroupCN)
        {
            foreach (var group in GetGroupsRecursive(GroupCN, new Stack<ADCachedGroup>()))
                yield return group.FriendlyName;
        }
        private static IEnumerable<ADCachedGroup> GetGroupsRecursive(string GroupCN, Stack<ADCachedGroup> RecursiveTree)
        {
            var group = GetGroup(GroupCN);

            if (group != null && !RecursiveTree.Contains(group))
            {
                yield return group;

                if (group.MemberOf != null)
                {
                    RecursiveTree.Push(group);

                    foreach (var memberOfGroupCN in group.MemberOf)
                        foreach (var memberOfGroup in GetGroupsRecursive(memberOfGroupCN, RecursiveTree))
                            yield return memberOfGroup;

                    RecursiveTree.Pop();
                }
            }
        }

        private static ADCachedGroup GetGroup(string GroupCN)
        {
            // Check Cache
            Tuple<ADCachedGroup, DateTime> groupRecord = TryCache(GroupCN);

            if (groupRecord == null)
            {
                // Load from AD
                var group = ADCachedGroup.LoadFromAD(GroupCN);
                SetValue(GroupCN, group);

                return group;
            }
            else
            {
                // Return from Cache
                return groupRecord.Item1;
            }
        }

        private static Tuple<ADCachedGroup, DateTime> TryCache(string GroupCN)
        {
            string groupCN = GroupCN.ToLower();
            Tuple<ADCachedGroup, DateTime> groupRecord;
            if (_Cache.TryGetValue(groupCN, out groupRecord))
            {
                if (groupRecord.Item2 > DateTime.Now)
                    return groupRecord;
                else
                    _Cache.TryRemove(groupCN, out groupRecord);
            }
            return null;
        }
        private static bool SetValue(string GroupCN, ADCachedGroup GroupRecord)
        {
            string key = GroupCN.ToLower();
            Tuple<ADCachedGroup, DateTime> groupRecord = new Tuple<ADCachedGroup, DateTime>(GroupRecord, DateTime.Now.AddTicks(CacheTimeoutTicks));
            if (_Cache.ContainsKey(key))
            {
                Tuple<ADCachedGroup, DateTime> oldGroupRecord;
                if (_Cache.TryGetValue(key, out oldGroupRecord))
                {
                    return _Cache.TryUpdate(key, groupRecord, oldGroupRecord);
                }
            }
            return _Cache.TryAdd(key, groupRecord);
        }

        private class ADCachedGroup
        {
            public string CN { get; private set; }
            public string FriendlyName { get; private set; }

            public List<string> MemberOf { get; private set; }

            public static ADCachedGroup LoadFromAD(string CN)
            {
                ADCachedGroup group = null;

                using (DirectoryEntry groupDE = new DirectoryEntry(string.Concat(ActiveDirectoryHelpers.DefaultLdapPath, CN)))
                {
                    if (groupDE != null)
                    {
                        group = new ADCachedGroup()
                        {
                            CN = CN
                        };

                        group.FriendlyName = (string)groupDE.Properties["sAMAccountName"].Value;

                        var groupMemberOf = groupDE.Properties["memberOf"];
                        if (groupMemberOf != null && groupMemberOf.Count > 0)
                        {
                            group.MemberOf = groupMemberOf.Cast<string>().ToList();
                        }
                    }
                }

                return group;
            }

            private ADCachedGroup()
            {
                // Private Constructor
            }
        }

        private static void CleanStaleCache()
        {
            var groupKeys = _Cache.Keys.ToArray();
            foreach (string groupKey in groupKeys)
            {
                Tuple<ADCachedGroup, DateTime> groupRecord;
                if (_Cache.TryGetValue(groupKey, out groupRecord))
                {
                    if (groupRecord.Item2 <= DateTime.Now)
                        _Cache.TryRemove(groupKey, out groupRecord);
                }
            }
        }

        public override string TaskName { get { return "AD Group Cache - Clean Stale Cache"; } }

        public override bool SingleInstanceTask { get { return true; } }
        public override bool CancelInitiallySupported { get { return false; } }
        public override bool LogExceptionsOnly { get { return true; } }

        public override void InitalizeScheduledTask(DiscoDataContext dbContext)
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
