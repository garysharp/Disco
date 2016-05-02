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
        internal static string LdapSearchFilterTemplate = "(&(objectCategory=Group)(|(sAMAccountName=*{0}*)(name=*{0}*)(cn=*{0}*)))";
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

        public Dictionary<string, object[]> LoadedProperties { get; private set; }

        private ADGroup(ADDomain Domain, string DistinguishedName, SecurityIdentifier SecurityIdentifier, string SamAccountName, string Name, List<string> MemberOf, Dictionary<string, object[]> LoadedProperties)
        {
            this.Domain = Domain;
            this.DistinguishedName = DistinguishedName;
            this.SecurityIdentifier = SecurityIdentifier;
            this.SamAccountName = SamAccountName;
            this.Name = Name;
            this.MemberOf = MemberOf;
            this.LoadedProperties = LoadedProperties;
        }

        public static ADGroup FromSearchResult(ADSearchResult SearchResult, string[] AdditionalProperties)
        {
            if (SearchResult == null)
                throw new ArgumentNullException("SearchResult");

            var name = SearchResult.Value<string>("name");
            var distinguishedName = SearchResult.Value<string>("distinguishedName");
            var sAMAccountName = SearchResult.Value<string>("sAMAccountName");
            var objectSid = new SecurityIdentifier(SearchResult.Value<byte[]>("objectSid"), 0);
            var memberOf = SearchResult.Values<string>("memberOf").ToList();

            // Additional Properties
            Dictionary<string, object[]> additionalProperties;
            if (AdditionalProperties != null)
                additionalProperties = AdditionalProperties
                    .Select(p => Tuple.Create(p, SearchResult.Values<object>(p).ToArray()))
                    .ToDictionary(t => t.Item1, t => t.Item2);
            else
            {
                additionalProperties = new Dictionary<string, object[]>();
            }

            return new ADGroup(SearchResult.Domain, distinguishedName, objectSid, sAMAccountName, name, memberOf, additionalProperties);
        }

        public static ADGroup FromDirectoryEntry(ADDirectoryEntry DirectoryEntry, string[] AdditionalProperties)
        {
            if (DirectoryEntry == null)
                throw new ArgumentNullException("DirectoryEntry");

            var properties = DirectoryEntry.Entry.Properties;

            var name = properties.Value<string>("name");
            var distinguishedName = properties.Value<string>("distinguishedName");
            var sAMAccountName = properties.Value<string>("sAMAccountName");
            var objectSid = new SecurityIdentifier(properties.Value<byte[]>("objectSid"), 0);
            var memberOf = properties.Values<string>("memberOf").ToList();

            Dictionary<string, object[]> additionalProperties;
            if (AdditionalProperties != null)
                additionalProperties = AdditionalProperties
                    .Select(p => Tuple.Create(p, properties.Values<object>(p).ToArray()))
                    .ToDictionary(t => t.Item1, t => t.Item2);
            else
            {
                additionalProperties = new Dictionary<string, object[]>();
            }

            return new ADGroup(DirectoryEntry.Domain, distinguishedName, objectSid, sAMAccountName, name, memberOf, additionalProperties);
        }

        [Obsolete("Use generic equivalents: GetPropertyValue<T>(string PropertyName)")]
        public object GetPropertyValue(string PropertyName, int Index = 0)
        {
            return GetPropertyValues<object>(PropertyName).Skip(Index).FirstOrDefault();
        }

        public T GetPropertyValue<T>(string PropertyName)
        {
            return GetPropertyValues<T>(PropertyName).FirstOrDefault();
        }
        public IEnumerable<T> GetPropertyValues<T>(string PropertyName)
        {
            switch (PropertyName.ToLower())
            {
                case "name":
                    return new string[] { this.Name }.OfType<T>();
                case "samaccountname":
                    return new string[] { this.SamAccountName }.OfType<T>();
                case "distinguishedname":
                    return new string[] { this.DistinguishedName }.OfType<T>();
                case "objectsid":
                    return new SecurityIdentifier[] { this.SecurityIdentifier }.OfType<T>();
                case "memberof":
                    return this.MemberOf.OfType<T>();
                default:
                    object[] adProperty;
                    if (this.LoadedProperties.TryGetValue(PropertyName, out adProperty))
                        return adProperty.OfType<T>();
                    else
                        return Enumerable.Empty<T>();
            }
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