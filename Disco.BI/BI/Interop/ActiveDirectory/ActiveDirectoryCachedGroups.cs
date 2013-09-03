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
        private static ConcurrentDictionary<string, Tuple<ADCachedGroup, DateTime>> _SidCache = new ConcurrentDictionary<string, Tuple<ADCachedGroup, DateTime>>();
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
        public static string GetGroupsCnForSid(string GroupSid)
        {
            var sidGroup = GetGroupBySid(GroupSid);
            if (sidGroup == null)
                return null;
            else
                return sidGroup.CN;
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
                var group = ADCachedGroup.LoadWithCN(GroupCN);
                SetValue(group);

                return group;
            }
            else
            {
                // Return from Cache
                return groupRecord.Item1;
            }
        }
        private static ADCachedGroup GetGroupBySid(string GroupSid)
        {
            // Check Cache
            Tuple<ADCachedGroup, DateTime> groupRecord = TrySidCache(GroupSid);

            if (groupRecord == null)
            {
                // Load from AD
                var group = ADCachedGroup.LoadWithSid(GroupSid);
                SetValue(group);

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
                {
                    if (_Cache.TryRemove(groupCN, out groupRecord))
                        _SidCache.TryRemove(groupRecord.Item1.ObjectSid, out groupRecord);
                }
            }
            return null;
        }
        private static Tuple<ADCachedGroup, DateTime> TrySidCache(string GroupSid)
        {
            Tuple<ADCachedGroup, DateTime> groupRecord;
            if (_SidCache.TryGetValue(GroupSid, out groupRecord))
            {
                if (groupRecord.Item2 > DateTime.Now)
                    return groupRecord;
                else
                {
                    if (_SidCache.TryRemove(GroupSid, out groupRecord))
                        _Cache.TryRemove(groupRecord.Item1.CN.ToLower(), out groupRecord);
                }
            }
            return null;
        }
        private static bool SetValue(ADCachedGroup GroupRecord)
        {
            Tuple<ADCachedGroup, DateTime> groupRecord = new Tuple<ADCachedGroup, DateTime>(GroupRecord, DateTime.Now.AddTicks(CacheTimeoutTicks));
            Tuple<ADCachedGroup, DateTime> oldGroupRecord;

            string key = GroupRecord.CN.ToLower();
            if (_Cache.ContainsKey(key))
            {
                if (_Cache.TryGetValue(key, out oldGroupRecord))
                {
                    _Cache.TryUpdate(key, groupRecord, oldGroupRecord);
                }
            }
            else
            {
                _Cache.TryAdd(key, groupRecord);
            }

            string sid = GroupRecord.ObjectSid;
            if (_SidCache.ContainsKey(sid))
            {
                if (_SidCache.TryGetValue(sid, out oldGroupRecord))
                {
                    _SidCache.TryUpdate(sid, groupRecord, oldGroupRecord);
                }
            }
            else
            {
                _SidCache.TryAdd(sid, groupRecord);
            }
            return true;
        }

        private class ADCachedGroup
        {
            public string ObjectSid { get; set; }
            public string CN { get; private set; }
            public string FriendlyName { get; private set; }

            public List<string> MemberOf { get; private set; }

            public static ADCachedGroup LoadWithCN(string CN)
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

                        group.ObjectSid = ActiveDirectoryHelpers.ConvertBytesToSDDLString((byte[])groupDE.Properties["objectSid"].Value);
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

            public static ADCachedGroup LoadWithSid(string Sid)
            {
                using (DirectoryEntry dRootEntry = ActiveDirectoryHelpers.DefaultLdapRoot)
                {
                    var loadProperties = new List<string> {
					"distinguishedName", 
					"objectSid", 
                    "sAMAccountName",
					"memberOf"
                };

                    var sidBytes = ActiveDirectoryHelpers.ConvertSDDLStringToBytes(Sid);
                    var sidBinaryString = ActiveDirectoryHelpers.ConvertBytesToBinarySidString(sidBytes);

                    using (DirectorySearcher dSearcher = new DirectorySearcher(dRootEntry, string.Format("(&(objectClass=group)(objectSid={0}))", sidBinaryString), loadProperties.ToArray(), SearchScope.Subtree))
                    {
                        SearchResult dResult = dSearcher.FindOne();
                        if (dResult != null)
                        {
                            var group = new ADCachedGroup()
                            {
                                CN = (string)dResult.Properties["distinguishedName"][0],
                                ObjectSid = ActiveDirectoryHelpers.ConvertBytesToSDDLString((byte[])dResult.Properties["objectSid"][0]),
                                FriendlyName = (string)dResult.Properties["sAMAccountName"][0]
                            };

                            var groupMemberOf = dResult.Properties["memberOf"];
                            if (groupMemberOf != null && groupMemberOf.Count > 0)
                            {
                                group.MemberOf = groupMemberOf.Cast<string>().ToList();
                            }

                            return group;
                        }
                        else
                            return null;
                    }
                }
            }

            private ADCachedGroup()
            {
                // Private Constructor
            }
        }

        private static void CleanStaleCache()
        {
            // Clean Cache
            var groupKeys = _Cache.Keys.ToArray();
            foreach (string groupKey in groupKeys)
            {
                Tuple<ADCachedGroup, DateTime> groupRecord;
                if (_Cache.TryGetValue(groupKey, out groupRecord))
                {
                    if (groupRecord.Item2 <= DateTime.Now)
                    {
                        _Cache.TryRemove(groupKey, out groupRecord);
                    }
                }
            }

            // Clean SID Cache
            groupKeys = _SidCache.Keys.ToArray();
            foreach (string groupKey in groupKeys)
            {
                Tuple<ADCachedGroup, DateTime> groupRecord;
                if (_SidCache.TryGetValue(groupKey, out groupRecord))
                {
                    if (groupRecord.Item2 <= DateTime.Now)
                    {
                        _SidCache.TryRemove(groupKey, out groupRecord);
                    }
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
