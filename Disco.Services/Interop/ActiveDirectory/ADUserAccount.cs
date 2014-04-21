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
        internal const string LdapSearchFilterTemplate = "(&(objectCategory=Person)(objectClass=user)(|(sAMAccountName=*{0}*)(displayName=*{0}*)))";
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

        public object GetPropertyValue(string PropertyName, int Index = 0)
        {
            switch (PropertyName.ToLower())
            {
                case "name":
                    return this.Name;
                case "samaccountname":
                    return this.SamAccountName;
                case "distinguishedname":
                    return this.DistinguishedName;
                case "objectsid":
                    return this.SecurityIdentifier.ToString();
                case "sn":
                    return this.Surname;
                case "givenname":
                    return this.GivenName;
                case "mail":
                    return this.Email;
                case "telephonenumber":
                    return this.Phone;
                default:
                    object[] adProperty;
                    if (this.LoadedProperties.TryGetValue(PropertyName, out adProperty) && Index <= adProperty.Length)
                        return adProperty[Index];
                    else
                        return null;
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
