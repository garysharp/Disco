using Disco.Data.Repository;
using Disco.Models.Interop.ActiveDirectory;
using Disco.Services.Interop.ActiveDirectory.Internal;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Threading.Tasks;

namespace Disco.Services.Interop.ActiveDirectory
{
    public static class ActiveDirectory
    {
        private const int SingleSearchResult = 1;
        public const int MaxForestServerSearch = 30;

        public static void Initialize(DiscoDataContext Database)
        {
            ADInterop.Initialize(Database);
        }
        public static void UpdateSearchContainers(DiscoDataContext Database, List<string> Containers)
        {
            ADInterop.UpdateSearchContainers(Database, Containers);
        }
        public static bool UpdateSearchEntireForest(DiscoDataContext Database, bool SearchEntireForest)
        {
            return ADInterop.UpdateSearchEntireForest(Database, SearchEntireForest);
        }

        public static ActiveDirectoryDomain PrimaryDomain
        {
            get
            {
                return ADInterop.PrimaryDomain;
            }
        }
        public static IEnumerable<ActiveDirectoryDomain> Domains
        {
            get
            {
                return ADInterop.Domains.ToList();
            }
        }

        public static ActiveDirectorySite Site
        {
            get
            {
                return ADInterop.Site;
            }
        }

        public static ActiveDirectoryDomain GetDomainByDistinguishedName(string DistinguishedName)
        {
            return ADInterop.GetDomainByDistinguishedName(DistinguishedName);
        }

        public static ActiveDirectoryDomain GetDomainByNetBiosName(string NetBiosName)
        {
            return ADInterop.GetDomainByNetBiosName(NetBiosName);
        }

        public static ActiveDirectoryDomain GetDomainByDnsName(string DnsName)
        {
            return ADInterop.GetDomainByDnsName(DnsName);
        }

        public static ActiveDirectoryDomain GetDomainFromId(string Id)
        {
            return ADInterop.GetDomainFromId(Id);
        }

        public static List<string> LoadForestServers()
        {
            return ADInterop.LoadForestServers();
        }

        public static Task<List<string>> LoadForestServersAsync()
        {
            return ADInterop.LoadForestServersAsync();
        }

        public static bool SearchEntireForest
        {
            get
            {
                return ADInterop.SearchEntireForest;
            }
        }

        #region Machine Account
        private static readonly string[] MachineLoadProperties = { "name", "distinguishedName", "sAMAccountName", "objectSid", "dNSHostName", "netbootGUID", "isCriticalSystemObject" };

        public static ActiveDirectoryMachineAccount RetrieveMachineAccount(DomainController DomainController, string Id, params string[] AdditionalProperties)
        {
            return RetrieveMachineAccount(DomainController, Id, (System.Guid?)null, (System.Guid?)null, AdditionalProperties);
        }
        public static ActiveDirectoryMachineAccount RetrieveMachineAccount(string Id, params string[] AdditionalProperties)
        {
            return RetrieveMachineAccount(Id, (System.Guid?)null, (System.Guid?)null, AdditionalProperties);
        }
        public static ActiveDirectoryMachineAccount RetrieveMachineAccount(DomainController DomainController, string Id, System.Guid? UUIDNetbootGUID, params string[] AdditionalProperties)
        {
            return RetrieveMachineAccount(DomainController, Id, UUIDNetbootGUID, (System.Guid?)null, AdditionalProperties);
        }
        public static ActiveDirectoryMachineAccount RetrieveMachineAccount(string Id, System.Guid? UUIDNetbootGUID, params string[] AdditionalProperties)
        {
            return RetrieveMachineAccount(Id, UUIDNetbootGUID, (System.Guid?)null, AdditionalProperties);
        }
        public static ActiveDirectoryMachineAccount RetrieveMachineAccount(string Id, System.Guid? UUIDNetbootGUID, System.Guid? MacAddressNetbootGUID, params string[] AdditionalProperties)
        {
            return RetrieveMachineAccount(null, Id, UUIDNetbootGUID, MacAddressNetbootGUID, AdditionalProperties);
        }
        public static ActiveDirectoryMachineAccount RetrieveMachineAccount(DomainController DomainController, string Id, System.Guid? UUIDNetbootGUID, System.Guid? MacAddressNetbootGUID, params string[] AdditionalProperties)
        {
            if (string.IsNullOrWhiteSpace(Id))
                throw new ArgumentNullException("Id");

            // Add $ identifier for machine accounts
            if (!Id.EndsWith("$"))
                Id += "$";

            const string ldapFilterTemplate = "(&(objectCategory=computer)(sAMAccountName={0}))";
            const string ldapNetbootGuidFilterTemplate = "(&(objectCategory=computer)(netbootGUID={0}))";

            string[] loadProperites = (AdditionalProperties != null && AdditionalProperties.Length > 0)
                ? MachineLoadProperties.Concat(AdditionalProperties).ToArray()
                : MachineLoadProperties;

            ActiveDirectorySearchResult adResult;

            var splitId = UserExtensions.SplitUserId(Id);
            var ldapSamAccountFilter = string.Format(ldapFilterTemplate, splitId.Item2);
            var domain = ADInterop.GetDomainFromId(Id);

            adResult = ADInterop.SearchAll(domain, DomainController, ldapSamAccountFilter, SingleSearchResult, loadProperites).FirstOrDefault();
            if (adResult == null && (UUIDNetbootGUID.HasValue || MacAddressNetbootGUID.HasValue))
            {

                if (UUIDNetbootGUID.HasValue)
                {
                    var ldapFilter = string.Format(ldapNetbootGuidFilterTemplate, ADInterop.FormatGuidForLdapQuery(UUIDNetbootGUID.Value));
                    adResult = ADInterop.SearchAll(domain, DomainController, ldapFilter, SingleSearchResult, loadProperites).FirstOrDefault();
                }
                if (adResult == null && MacAddressNetbootGUID.HasValue)
                {
                    var ldapFilter = string.Format(ldapNetbootGuidFilterTemplate, ADInterop.FormatGuidForLdapQuery(MacAddressNetbootGUID.Value));
                    adResult = ADInterop.SearchAll(domain, DomainController, ldapFilter, SingleSearchResult, loadProperites).FirstOrDefault();
                }
            }

            if (adResult != null)
                return adResult.AsMachineAccount(AdditionalProperties);
            else
                return null; // Not Found
        }

        private static ActiveDirectoryMachineAccount AsMachineAccount(this ActiveDirectorySearchResult item, string[] AdditionalProperties)
        {
            string name = item.Result.Properties["name"][0].ToString();
            string sAMAccountName = item.Result.Properties["sAMAccountName"][0].ToString();
            string distinguishedName = item.Result.Properties["distinguishedName"][0].ToString();
            string objectSid = ADInterop.ConvertBytesToSDDLString((byte[])item.Result.Properties["objectSid"][0]);

            var dNSNameProperty = item.Result.Properties["dNSHostName"];
            string dNSName = null;
            if (dNSNameProperty.Count > 0)
                dNSName = dNSNameProperty[0].ToString();
            else
                dNSName = string.Format("{0}.{1}", sAMAccountName.TrimEnd('$'), item.Domain.DnsName);

            bool isCriticalSystemObject = (bool)item.Result.Properties["isCriticalSystemObject"][0];

            System.Guid netbootGUIDResult = default(System.Guid);
            ResultPropertyValueCollection netbootGUIDProp = item.Result.Properties["netbootGUID"];
            if (netbootGUIDProp.Count > 0)
            {
                netbootGUIDResult = new System.Guid((byte[])netbootGUIDProp[0]);
            }

            // Additional Properties
            Dictionary<string, object[]> additionalProperties = new Dictionary<string, object[]>();
            if (AdditionalProperties != null)
                foreach (string propertyName in AdditionalProperties)
                {
                    var property = item.Result.Properties[propertyName];
                    var propertyValues = new List<object>();
                    for (int index = 0; index < property.Count; index++)
                        propertyValues.Add(property[index]);
                    additionalProperties.Add(propertyName, propertyValues.ToArray());
                }

            return new ActiveDirectoryMachineAccount
            {
                Name = name,
                DistinguishedName = distinguishedName,
                SamAccountName = sAMAccountName,
                SecurityIdentifier = objectSid,
                NetbootGUID = netbootGUIDResult,
                Path = item.Result.Path,
                Domain = item.Domain.NetBiosName,
                DnsName = dNSName,
                IsCriticalSystemObject = isCriticalSystemObject,
                LoadedProperties = additionalProperties
            };
        }

        public static string OfflineDomainJoinProvision(ActiveDirectoryDomain Domain, DomainController DomainController, string ComputerName, string OrganisationalUnit, ref ActiveDirectoryMachineAccount MachineAccount, out string DiagnosticInformation)
        {
            const string ldapFilterTemplate = "(&(objectCategory=computer)(sAMAccountName={0}))";

            if (MachineAccount != null && MachineAccount.IsCriticalSystemObject)
                throw new InvalidOperationException(string.Format("This account {0} is a Critical System Active Directory Object and Disco refuses to modify it", MachineAccount.DistinguishedName));

            if (MachineAccount != null)
                MachineAccount.DeleteAccount(DomainController);

            var computerId = UserExtensions.SplitUserId(ComputerName);

            var offlineJoinResult = ADInterop.OfflineDomainJoinProvision(Domain, DomainController, computerId.Item2, OrganisationalUnit, out DiagnosticInformation);

            // Reload newly created Account
            string[] loadAdditionalProperties = (MachineAccount != null && MachineAccount.LoadedProperties != null && MachineAccount.LoadedProperties.Count > 0)
                ? MachineAccount.LoadedProperties.Keys.ToArray()
                : null;

            MachineAccount = null;

            string[] loadProperites = (loadAdditionalProperties != null)
                ? MachineLoadProperties.Concat(loadAdditionalProperties).ToArray()
                : MachineLoadProperties;

            var ldapSamAccountName = computerId.Item2.EndsWith("$") ? computerId.Item2 : computerId.Item2 + "$";
            var ldapFilter = string.Format(ldapFilterTemplate, ldapSamAccountName);
            var ldapResult = ADInterop.SearchDomain(Domain, DomainController, Domain.DistinguishedName, ldapFilter, 1, loadProperites).FirstOrDefault();

            if (ldapResult != null)
                MachineAccount = ldapResult.AsMachineAccount(loadAdditionalProperties);

            return offlineJoinResult;
        }

        #endregion

        #region User Account
        private static readonly string[] UserLoadProperties = { "name", "distinguishedName", "sAMAccountName", "objectSid", "displayName", "sn", "givenName", "memberOf", "primaryGroupID", "mail", "telephoneNumber" };

        public static ActiveDirectoryUserAccount RetrieveUserAccount(string Id, params string[] AdditionalProperties)
        {
            const string ldapFilter = "(&(objectCategory=Person)(sAMAccountName={0}))";

            string[] loadProperites = (AdditionalProperties != null && AdditionalProperties.Length > 0)
                ? UserLoadProperties.Concat(AdditionalProperties).ToArray()
                : UserLoadProperties;

            return SearchBySamAccountName(Id, ldapFilter, loadProperites).Select(result => result.AsUserAccount(AdditionalProperties)).FirstOrDefault();
        }
        public static IEnumerable<ActiveDirectoryUserAccount> SearchUserAccounts(string Term, params string[] AdditionalProperties)
        {
            const int resultLimit = 30; // Default Search Limit

            if (string.IsNullOrWhiteSpace(Term))
                throw new ArgumentNullException("Term");

            // Apply Domain Restriction
            ActiveDirectoryDomain searchDomain = null;
            Term = ApplySearchTermDomainRestriction(Term, out searchDomain);

            if (string.IsNullOrWhiteSpace(Term))
                return Enumerable.Empty<ActiveDirectoryUserAccount>();

            string ldapFilter = string.Format("(&(objectCategory=Person)(objectClass=user)(|(sAMAccountName=*{0}*)(displayName=*{0}*)))", ADInterop.EscapeLdapQuery(Term));

            string[] loadProperites = (AdditionalProperties != null && AdditionalProperties.Length > 0)
                ? UserLoadProperties.Concat(AdditionalProperties).ToArray()
                : UserLoadProperties;

            IEnumerable<ActiveDirectorySearchResult> searchResults;
            if (searchDomain == null)
                searchResults = ADInterop.SearchScope(ldapFilter, resultLimit, loadProperites);
            else
                searchResults = ADInterop.SearchScope(searchDomain, ldapFilter, resultLimit, loadProperites);

            return searchResults.Select(result => result.AsUserAccount(AdditionalProperties));
        }

        private static ActiveDirectoryUserAccount AsUserAccount(this ActiveDirectorySearchResult item, string[] AdditionalProperties)
        {
            string name = item.Result.Properties["name"][0].ToString();
            string username = item.Result.Properties["sAMAccountName"][0].ToString();
            string distinguishedName = item.Result.Properties["distinguishedName"][0].ToString();
            byte[] objectSid = (byte[])item.Result.Properties["objectSid"][0];
            string objectSidSDDL = ADInterop.ConvertBytesToSDDLString(objectSid);

            ResultPropertyValueCollection displayNameProp = item.Result.Properties["displayName"];
            string displayName = username;
            if (displayNameProp.Count > 0)
                displayName = displayNameProp[0].ToString();
            string surname = null;
            ResultPropertyValueCollection surnameProp = item.Result.Properties["sn"];
            if (surnameProp.Count > 0)
                surname = surnameProp[0].ToString();
            string givenName = null;
            ResultPropertyValueCollection givenNameProp = item.Result.Properties["givenName"];
            if (givenNameProp.Count > 0)
                givenName = givenNameProp[0].ToString();
            string email = null;
            ResultPropertyValueCollection emailProp = item.Result.Properties["mail"];
            if (emailProp.Count > 0)
                email = emailProp[0].ToString();
            string phone = null;
            ResultPropertyValueCollection phoneProp = item.Result.Properties["telephoneNumber"];
            if (phoneProp.Count > 0)
                phone = phoneProp[0].ToString();

            int primaryGroupID = (int)item.Result.Properties["primaryGroupID"][0];
            string primaryGroupSid = ADInterop.ConvertBytesToSDDLString(ADInterop.BuildPrimaryGroupSid(objectSid, primaryGroupID));
            var groupDistinguishedNames = item.Result.Properties["memberOf"].Cast<string>().ToList();
            groupDistinguishedNames.Add(ADGroupCache.GetGroupsDistinguishedNameForSecurityIdentifier(primaryGroupSid));
            List<string> groups = ADGroupCache.GetGroups(groupDistinguishedNames).ToList();

            // Additional Properties
            Dictionary<string, object[]> additionalProperties = new Dictionary<string, object[]>();
            if (AdditionalProperties != null)
                foreach (string propertyName in AdditionalProperties)
                {
                    var property = item.Result.Properties[propertyName];
                    var propertyValues = new List<object>();
                    for (int index = 0; index < property.Count; index++)
                        propertyValues.Add(property[index]);
                    additionalProperties.Add(propertyName, propertyValues.ToArray());
                }

            return new ActiveDirectoryUserAccount
            {
                Domain = item.Domain.NetBiosName,
                Name = name,
                Surname = surname,
                GivenName = givenName,
                Email = email,
                Phone = phone,
                DistinguishedName = distinguishedName,
                SamAccountName = username,
                DisplayName = displayName,
                SecurityIdentifier = objectSidSDDL,
                Groups = groups,
                Path = item.Result.Path,
                LoadedProperties = additionalProperties
            };
        }
        #endregion

        #region Groups
        private static readonly string[] GroupLoadProperties = { "name", "distinguishedName", "cn", "sAMAccountName", "objectSid", "memberOf" };

        public static ActiveDirectoryGroup RetrieveGroup(string Id)
        {
            const string ldapFilter = "(&(objectCategory=Group)(objectSid={0}))";

            return SearchBySamAccountName(Id, ldapFilter, GroupLoadProperties).Select(result => result.AsGroup()).FirstOrDefault();
        }
        public static ActiveDirectoryGroup RetrieveGroupWithDistinguishedName(string DistinguishedName)
        {
            ActiveDirectoryDomain domain;

            using (var groupEntry = ADInterop.RetrieveDirectoryEntry(DistinguishedName, out domain))
            {
                if (groupEntry == null)
                    return null;

                return groupEntry.AsGroup(domain);
            }
        }
        public static ActiveDirectoryGroup RetrieveGroupWithSecurityIdentifier(string SecurityIdentifier)
        {
            const int resultLimit = 1;

            if (string.IsNullOrWhiteSpace(SecurityIdentifier))
                throw new ArgumentNullException("SecurityIdentifier");

            var sidBytes = ADInterop.ConvertSDDLStringToBytes(SecurityIdentifier);
            var sidBinaryString = ADInterop.ConvertBytesToBinarySidString(sidBytes);

            string ldapFilter = string.Format("(&(objectCategory=Group)(objectSid={0}))", sidBinaryString);

            return ADInterop.SearchAll(ldapFilter, resultLimit, null).Select(result => result.AsGroup()).FirstOrDefault();
        }
        public static IEnumerable<ActiveDirectoryGroup> SearchGroups(string Term)
        {
            const int resultLimit = 30; // Default Search Limit

            if (string.IsNullOrWhiteSpace(Term))
                throw new ArgumentNullException("Term");

            // Apply Domain Restriction
            ActiveDirectoryDomain searchDomain = null;
            Term = ApplySearchTermDomainRestriction(Term, out searchDomain);

            if (string.IsNullOrWhiteSpace(Term))
                return Enumerable.Empty<ActiveDirectoryGroup>();

            string ldapFilter = string.Format("(&(objectCategory=Group)(|(sAMAccountName=*{0}*)(name=*{0}*)(cn=*{0}*)))", ADInterop.EscapeLdapQuery(Term));

            IEnumerable<ActiveDirectorySearchResult> searchResults;
            if (searchDomain == null)
                searchResults = ADInterop.SearchScope(ldapFilter, resultLimit, GroupLoadProperties);
            else
                searchResults = ADInterop.SearchScope(searchDomain, ldapFilter, resultLimit, GroupLoadProperties);

            return searchResults.Select(result => result.AsGroup());
        }

        private static ActiveDirectoryGroup AsGroup(this ActiveDirectorySearchResult item)
        {
            var name = (string)item.Result.Properties["name"][0];
            var distinguishedName = (string)item.Result.Properties["distinguishedName"][0];
            var cn = (string)item.Result.Properties["cn"][0];
            var sAMAccountName = (string)item.Result.Properties["sAMAccountName"][0];
            var objectSid = ADInterop.ConvertBytesToSDDLString((byte[])item.Result.Properties["objectSid"][0]);
            var memberOf = item.Result.Properties["memberOf"].Cast<string>().ToList();

            return new ActiveDirectoryGroup()
            {
                Domain = item.Domain.NetBiosName,
                Name = name,
                DistinguishedName = distinguishedName,
                CommonName = cn,
                SamAccountName = sAMAccountName,
                SecurityIdentifier = objectSid,
                MemberOf = memberOf
            };
        }
        private static ActiveDirectoryGroup AsGroup(this DirectoryEntry item, ActiveDirectoryDomain Domain)
        {
            var name = (string)item.Properties["name"][0];
            var distinguishedName = (string)item.Properties["distinguishedName"][0];
            var cn = (string)item.Properties["cn"][0];
            var sAMAccountName = (string)item.Properties["sAMAccountName"][0];
            var objectSid = ADInterop.ConvertBytesToSDDLString((byte[])item.Properties["objectSid"][0]);
            var memberOf = item.Properties["memberOf"].Cast<string>().ToList();

            return new ActiveDirectoryGroup()
            {
                Domain = Domain.NetBiosName,
                Name = name,
                DistinguishedName = distinguishedName,
                CommonName = cn,
                SamAccountName = sAMAccountName,
                SecurityIdentifier = objectSid,
                MemberOf = memberOf
            };
        }
        #endregion

        #region Object
        private static readonly string[] ObjectLoadProperties = { "objectCategory" };
        private static readonly string[] ObjectLoadPropertiesAll = ObjectLoadProperties.Concat(UserLoadProperties).Concat(MachineLoadProperties).Concat(GroupLoadProperties).Distinct().ToArray();

        public static IActiveDirectoryObject RetrieveObject(string Id)
        {
            const string ldapFilter = "(&(|(objectCategory=Person)(objectCategory=Computer)(objectCategory=Group))(sAMAccountName={0}))";

            return SearchBySamAccountName(Id, ldapFilter, ObjectLoadPropertiesAll)
                .Select<ActiveDirectorySearchResult, IActiveDirectoryObject>(result =>
                {
                    var objectCategory = (string)result.Result.Properties["objectCategory"][0];
                    objectCategory = objectCategory.Substring(0, objectCategory.IndexOf(',')).ToLower();
                    switch (objectCategory)
                    {
                        case "cn=person":
                            return result.AsUserAccount(null);
                        case "cn=computer":
                            return result.AsMachineAccount(null);
                        case "cn=group":
                            return result.AsGroup();
                        default:
                            throw new InvalidOperationException("Unexpected objectCategory");
                    }
                }).FirstOrDefault();
        }
        #endregion

        #region Organisation Units

        public static List<ActiveDirectoryOrganisationalUnit> RetrieveOrganisationalUnitStructure(ActiveDirectoryDomain Domain)
        {
            using (DirectoryEntry domainRoot = ADInterop.RetrieveDirectoryEntry(Domain.DistinguishedName, out Domain))
            {
                return ActiveDirectory.RetrieveOrganisationalUnitStructureInternal(Domain, domainRoot);
            }
        }
        private static List<ActiveDirectoryOrganisationalUnit> RetrieveOrganisationalUnitStructureInternal(ActiveDirectoryDomain Domain, DirectoryEntry Container)
        {
            Dictionary<string, List<ActiveDirectoryOrganisationalUnit>> resultTree = new Dictionary<string, List<ActiveDirectoryOrganisationalUnit>>();

            using (DirectorySearcher adSearcher = new DirectorySearcher(Container, "(objectCategory=organizationalUnit)", new string[]
			{
				"name", 
				"distinguishedName"
			}, SearchScope.Subtree))
            {
                adSearcher.PageSize = 500;

                using (SearchResultCollection adResults = adSearcher.FindAll())
                {
                    resultTree = adResults.Cast<SearchResult>().Select(adResult =>
                    {
                        string i = adResult.Properties["name"][0].ToString();
                        string dn = adResult.Properties["distinguishedName"][0].ToString();
                        return new ActiveDirectoryOrganisationalUnit
                        {
                            Domain = Domain.NetBiosName,
                            Name = i,
                            DistinguishedName = dn,
                        };
                    }).GroupBy(ou => ou.DistinguishedName.Substring(ou.DistinguishedName.IndexOf(',') + 1)).ToDictionary(g => g.Key, g => g.ToList());
                }
            }

            // Build Tree
            var results = resultTree[Domain.DistinguishedName];
            foreach (var child in results)
                RetrieveOrganisationalUnitStructureChildrenInternal(child, resultTree);

            return results;
        }
        private static void RetrieveOrganisationalUnitStructureChildrenInternal(ActiveDirectoryOrganisationalUnit OrganisationalUnit, Dictionary<string, List<ActiveDirectoryOrganisationalUnit>> ResultTree)
        {
            List<ActiveDirectoryOrganisationalUnit> children;

            if (ResultTree.TryGetValue(OrganisationalUnit.DistinguishedName, out children))
            {
                foreach (var child in children)
                    RetrieveOrganisationalUnitStructureChildrenInternal(child, ResultTree);

                OrganisationalUnit.Children = children;
            }
        }

        #endregion

        #region Helpers

        private static IEnumerable<ActiveDirectorySearchResult> SearchBySamAccountName(string Id, string LdapFilterTemplate, string[] LoadProperties)
        {
            var splitId = UserExtensions.SplitUserId(Id);
            var ldapFilter = string.Format(LdapFilterTemplate, splitId.Item2);
            var domains = ADInterop.GetDomainFromId(Id);

            return ADInterop.SearchAll(domains, ldapFilter, SingleSearchResult, LoadProperties);
        }

        private static string ApplySearchTermDomainRestriction(string Term, out ActiveDirectoryDomain Domain)
        {
            if (string.IsNullOrWhiteSpace(Term))
                throw new ArgumentNullException("Term");

            var domainIndex = Term.IndexOf('\\');
            if (domainIndex >= 0)
            {
                var domain = Term.Substring(0, domainIndex);

                if (!ADInterop.TryGetDomainByNetBiosName(domain, out Domain))
                    return null; // Domain not found - invalid search

                if (Term.Length > (domainIndex + 1))
                    return Term.Substring(domainIndex + 1);
                else
                    return null; // Domain only, no Term
            }
            else
            {
                Domain = null;
                return Term;
            }
        }

        #endregion

    }
}