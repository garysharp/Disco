using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Security.Principal;

namespace Disco.Services.Interop.ActiveDirectory
{
    public class ADGroup : IADObject
    {
        internal static readonly string[] LoadProperties = { "name", "distinguishedName", "sAMAccountName", "objectSid", "memberOf" };
        internal const string LdapSearchFilterTemplate = "(&(objectCategory=Group)(|(sAMAccountName=*{0}*)(name=*{0}*)(cn=*{0}*)))";
        internal const string LdapSamAccountNameFilterTemplate = "(&(objectCategory=Group)(sAMAccountName={0}))";
        internal const string LdapSecurityIdentifierFilterTemplate = "(&(objectCategory=Group)(objectSid={0}))";

        public ADDomain Domain { get; private set; }

        public string DistinguishedName { get; private set; }
        public SecurityIdentifier SecurityIdentifier { get; private set; }
        
        public string Id { get { return string.Format(@"{0}\{1}", Domain.NetBiosName, SamAccountName); } }
        public string SamAccountName { get; private set; }

        public string Name { get; private set; }
        public string DisplayName { get { return this.Name; } }

        public List<string> MemberOf { get; private set; }

        private ADGroup(ADDomain Domain, string DistinguishedName, SecurityIdentifier SecurityIdentifier, string SamAccountName, string Name, List<string> MemberOf)
        {
            this.Domain = Domain;
            this.DistinguishedName = DistinguishedName;
            this.SecurityIdentifier = SecurityIdentifier;
            this.SamAccountName = SamAccountName;
            this.Name = Name;
            this.MemberOf = MemberOf;
        }

        public static ADGroup FromSearchResult(ADSearchResult SearchResult)
        {
            if (SearchResult == null)
                throw new ArgumentNullException("SearchResult");

            var name = SearchResult.Value<string>("name");
            var distinguishedName = SearchResult.Value<string>("distinguishedName");
            var sAMAccountName = SearchResult.Value<string>("sAMAccountName");
            var objectSid = new SecurityIdentifier(SearchResult.Value<byte[]>("objectSid"), 0);
            var memberOf = SearchResult.Values<string>("memberOf").ToList();

            return new ADGroup(SearchResult.Domain, distinguishedName, objectSid, sAMAccountName, name, memberOf);
        }

        public static ADGroup FromDirectoryEntry(ADDirectoryEntry DirectoryEntry)
        {
            if (DirectoryEntry == null)
                throw new ArgumentNullException("DirectoryEntry");

            var properties = DirectoryEntry.Entry.Properties;

            var name = properties.Value<string>("name");
            var distinguishedName = properties.Value<string>("distinguishedName");
            var sAMAccountName = properties.Value<string>("sAMAccountName");
            var objectSid = new SecurityIdentifier(properties.Value<byte[]>("objectSid"), 0);
            var memberOf = properties.Values<string>("memberOf").ToList();

            return new ADGroup(DirectoryEntry.Domain, distinguishedName, objectSid, sAMAccountName, name, memberOf);
        }

        public override string ToString()
        {
            return this.Id;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is ADGroup))
                return false;
            else
                return this.DistinguishedName == ((ADGroup)obj).DistinguishedName;
        }
        public override int GetHashCode()
        {
            return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(this.DistinguishedName);
        }
    }
}