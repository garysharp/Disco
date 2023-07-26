using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using Disco.Data.Repository;

namespace Disco.Services.Interop.ActiveDirectory
{
    using Disco.Services.Logging;
    using Disco.Services.Tasks;
    using ScheduledActionItemGrouping = List<Tuple<ADManagedGroup, List<ADManagedGroupScheduledActionItem>>>;

    public class ActiveDirectoryManagedGroups : IDisposable
    {
        private ConcurrentDictionary<string, ADManagedGroup> managedGroups;
        private Subject<ADManagedGroupScheduledAction> actionBuffer;
        private IDisposable actionBufferSubscription;

        internal ActiveDirectoryManagedGroups()
        {
            managedGroups = new ConcurrentDictionary<string, ADManagedGroup>();
            actionBuffer = new Subject<ADManagedGroupScheduledAction>();

            // Subscribe, wait for no additional actions after 10 seconds
            actionBufferSubscription = actionBuffer
                .BufferWithInactivity(TimeSpan.FromSeconds(10))
                .Subscribe(ParseScheduledActions);
        }

        #region Collection Methods
        public void AddOrUpdate(ADManagedGroup ManagedGroup)
        {
            ManagedGroup.Context = this;
            ManagedGroup.Initialize();

            string key = ManagedGroup.Key;

            var existingGroup = managedGroups.Values
                .Where(g => g.Key != ManagedGroup.Key)
                .FirstOrDefault(g => g.Configuration.GroupId.Equals(ManagedGroup.Configuration.GroupId, StringComparison.OrdinalIgnoreCase));

            if (existingGroup != null)
                throw new ArgumentException(string.Format("[{0}] cannot manage this group [{1}] because is already managed by [{2}]", ManagedGroup.Key, ManagedGroup.Configuration.GroupId, existingGroup.Key), "ManagedGroup");

            managedGroups.AddOrUpdate(key, ManagedGroup, (itemKey, item) =>
            {
                item.Dispose();
                return ManagedGroup;
            });
        }
        public bool Remove(string Key)
        {
            ADManagedGroup item;

            if (managedGroups.TryRemove(Key, out item))
            {
                item.Dispose();
                return true;
            }

            return false;
        }
        public bool TryGetValue(string Key, out ADManagedGroup ManagedGroup)
        {
            return managedGroups.TryGetValue(Key, out ManagedGroup);
        }
        public List<ADManagedGroup> Values
        {
            get
            {
                return managedGroups.Values.ToList();
            }
        }
        #endregion

        public string ValidateGroupId(string GroupId, string IgnoreManagedGroupKey)
        {
            var group = ActiveDirectory.RetrieveADGroup(GroupId, "isCriticalSystemObject");
            if (group == null)
                throw new ArgumentException(string.Format("The group [{0}] wasn't found", GroupId), "DevicesLinkedGroup");
            if (group.GetPropertyValue<bool>("isCriticalSystemObject"))
                throw new ArgumentException(string.Format("The group [{0}] is a Critical System Active Directory Object and Disco ICT refuses to modify it", group.DistinguishedName), "DevicesLinkedGroup");

            GroupId = group.Id;

            var otherManagedGroup = ActiveDirectory.Context.ManagedGroups.Values
                .Where(g => g.Key != IgnoreManagedGroupKey)
                .FirstOrDefault(g => g.Configuration.GroupId.Equals(GroupId, StringComparison.OrdinalIgnoreCase));
            if (otherManagedGroup != null)
                throw new ArgumentException(string.Format("Cannot manage this group [{0}] because is already managed by [{1}]", GroupId, otherManagedGroup.Key), "DevicesLinkedGroup");

            return GroupId;
        }

        internal void ScheduleAction(ADManagedGroupScheduledAction ScheduledAction)
        {
            actionBuffer.OnNext(ScheduledAction);
        }

        private void ParseScheduledActions(IEnumerable<ADManagedGroupScheduledAction> Actions)
        {
            ScheduledActionItemGrouping groupedActionItems;

            using (DiscoDataContext Database = new DiscoDataContext())
            {
                groupedActionItems = Actions
                .GroupBy(a => a.ManagedGroup)
                .Where(g =>
                {
                    ADManagedGroup item;
                    if (managedGroups.TryGetValue(g.Key.Key, out item))
                        return item == g.Key;
                    else
                        return false;
                })
                .Select(g => // Reduce actions to last instance of ActionSubjectId
                    Tuple.Create(
                        g.Key,
                        g.GroupBy(i => i.InvokingIdentifier, (id, idg) => idg.Last())
                    )
                ).Select(g => // Resolve action group members (action subjects)
                    Tuple.Create(g.Item1, g.Item2.SelectMany(i => i.ResolveMembers(Database)))
                ).Select(g => // Reduce actions to last instance of MemberId
                    Tuple.Create(
                        g.Item1,
                        g.Item2.GroupBy(i => i.MemberId, (id, idg) => idg.Last()).ToList()
                    )
                ).ToList();
            }

            ApplyScheduledActionItems(groupedActionItems);
        }

        private void ApplyScheduledActionItems(ScheduledActionItemGrouping ActionGroups)
        {
            var actionsCount = ActionGroups.SelectMany(a => a.Item2).Count();
            if (actionsCount > 0)
            {
                var adSearchLoadProperties = new string[] { "distinguishedName", "sAMAccountName" };
                var accountDNCache = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                if (actionsCount > 40)
                {
                    // Potentially over 40 accounts, cache all scoped
                    var scopeAccounts = ActiveDirectory.Context.SearchScope("(|(objectCategory=computer)(objectCategory=person))", adSearchLoadProperties);
                    foreach (var scopeAccount in scopeAccounts)
                    {
                        var id = string.Format(@"{0}\{1}", scopeAccount.Domain.NetBiosName, scopeAccount.Value<string>("sAMAccountName"));
                        accountDNCache[id] = scopeAccount.Value<string>("distinguishedName");
                    }
                }

                foreach (var actionGroup in ActionGroups)
                {
                    // Resolve Member Ids to AD Distinguished Names
                    //   Discard non-existent users
                    var actionItems = actionGroup.Item2.Select(a =>
                    {
                        string distinguishedName;
                        if (!accountDNCache.TryGetValue(a.MemberId, out distinguishedName))
                        {
                            string memberUsername;
                            ADDomain memberDomain;
                            if (!ActiveDirectory.IsValidDomainAccountId(a.MemberId, out memberUsername, out memberDomain))
                            {
                                accountDNCache[a.MemberId] = null; // Add to cache (avoid retries)
                                return null;
                            }

                            var ldapFilter = string.Format("(&(|(objectCategory=computer)(objectCategory=person))(sAMAccountName={0}))", memberUsername);

                            var adSearchResult = memberDomain.SearchEntireDomain(ldapFilter, adSearchLoadProperties, ActiveDirectory.SingleSearchResult).FirstOrDefault();
                            if (adSearchResult != null)
                            {
                                var adSearchResultDN = adSearchResult.Value<string>("distinguishedName");
                                accountDNCache[a.MemberId] = adSearchResultDN; // Add to cache
                                a.MemberDistinguishedName = adSearchResultDN; // Update ActionItem
                                return a;
                            }
                            else
                            {
                                accountDNCache[a.MemberId] = null; // Add to cache (avoid retries)
                                return null;
                            }
                        }
                        else if (distinguishedName == null)
                            return null;
                        else
                        {
                            a.MemberDistinguishedName = distinguishedName; // Update ActionItem
                            return a;
                        }
                    }).Where(a => a != null).ToList();

                    if (actionItems.Count > 0)
                    {
                        var adGroup = actionGroup.Item1.GetGroup();
                        if (adGroup == null)
                        {
                            SystemLog.LogWarning("Active Directory Managed Group", actionGroup.Item1.Key, "Group Not Found", actionGroup.Item1.Configuration.GroupId);
                            break;
                        }
                        var adGroupMembers = adGroup.GetPropertyValues<string>("member").ToList();
                        actionItems = actionItems.Where(a =>
                        {
                            switch (a.ActionType)
                            {
                                case ADManagedGroupScheduledActionType.AddGroupMember:
                                    return !adGroupMembers.Contains(a.MemberDistinguishedName);
                                case ADManagedGroupScheduledActionType.RemoveGroupMember:
                                    return adGroupMembers.Contains(a.MemberDistinguishedName);
                                default:
                                    return false;
                            }
                        }).ToList();

                        if (actionItems.Count > 0)
                        {
                            using (var adGroupEntry = ActiveDirectory.Context.RetrieveDirectoryEntry(adGroup.DistinguishedName, new string[] { "member", "isCriticalSystemObject" }))
                            {
                                if (adGroupEntry.Entry.Properties.Value<bool>("isCriticalSystemObject"))
                                    throw new InvalidOperationException(string.Format("This group [{0}] is a Critical System Active Directory Object and Disco ICT refuses to modify it", adGroup.DistinguishedName));

                                var adGroupEntryMembers = adGroupEntry.Entry.Properties["member"];
                                foreach (var item in actionItems)
                                {
                                    switch (item.ActionType)
                                    {
                                        case ADManagedGroupScheduledActionType.AddGroupMember:
                                            if (!adGroupEntryMembers.Contains(item.MemberDistinguishedName))
                                            {
                                                // Add Member Entry
                                                adGroupEntryMembers.Add(item.MemberDistinguishedName);
                                            }
                                            break;
                                        case ADManagedGroupScheduledActionType.RemoveGroupMember:
                                            if (adGroupEntryMembers.Contains(item.MemberDistinguishedName))
                                            {
                                                // Add Member Entry
                                                adGroupEntryMembers.Remove(item.MemberDistinguishedName);
                                            }
                                            break;
                                    }
                                }

                                // Commit Changes
                                adGroupEntry.Entry.CommitChanges();
                            }
                        }
                    }
                }
            }
        }

        public int SyncManagedGroups(IScheduledTaskStatus Status)
        {
            return SyncManagedGroups(managedGroups.Values, Status);
        }

        public int SyncManagedGroups(ADManagedGroup ManagedGroup, IScheduledTaskStatus Status)
        {
            return SyncManagedGroups(new ADManagedGroup[] { ManagedGroup }, Status);
        }

        public int SyncManagedGroups(IEnumerable<ADManagedGroup> ManagedGroups, IScheduledTaskStatus Status)
        {
            List<ADManagedGroup> managedGroups = ManagedGroups.ToList();
            ScheduledActionItemGrouping actionGroups;
            int changeCount = 0;

            Status.UpdateStatus(0, "Determining Managed Group Members");

            using (DiscoDataContext Database = new DiscoDataContext())
            {
                actionGroups = managedGroups.Select((g, index) =>
                {
                    Status.UpdateStatus(
                        ((double)30 / managedGroups.Count) * index, // 0 -> 30
                        string.Format("Determining Group Members: {0} [{1}]", g.GroupDescription, g.Configuration.GroupId));
                    return Tuple.Create(
                        g,
                        g.DetermineMembers(Database).Select(m =>
                            new ADManagedGroupScheduledActionItem(
                                g,
                                ADManagedGroupScheduledActionType.AddGroupMember,
                                m
                            )).ToList());
                }).ToList();
            }

            var actionsCount = actionGroups.SelectMany(a => a.Item2).Count();
            if (actionsCount > 0)
            {
                Status.UpdateStatus(30, "Resolving Group Members");

                var adSearchLoadProperties = new string[] { "distinguishedName", "sAMAccountName", "displayName", "name" };
                var accountDNCache = new Dictionary<string, Tuple<string, string>>(StringComparer.OrdinalIgnoreCase);
                if (actionsCount > 40)
                {
                    // Potentially over 40 accounts, cache all scoped
                    var scopeAccounts = ActiveDirectory.Context.SearchScope("(|(objectCategory=computer)(objectCategory=person))", adSearchLoadProperties);
                    foreach (var scopeAccount in scopeAccounts)
                    {
                        var id = string.Format(@"{0}\{1}", scopeAccount.Domain.NetBiosName, scopeAccount.Value<string>("sAMAccountName"));
                        accountDNCache[id] = Tuple.Create(scopeAccount.Value<string>("distinguishedName"), scopeAccount.Value<string>("displayName") ?? scopeAccount.Value<string>("name"));
                    }
                }

                actionGroups = actionGroups.Select((g, index) =>
                {
                    Status.UpdateStatus(
                        30 + (((double)30 / actionGroups.Count) * index), // 30 -> 60
                        string.Format("Resolving {0} Group Members: {1} [{2}]", g.Item2.Count, g.Item1.GroupDescription, g.Item1.Configuration.GroupId));

                    // Resolve Member Ids to AD Distinguished Names
                    //   Discard non-existent users
                    return Tuple.Create(
                        g.Item1,
                        g.Item2.Select(a =>
                            {
                                Tuple<string, string> definition;
                                if (!accountDNCache.TryGetValue(a.MemberId, out definition))
                                {
                                    string memberUsername;
                                    ADDomain memberDomain;
                                    if (!ActiveDirectory.IsValidDomainAccountId(a.MemberId, out memberUsername, out memberDomain))
                                    {
                                        accountDNCache[a.MemberId] = null; // Add to cache (avoid retries)
                                        return null;
                                    }

                                    var ldapFilter = string.Format("(&(|(objectCategory=computer)(objectCategory=person))(sAMAccountName={0}))", memberUsername);

                                    var adSearchResult = memberDomain.SearchEntireDomain(ldapFilter, adSearchLoadProperties, ActiveDirectory.SingleSearchResult).FirstOrDefault();
                                    if (adSearchResult != null)
                                    {
                                        definition = Tuple.Create(adSearchResult.Value<string>("distinguishedName"), adSearchResult.Value<string>("displayName") ?? adSearchResult.Value<string>("name"));
                                        accountDNCache[a.MemberId] = definition; // Add to cache
                                    }
                                    else
                                    {
                                        accountDNCache[a.MemberId] = null; // Add to cache (avoid retries)
                                        return null;
                                    }
                                }
                                else if (definition == null)
                                    return null;

                                a.MemberDistinguishedName = definition.Item1; // Update ActionItem
                                a.MemberDisplayName = definition.Item2;
                                return a;
                            }).Where(a => a != null).ToList());
                }).ToList();
            }

            foreach (var actionGroup in actionGroups)
            {
                var adGroup = actionGroup.Item1.GetGroup();
                if (adGroup == null)
                {
                    SystemLog.LogWarning("Active Directory Managed Group", actionGroup.Item1.Key, "Group Not Found", actionGroup.Item1.Configuration.GroupId);
                    break;
                }

                Status.UpdateStatus(
                        60 + (((double)40 / actionGroups.Count) * actionGroups.IndexOf(actionGroup)), // 60 -> 100
                        string.Format("Synchronizing {0} Group Members: {1} [{2}]", actionGroup.Item2.Count, actionGroup.Item1.GroupDescription, actionGroup.Item1.Configuration.GroupId));

                using (var adGroupEntry = ActiveDirectory.Context.RetrieveDirectoryEntry(adGroup.DistinguishedName, new string[] { "isCriticalSystemObject", "description", "member" }))
                {
                    if (adGroupEntry.Entry.Properties.Value<bool>("isCriticalSystemObject"))
                        throw new InvalidOperationException(string.Format("This group [{0}] is a Critical System Active Directory Object and Disco ICT refuses to modify it", adGroup.DistinguishedName));

                    // Update Description
                    var groupDescription = string.Format("Disco ICT: {0}", actionGroup.Item1.GroupDescription);
                    if (adGroupEntry.Entry.Properties.Value<string>("description") != groupDescription)
                    {
                        var adGroupEntryDescription = adGroupEntry.Entry.Properties["description"];
                        if (adGroupEntryDescription.Count > 0)
                            adGroupEntryDescription.Clear();
                        adGroupEntryDescription.Add(groupDescription);
                    }

                    // Sync Members
                    var adGroupEntryMembers = adGroupEntry.Entry.Properties["member"];

                    // Remove Items
                    var removeItems = adGroupEntryMembers
                        .Cast<string>()
                        .Except(actionGroup.Item2.Select(i => i.MemberDistinguishedName))
                        .ToList();
                    removeItems.ForEach(i => adGroupEntryMembers.Remove(i));

                    // Add Items
                    var addItems = actionGroup.
                        Item2.Select(i => i.MemberDistinguishedName)
                        .Except(adGroupEntryMembers.Cast<string>())
                        .ToList();
                    addItems.ForEach(i => adGroupEntryMembers.Add(i));

                    // Commit Changes
                    adGroupEntry.Entry.CommitChanges();

                    changeCount += removeItems.Count;
                    changeCount += addItems.Count;
                }
            }

            Status.UpdateStatus(100, "Managed Group Synchronization Finished");

            return changeCount;
        }

        public void Dispose()
        {
            if (actionBufferSubscription != null)
                actionBufferSubscription.Dispose();

            if (actionBuffer != null)
                actionBuffer.Dispose();
        }
    }

    internal class ADManagedGroupScheduledAction
    {
        private Func<DiscoDataContext, IEnumerable<string>> memberResolver;

        public ADManagedGroup ManagedGroup { get; private set; }
        public ADManagedGroupScheduledActionType ActionType { get; private set; }
        public string InvokingIdentifier { get; set; }

        public ADManagedGroupScheduledAction(ADManagedGroup ManagedGroup, ADManagedGroupScheduledActionType ActionType, string InvokingIdentifier, Func<DiscoDataContext, IEnumerable<string>> MemberResolver)
        {
            this.ManagedGroup = ManagedGroup;
            this.ActionType = ActionType;
            this.InvokingIdentifier = InvokingIdentifier;
            memberResolver = MemberResolver;
        }

        public IEnumerable<ADManagedGroupScheduledActionItem> ResolveMembers(DiscoDataContext Database)
        {
            if (memberResolver != null)
            {
                var members = memberResolver(Database);
                if (members == null)
                    return Enumerable.Empty<ADManagedGroupScheduledActionItem>();
                else
                    return members.Select(m =>
                        new ADManagedGroupScheduledActionItem(ManagedGroup, ActionType, m)
                    );
            }
            else
            {
                return new ADManagedGroupScheduledActionItem[]
                    {
                        new ADManagedGroupScheduledActionItem(ManagedGroup, ActionType, InvokingIdentifier) 
                    };
            }
        }
    }
    internal class ADManagedGroupScheduledActionItem
    {
        public ADManagedGroup ManagedGroup { get; private set; }
        public ADManagedGroupScheduledActionType ActionType { get; private set; }
        public string MemberId { get; set; }
        public string MemberDistinguishedName { get; set; }
        public string MemberDisplayName { get; set; }

        public ADManagedGroupScheduledActionItem(ADManagedGroup ManagedGroup, ADManagedGroupScheduledActionType ActionType, string MemberId)
        {
            this.ManagedGroup = ManagedGroup;
            this.ActionType = ActionType;
            this.MemberId = MemberId;
        }
    }
    internal enum ADManagedGroupScheduledActionType
    {
        AddGroupMember,
        RemoveGroupMember
    }
}
