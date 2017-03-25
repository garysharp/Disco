using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;

namespace Disco.Services.Interop.ActiveDirectory
{
    public class ActiveDirectoryGroupCache
    {
        private ConcurrentDictionary<SecurityIdentifier, Tuple<ADGroup, DateTime>> securityIdentifierCache;
        private ConcurrentDictionary<string, Tuple<ADGroup, DateTime>> distinguishedNameCache;
        private const long CacheTimeoutTicks = 6000000000; // 10 Minutes
        
        private const int CacheCleanIntervalMinutes = 15;
        private DateTime cacheCleanNext;
        private object cacheCleanLock = new object();
        private Task cacheCleanTask;

        public ActiveDirectoryGroupCache()
        {
            securityIdentifierCache = new ConcurrentDictionary<SecurityIdentifier, Tuple<ADGroup, DateTime>>();
            distinguishedNameCache = new ConcurrentDictionary<string, Tuple<ADGroup, DateTime>>(StringComparer.OrdinalIgnoreCase);
            cacheCleanNext = DateTime.Now.AddMinutes(CacheCleanIntervalMinutes);
        }

        public ADGroup GetGroup(string DistinguishedName)
        {
            // Check Cache
            Tuple<ADGroup, DateTime> groupRecord = TryDistinguishedNameCache(DistinguishedName);

            if (groupRecord == null)
            {
                // Load from AD
                var group = ActiveDirectory.RetrieveADGroupByDistinguishedName(DistinguishedName);
                SetValue(group);

                return group;
            }
            else
            {
                // Return from Cache
                return groupRecord.Item1;
            }
        }
        public ADGroup GetGroup(SecurityIdentifier SecurityIdentifier)
        {
            // Check Cache
            Tuple<ADGroup, DateTime> groupRecord = TrySecurityIdentifierCache(SecurityIdentifier);

            if (groupRecord == null)
            {
                // Load from AD
                var group = ActiveDirectory.RetrieveADGroupWithSecurityIdentifier(SecurityIdentifier);
                SetValue(group);

                return group;
            }
            else
            {
                // Return from Cache
                return groupRecord.Item1;
            }
        }

        public IEnumerable<ADGroup> GetRecursiveGroups(IEnumerable<string> DistinguishedNames)
        {
            List<ADGroup> groups = new List<ADGroup>();

            foreach (var distinguishedName in DistinguishedNames)
                foreach (var group in GetGroupsRecursive(distinguishedName, new Stack<ADGroup>()))
                    if (!groups.Contains(group))
                    {
                        groups.Add(group);
                        yield return group;
                    }
        }
        public IEnumerable<ADGroup> GetRecursiveGroups(string DistinguishedName)
        {
            foreach (var group in GetGroupsRecursive(DistinguishedName, new Stack<ADGroup>()))
                yield return group;
        }
        private IEnumerable<ADGroup> GetGroupsRecursive(string DistinguishedName, Stack<ADGroup> RecursiveTree)
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

        private Tuple<ADGroup, DateTime> TryDistinguishedNameCache(string DistinguishedName)
        {
            Tuple<ADGroup, DateTime> groupRecord;
            if (distinguishedNameCache.TryGetValue(DistinguishedName, out groupRecord))
            {
                if (groupRecord.Item2 > DateTime.Now)
                    return groupRecord;
                else
                {
                    if (distinguishedNameCache.TryRemove(DistinguishedName, out groupRecord))
                        securityIdentifierCache.TryRemove(groupRecord.Item1.SecurityIdentifier, out groupRecord);
                }
            }
            return null;
        }
        private Tuple<ADGroup, DateTime> TrySecurityIdentifierCache(SecurityIdentifier SecurityIdentifier)
        {
            Tuple<ADGroup, DateTime> groupRecord;
            if (securityIdentifierCache.TryGetValue(SecurityIdentifier, out groupRecord))
            {
                if (groupRecord.Item2 > DateTime.Now)
                    return groupRecord;
                else
                {
                    if (securityIdentifierCache.TryRemove(SecurityIdentifier, out groupRecord))
                        distinguishedNameCache.TryRemove(groupRecord.Item1.DistinguishedName, out groupRecord);
                }
            }
            return null;
        }

        private bool SetValue(ADGroup Group)
        {
            Tuple<ADGroup, DateTime> groupRecord = Tuple.Create(Group, DateTime.Now.AddTicks(CacheTimeoutTicks));
            Tuple<ADGroup, DateTime> oldGroupRecord;

            var distinguishedName = Group.DistinguishedName;
            var securityIdentifier = Group.SecurityIdentifier;

            if (distinguishedNameCache.ContainsKey(distinguishedName))
            {
                if (distinguishedNameCache.TryGetValue(distinguishedName, out oldGroupRecord))
                {
                    distinguishedNameCache.TryUpdate(distinguishedName, groupRecord, oldGroupRecord);
                }
            }
            else
            {
                distinguishedNameCache.TryAdd(distinguishedName, groupRecord);
            }

            if (securityIdentifierCache.ContainsKey(securityIdentifier))
            {
                if (securityIdentifierCache.TryGetValue(securityIdentifier, out oldGroupRecord))
                {
                    securityIdentifierCache.TryUpdate(securityIdentifier, groupRecord, oldGroupRecord);
                }
            }
            else
            {
                securityIdentifierCache.TryAdd(securityIdentifier, groupRecord);
            }
            return true;
        }

        #region Stale Cache Clean

        private void EnsureCleanCache()
        {
            if (cacheCleanTask == null && cacheCleanNext < DateTime.Now)
            {
                lock (cacheCleanLock)
                {
                    if (cacheCleanTask == null && cacheCleanNext < DateTime.Now)
                    {
                        cacheCleanTask = Task.Factory.StartNew(CleanCache);
                    }
                }
            }
        }

        private void CleanCache()
        {
            DateTime now = DateTime.Now;

            // Clean Cache
            var dnKeys = distinguishedNameCache.Keys.ToArray();
            foreach (var dnKey in dnKeys)
            {
                Tuple<ADGroup, DateTime> groupRecord;
                if (distinguishedNameCache.TryGetValue(dnKey, out groupRecord))
                {
                    if (groupRecord.Item2 <= now)
                    {
                        distinguishedNameCache.TryRemove(dnKey, out groupRecord);
                    }
                }
            }

            // Clean SID Cache
            var siKeys = securityIdentifierCache.Keys.ToArray();
            foreach (var siKey in siKeys)
            {
                Tuple<ADGroup, DateTime> groupRecord;
                if (securityIdentifierCache.TryGetValue(siKey, out groupRecord))
                {
                    if (groupRecord.Item2 <= now)
                    {
                        securityIdentifierCache.TryRemove(siKey, out groupRecord);
                    }
                }
            }

            // Schedule Next Clean
            cacheCleanNext = now.AddMinutes(CacheCleanIntervalMinutes);
            cacheCleanTask = null;
        }

        #endregion
    }
}
