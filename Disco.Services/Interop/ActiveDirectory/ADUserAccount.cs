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
        internal static readonly string[] LoadProperties = { "name", "distinguishedName", "sAMAccountName", "objectSid", "userAccountControl", "isCriticalSystemObject", "displayName", "sn", "givenName", "memberOf", "primaryGroupID", "mail", "telephoneNumber" };
        internal static readonly string[] QuickLoadProperties = { "name", "distinguishedName", "sAMAccountName", "objectSid", "userAccountControl", "isCriticalSystemObject", "displayName", "sn", "givenName", "mail", "telephoneNumber" };


        public ADDomain Domain { get; private set; }

        public string DistinguishedName { get; private set; }
        public SecurityIdentifier SecurityIdentifier { get; private set; }

        public string Id { get { return $@"{Domain.NetBiosName}\{SamAccountName}"; } }
        public string SamAccountName { get; private set; }

        public string Name { get; private set; }
        public string DisplayName { get; private set; }

        public string Surname { get; private set; }
        public string GivenName { get; private set; }
        public string Email { get; private set; }
        public string Phone { get; private set; }

        public ADUserAccountControlFlags UserAccountControl { get; private set; }

        public bool IsCriticalSystemObject { get; private set; }

        public List<ADGroup> Groups { get; private set; }

        public Dictionary<string, object[]> LoadedProperties { get; private set; }

        public bool IsDisabled { get { return UserAccountControl.HasFlag(ADUserAccountControlFlags.ADS_UF_ACCOUNTDISABLE); } }
        public bool IsLockedOut { get { return UserAccountControl.HasFlag(ADUserAccountControlFlags.ADS_UF_LOCKOUT); } }
        public bool IsPasswordExpired { get { return UserAccountControl.HasFlag(ADUserAccountControlFlags.ADS_UF_PASSWORD_EXPIRED); } }

        private ADUserAccount(ADDomain Domain, string DistinguishedName, SecurityIdentifier SecurityIdentifier, string SamAccountName,
            string Name, string DisplayName, string Surname, string GivenName, string Email, string Phone, ADUserAccountControlFlags UserAccountControl,
            bool IsCriticalSystemObject, List<ADGroup> Groups, Dictionary<string, object[]> LoadedProperties)
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
            this.UserAccountControl = UserAccountControl;
            this.IsCriticalSystemObject = IsCriticalSystemObject;
            this.Groups = Groups;
            this.LoadedProperties = LoadedProperties;
        }

        public static ADUserAccount FromSearchResult(ADSearchResult SearchResult, bool Quick, string[] AdditionalProperties)
        {
            if (SearchResult == null)
                throw new ArgumentNullException("SearchResult");

            var name = SearchResult.Value<string>("name");
            var sAMAccountName = SearchResult.Value<string>("sAMAccountName");
            var distinguishedName = SearchResult.Value<string>("distinguishedName");
            var objectSid = new SecurityIdentifier(SearchResult.Value<byte[]>("objectSid"), 0);

            var displayName = SearchResult.Value<string>("displayName") ?? sAMAccountName;
            var surname = SearchResult.Value<string>("sn");
            var givenName = SearchResult.Value<string>("givenName");
            var email = SearchResult.Value<string>("mail");
            var phone = SearchResult.Value<string>("telephoneNumber");

            var userAccountControl = (ADUserAccountControlFlags)SearchResult.Value<int>("userAccountControl");
            var isCriticalSystemObject = SearchResult.Value<bool>("isCriticalSystemObject");

            List<ADGroup> groups = null;
            // Don't load Groups when doing a quick search
            if (!Quick)
            {
                var primaryGroupID = SearchResult.Value<int>("primaryGroupID");
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
                userAccountControl,
                isCriticalSystemObject,
                groups,
                additionalProperties);
        }

        public static ADUserAccount FromDirectoryEntry(ADDirectoryEntry directoryEntry, bool quick, string[] additionalProperties)
        {
            if (directoryEntry == null)
                throw new ArgumentNullException(nameof(directoryEntry));

            var properties = directoryEntry.Entry.Properties;

            var name = properties.Value<string>("name");
            var sAMAccountName = properties.Value<string>("sAMAccountName");
            var distinguishedName = properties.Value<string>("distinguishedName");
            var objectSid = new SecurityIdentifier(properties.Value<byte[]>("objectSid"), 0);

            var displayName = properties.Value<string>("displayName") ?? sAMAccountName;
            var surname = properties.Value<string>("sn");
            var givenName = properties.Value<string>("givenName");
            var email = properties.Value<string>("mail");
            var phone = properties.Value<string>("telephoneNumber");

            var userAccountControl = (ADUserAccountControlFlags)properties.Value<int>("userAccountControl");
            var isCriticalSystemObject = properties.Value<bool>("isCriticalSystemObject");

            List<ADGroup> groups = null;
            // Don't load Groups when doing a quick search
            if (!quick)
            {
                var primaryGroupID = properties.Value<int>("primaryGroupID");
                var primaryGroupSid = ADHelpers.BuildPrimaryGroupSid(objectSid, primaryGroupID);
                var memberGroups = properties.Values<string>("memberOf");

                var primaryGroup = ActiveDirectory.GroupCache.GetGroup(primaryGroupSid);

                var groupDistinguishedNames =
                    new string[] { primaryGroup.DistinguishedName }
                    .Concat(memberGroups);

                groups = ActiveDirectory.GroupCache.GetRecursiveGroups(groupDistinguishedNames).ToList();
            }

            // Additional Properties
            Dictionary<string, object[]> additionalProps;
            if (additionalProperties != null)
                additionalProps = additionalProperties
                    .Select(p => Tuple.Create(p, properties.Values<object>(p).ToArray()))
                    .ToDictionary(t => t.Item1, t => t.Item2);
            else
            {
                additionalProps = new Dictionary<string, object[]>();
            }

            return new ADUserAccount(
                directoryEntry.Domain,
                distinguishedName,
                objectSid,
                sAMAccountName,
                name,
                displayName,
                surname,
                givenName,
                email,
                phone,
                userAccountControl,
                isCriticalSystemObject,
                groups,
                additionalProps);
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
                    return new string[] { Name }.OfType<T>();
                case "samaccountname":
                    return new string[] { SamAccountName }.OfType<T>();
                case "distinguishedname":
                    return new string[] { DistinguishedName }.OfType<T>();
                case "objectsid":
                    return new SecurityIdentifier[] { SecurityIdentifier }.OfType<T>();
                case "sn":
                    return new string[] { Surname }.OfType<T>();
                case "givenname":
                    return new string[] { GivenName }.OfType<T>();
                case "mail":
                    return new string[] { Email }.OfType<T>();
                case "telephonenumber":
                    return new string[] { Phone }.OfType<T>();
                case "userAccountControl":
                    return new int[] { (int)UserAccountControl }.OfType<T>();
                default:
                    if (LoadedProperties.TryGetValue(PropertyName, out var adProperty))
                        return adProperty.OfType<T>();
                    else
                        return Enumerable.Empty<T>();
            }
        }

        public User ToRepositoryUser()
        {
            return new User
            {
                UserId = Id,
                DisplayName = DisplayName,
                Surname = Surname,
                GivenName = GivenName,
                EmailAddress = Email,
                PhoneNumber = Phone,
            };
        }

        #region Actions

        public void DeleteAccount(ADDomainController WritableDomainController)
        {
            if (IsCriticalSystemObject)
                throw new InvalidOperationException($"This account [{DistinguishedName}] is a Critical System Active Directory Object and Disco ICT refuses to modify it");

            if (!WritableDomainController.IsWritable)
                throw new InvalidOperationException($"The domain controller [{Name}] is not writable. This action (Delete Account) requires a writable domain controller.");

            using (ADDirectoryEntry entry = WritableDomainController.RetrieveDirectoryEntry(DistinguishedName))
            {
                entry.Entry.DeleteTree();
            }
        }
        public void DeleteAccount()
        {
            DeleteAccount(Domain.GetAvailableDomainController(RequireWritable: true));
        }

        public void DisableAccount(ADDomainController WritableDomainController)
        {
            if (IsCriticalSystemObject)
                throw new InvalidOperationException($"This account {DistinguishedName} is a Critical System Active Directory Object and Disco ICT refuses to modify it");

            if (!IsDisabled)
            {
                using (var deAccount = WritableDomainController.RetrieveDirectoryEntry(DistinguishedName))
                {
                    var accountControl = (ADUserAccountControlFlags)deAccount.Entry.Properties["userAccountControl"][0];
                    if (!accountControl.HasFlag(ADUserAccountControlFlags.ADS_UF_ACCOUNTDISABLE))
                    {
                        var updatedAccountControl = (accountControl | ADUserAccountControlFlags.ADS_UF_ACCOUNTDISABLE);
                        deAccount.Entry.Properties["userAccountControl"][0] = (int)updatedAccountControl;
                        deAccount.Entry.CommitChanges();
                        UserAccountControl = updatedAccountControl;
                    }
                }
            }
        }
        public void DisableAccount()
        {
            if (!IsDisabled)
            {
                DisableAccount(Domain.GetAvailableDomainController(RequireWritable: true));
            }
        }

        public void EnableAccount(ADDomainController WritableDomainController)
        {
            if (IsCriticalSystemObject)
                throw new InvalidOperationException($"This account {DistinguishedName} is a Critical System Active Directory Object and Disco ICT refuses to modify it");

            if (IsDisabled)
            {
                using (var deAccount = WritableDomainController.RetrieveDirectoryEntry(DistinguishedName))
                {
                    var accountControl = (ADUserAccountControlFlags)deAccount.Entry.Properties["userAccountControl"][0];
                    if (accountControl.HasFlag(ADUserAccountControlFlags.ADS_UF_ACCOUNTDISABLE))
                    {
                        var updatedAccountControl = (accountControl ^ ADUserAccountControlFlags.ADS_UF_ACCOUNTDISABLE);
                        deAccount.Entry.Properties["userAccountControl"][0] = (int)updatedAccountControl;
                        deAccount.Entry.CommitChanges();
                        UserAccountControl = updatedAccountControl;
                    }
                }
            }
        }
        public void EnableAccount()
        {
            if (IsDisabled)
            {
                EnableAccount(Domain.GetAvailableDomainController(RequireWritable: true));
            }
        }

        public void SetDisplayName(ADDomainController WritableDomainController, string DisplayName)
        {
            if (IsCriticalSystemObject)
                throw new InvalidOperationException($"This account {DistinguishedName} is a Critical System Active Directory Object and Disco ICT refuses to modify it");

            if (this.DisplayName != DisplayName)
            {
                using (var deAccount = WritableDomainController.RetrieveDirectoryEntry(DistinguishedName))
                {
                    var property = deAccount.Entry.Properties["displayName"];
                    if (property.Count != 1 || (property[0] as string) != DisplayName)
                    {
                        if (property.Count > 0)
                        {
                            property.Clear();
                        }
                        if (!string.IsNullOrEmpty(DisplayName))
                        {
                            property.Add(DisplayName);
                        }
                        deAccount.Entry.CommitChanges();
                    }
                }
            }
        }
        public void SetDisplayName(string DisplayName)
        {
            if (this.DisplayName != DisplayName)
            {
                SetDisplayName(Domain.GetAvailableDomainController(RequireWritable: true), DisplayName);
            }
        }

        public void SetSurname(ADDomainController WritableDomainController, string Surname)
        {
            if (IsCriticalSystemObject)
                throw new InvalidOperationException($"This account {DistinguishedName} is a Critical System Active Directory Object and Disco ICT refuses to modify it");

            if (this.Surname != Surname)
            {
                using (var deAccount = WritableDomainController.RetrieveDirectoryEntry(DistinguishedName))
                {
                    var property = deAccount.Entry.Properties["sn"];
                    if (property.Count != 1 || (property[0] as string) != Surname)
                    {
                        if (property.Count > 0)
                        {
                            property.Clear();
                        }
                        if (!string.IsNullOrEmpty(Surname))
                        {
                            property.Add(Surname);
                        }
                        deAccount.Entry.CommitChanges();
                    }
                }
            }
        }
        public void SetSurname(string Surname)
        {
            if (this.Surname != Surname)
            {
                SetSurname(Domain.GetAvailableDomainController(RequireWritable: true), Surname);
            }
        }

        public void SetGivenName(ADDomainController WritableDomainController, string GivenName)
        {
            if (IsCriticalSystemObject)
                throw new InvalidOperationException($"This account {DistinguishedName} is a Critical System Active Directory Object and Disco ICT refuses to modify it");

            if (this.GivenName != GivenName)
            {
                using (var deAccount = WritableDomainController.RetrieveDirectoryEntry(DistinguishedName))
                {
                    var property = deAccount.Entry.Properties["givenName"];
                    if (property.Count != 1 || (property[0] as string) != GivenName)
                    {
                        if (property.Count > 0)
                        {
                            property.Clear();
                        }
                        if (!string.IsNullOrEmpty(GivenName))
                        {
                            property.Add(GivenName);
                        }
                        deAccount.Entry.CommitChanges();
                    }
                }
            }
        }
        public void SetGivenName(string GivenName)
        {
            if (this.GivenName != GivenName)
            {
                SetGivenName(Domain.GetAvailableDomainController(RequireWritable: true), GivenName);
            }
        }

        public void SetEmail(ADDomainController WritableDomainController, string Email)
        {
            if (IsCriticalSystemObject)
                throw new InvalidOperationException($"This account {DistinguishedName} is a Critical System Active Directory Object and Disco ICT refuses to modify it");

            if (this.Email != Email)
            {
                using (var deAccount = WritableDomainController.RetrieveDirectoryEntry(DistinguishedName))
                {
                    var property = deAccount.Entry.Properties["mail"];
                    if (property.Count != 1 || (property[0] as string) != Email)
                    {
                        if (property.Count > 0)
                        {
                            property.Clear();
                        }
                        if (!string.IsNullOrEmpty(Email))
                        {
                            property.Add(Email);
                        }
                        deAccount.Entry.CommitChanges();
                    }
                }
            }
        }
        public void SetEmail(string Email)
        {
            if (this.Email != Email)
            {
                SetEmail(Domain.GetAvailableDomainController(RequireWritable: true), Email);
            }
        }

        public void SetPhone(ADDomainController WritableDomainController, string Phone)
        {
            if (IsCriticalSystemObject)
                throw new InvalidOperationException($"This account {DistinguishedName} is a Critical System Active Directory Object and Disco ICT refuses to modify it");

            if (this.Phone != Phone)
            {
                using (var deAccount = WritableDomainController.RetrieveDirectoryEntry(DistinguishedName))
                {
                    var property = deAccount.Entry.Properties["telephoneNumber"];
                    if (property.Count != 1 || (property[0] as string) != Phone)
                    {
                        if (property.Count > 0)
                        {
                            property.Clear();
                        }
                        if (!string.IsNullOrEmpty(Phone))
                        {
                            property.Add(Phone);
                        }
                        deAccount.Entry.CommitChanges();
                    }
                }
            }
        }
        public void SetPhone(string Phone)
        {
            if (this.Phone != Phone)
            {
                SetPhone(Domain.GetAvailableDomainController(RequireWritable: true), Phone);
            }
        }

        #endregion

        public override string ToString()
        {
            return Id;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is ADUserAccount))
                return false;
            else
                return DistinguishedName == ((ADUserAccount)obj).DistinguishedName;
        }
        public override int GetHashCode()
        {
            return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(DistinguishedName);
        }
    }
}
