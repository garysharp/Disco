using Disco.Models.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;

namespace Disco.Services.Interop.ActiveDirectory
{
    public class ADUserAccount : IADObject
    {
        internal const string LdapSamAccountNameFilterTemplate = "(&(objectCategory=Person)(sAMAccountName={0}))";
        internal static string LdapSearchFilterTemplate = "(&(objectCategory=Person)(objectClass=user)(|(sAMAccountName={0}*)(displayName={0}*)(sn={0}*)(givenName={0}*)))";
        internal static readonly string[] LoadProperties = { "name", "distinguishedName", "sAMAccountName", "objectSid", "displayName", "sn", "givenName", "memberOf", "primaryGroupID", "mail", "telephoneNumber" };
        internal static readonly string[] QuickLoadProperties = { "name", "distinguishedName", "sAMAccountName", "objectSid", "displayName", "sn", "givenName", "mail", "telephoneNumber" };


        public ADDomain Domain { get; private set; }

        public string DistinguishedName { get; private set; }
        public SecurityIdentifier SecurityIdentifier { get; private set; }

        public string Id { get { return string.Format(@"{0}\{1}", Domain.NetBiosName, SamAccountName); } }
        public string SamAccountName { get; private set; }

        public string Name { get; private set; }
        public string DisplayName { get; private set; }

        public string Surname { get; private set; }
        public string GivenName { get; private set; }
        public string Email { get; private set; }
        public string Phone { get; private set; }

        public List<ADGroup> Groups { get; private set; }

        public Dictionary<string, object[]> LoadedProperties { get; private set; }

        private ADUserAccount(ADDomain Domain, string DistinguishedName, SecurityIdentifier SecurityIdentifier, string SamAccountName,
            string Name, string DisplayName, string Surname, string GivenName, string Email, string Phone,
            List<ADGroup> Groups, Dictionary<string, object[]> LoadedProperties)
        {
            this.Domain = Domain;
            this.DistinguishedName = DistinguishedName;
            this.SecurityIdentifier = SecurityIdentifier;
            this.SamAccountName = SamAccountName;
            this.Name = Name;
            this.DisplayName = DisplayName;
            this.Surname = Surname;
            this.GivenName = GivenName;
            this.Email = Email;
            this.Phone = Phone;
            this.Groups = Groups;
            this.LoadedProperties = LoadedProperties;
        }

        public static ADUserAccount FromSearchResult(ADSearchResult SearchResult, bool Quick, string[] AdditionalProperties)
        {
            if (SearchResult == null)
                throw new ArgumentNullException("SearchResult");

            string name = SearchResult.Value<string>("name");
            string sAMAccountName = SearchResult.Value<string>("sAMAccountName");
            string distinguishedName = SearchResult.Value<string>("distinguishedName");
            var objectSid = new SecurityIdentifier(SearchResult.Value<byte[]>("objectSid"), 0);

            var displayName = SearchResult.Value<string>("displayName") ?? sAMAccountName;
            var surname = SearchResult.Value<string>("sn");
            string givenName = SearchResult.Value<string>("givenName");
            string email = SearchResult.Value<string>("mail");
            string phone = SearchResult.Value<string>("telephoneNumber");

            List<ADGroup> groups = null;
            // Don't load Groups when doing a quick search
            if (!Quick)
            {
                int primaryGroupID = (int)SearchResult.Value<int>("primaryGroupID");
                var primaryGroupSid = ADHelpers.BuildPrimaryGroupSid(objectSid, primaryGroupID);
                var memberGroups = SearchResult.Values<string>("memberOf");

                var primaryGroup = ActiveDirectory.GroupCache.GetGroup(primaryGroupSid);

                var groupDistinguishedNames =
                    new string[] { primaryGroup.DistinguishedName }
                    .Concat(memberGroups);

                groups = ActiveDirectory.GroupCache.GetRecursiveGroups(groupDistinguishedNames).ToList();
            }

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

            return new ADUserAccount(
                SearchResult.Domain,
                distinguishedName,
                objectSid,
                sAMAccountName,
                name,
                displayName,
                surname,
                givenName,
                email,
                phone,
                groups,
                additionalProperties);
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
                case "sn":
                    return new string[] { this.Surname }.OfType<T>();
                case "givenname":
                    return new string[] { this.GivenName }.OfType<T>();
                case "mail":
                    return new string[] { this.Email }.OfType<T>();
                case "telephonenumber":
                    return new string[] { this.Phone }.OfType<T>();
                default:
                    object[] adProperty;
                    if (this.LoadedProperties.TryGetValue(PropertyName, out adProperty))
                        return adProperty.OfType<T>();
                    else
                        return Enumerable.Empty<T>();
            }
        }

        public User ToRepositoryUser()
        {
            return new User
            {
                UserId = this.Id,
                DisplayName = this.DisplayName,
                Surname = this.Surname,
                GivenName = this.GivenName,
                EmailAddress = this.Email,
                PhoneNumber = this.Phone,
            };
        }

        public override string ToString()
        {
            return this.Id;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is ADUserAccount))
                return false;
            else
                return this.DistinguishedName == ((ADUserAccount)obj).DistinguishedName;
        }
        public override int GetHashCode()
        {
            return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(this.DistinguishedName);
        }
    }
}
