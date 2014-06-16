using Disco.Data.Repository;
using Disco.Models.Services.Interop.ActiveDirectory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Disco.Services.Interop.ActiveDirectory
{
    public abstract class ADManagedGroup : IDisposable
    {
        public string Key { get; private set; }
        public ADManagedGroupConfiguration Configuration { get; private set; }

        internal ActiveDirectoryManagedGroups Context { get; set; }

        public abstract string Description { get; }
        public abstract string CategoryDescription { get; }
        public abstract string GroupDescription { get; }
        public abstract bool IncludeFilterBeginDate { get; }

        public ADManagedGroup(string Key, ADManagedGroupConfiguration Configuration)
        {
            if (string.IsNullOrWhiteSpace(Key))
                throw new ArgumentNullException("Key");
            if (Configuration == null)
                throw new ArgumentNullException("Configuration");
            if (!ActiveDirectory.IsValidDomainAccountId(Configuration.GroupId))
                throw new ArgumentException("Configuration.GroupId is not a valid Domain Account Id", "Configuration");

            this.Key = Key;
            this.Configuration = Configuration;
        }

        public abstract void Initialize();
        public abstract IEnumerable<string> DetermineMembers(DiscoDataContext Database);

        public ADGroup GetGroup()
        {
            return ActiveDirectory.RetrieveADGroup(this.Configuration.GroupId, "member");
        }

        protected void AddMember(string Id)
        {
            AddMember(Id, null);
        }
        protected void AddMember(string InvokingIdentifier, Func<DiscoDataContext, IEnumerable<string>> MemberResolver)
        {
            if (Context == null)
                return; // Must be added to ActiveDirectoryManagedGroups

            var action = new ADManagedGroupScheduledAction(
                this,
                ADManagedGroupScheduledActionType.AddGroupMember,
                InvokingIdentifier,
                MemberResolver);

            Context.ScheduleAction(action);
        }

        protected void RemoveMember(string Id)
        {
            RemoveMember(Id, null);
        }
        protected void RemoveMember(string InvokingIdentifier, Func<DiscoDataContext, IEnumerable<string>> MemberResolver)
        {
            if (Context == null)
                return; // Must be added to ActiveDirectoryManagedGroups

            var action = new ADManagedGroupScheduledAction(
                this,
                ADManagedGroupScheduledActionType.RemoveGroupMember,
                InvokingIdentifier,
                MemberResolver);

            Context.ScheduleAction(action);
        }

        public static ADManagedGroupConfiguration ConfigurationFromJson(string ConfigurationJson)
        {
            return JsonConvert.DeserializeObject<ADManagedGroupConfiguration>(ConfigurationJson);
        }
        public static string ValidConfigurationToJson(string GroupKey, string GroupId, DateTime? FilterBeginDate)
        {
            if (string.IsNullOrWhiteSpace(GroupId))
                GroupId = null;

            if (GroupId != null)
                GroupId = ActiveDirectory.Context.ManagedGroups.ValidateGroupId(GroupId, GroupKey);

            if (GroupId == null)
                return null;
            else
                return JsonConvert.SerializeObject(new ADManagedGroupConfiguration()
                {
                    GroupId = GroupId,
                    FilterBeginDate = FilterBeginDate
                }, new JsonSerializerSettings() { DefaultValueHandling = DefaultValueHandling.Ignore });
        }

        public abstract void Dispose();
    }
}
