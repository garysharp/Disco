using Disco.Data.Repository;
using Disco.Models.Repository;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Services.Interop.ActiveDirectory
{
    public static class ActiveDirectory
    {
        public const int SingleSearchResult = 1;
        public const int DefaultSearchResultLimit = 30;
        public const int MaxForestServerSearch = 30;
        public const int DomainControllerUnavailableMinutes = 10;

        private static ActiveDirectoryContext context;
        private static ActiveDirectoryGroupCache groupCache;
        private static object contextInitializingLock = new object();

        public static void Initialize(DiscoDataContext Database)
        {
            lock (contextInitializingLock)
            {
                context = new ActiveDirectoryContext(Database);
                groupCache = new ActiveDirectoryGroupCache();
            }
        }

        public static ActiveDirectoryContext Context
        {
            get
            {
                if (context == null)
                {
                    lock (contextInitializingLock)
                    {
                        if (context == null)
                            throw new InvalidOperationException("Active Directory interoperability hasn't been initialized");
                    }
                }

                return context;
            }
        }
        public static ActiveDirectoryGroupCache GroupCache
        {
            get
            {
                if (groupCache == null)
                {
                    lock (contextInitializingLock)
                    {
                        if (groupCache == null)
                            throw new InvalidOperationException("Active Directory interoperability hasn't been initialized");
                    }
                }

                return groupCache;
            }
        }

        #region User Accounts

        public static ADUserAccount RetrieveADUserAccount(string Id, params string[] AdditionalProperties)
        {
            var domain = Context.GetDomainFromId(Id);
            return domain.GetAvailableDomainController().RetrieveADUserAccount(Id, AdditionalProperties);
        }

        public static ADUserAccount RetrieveADUserAccount(User User, params string[] AdditionalProperties)
        {
            var domain = Context.GetDomainFromId(User.UserId);
            return domain.GetAvailableDomainController().RetrieveADUserAccount(User.UserId, AdditionalProperties);
        }

        public static IEnumerable<ADUserAccount> SearchADUserAccounts(string Term, bool Quick, int? ResultLimit = ActiveDirectory.DefaultSearchResultLimit, params string[] AdditionalProperties)
        {
            if (string.IsNullOrWhiteSpace(Term))
                throw new ArgumentNullException("Term");

            ADDomain searchDomain;
            var term = RelevantSearchTerm(Term, out searchDomain);

            if (string.IsNullOrWhiteSpace(term))
                return Enumerable.Empty<ADUserAccount>();

            var ldapFilter = string.Format(ADUserAccount.LdapSearchFilterTemplate, ADHelpers.EscapeLdapQuery(term));

            IEnumerable<ADSearchResult> searchResults;
            if (searchDomain != null)
                searchResults = searchDomain.SearchScope(ldapFilter, ADUserAccount.LoadProperties, ResultLimit);
            else
                searchResults = Context.SearchScope(ldapFilter, ADUserAccount.LoadProperties, ResultLimit);

            return searchResults.Select(result => result.AsADUserAccount(Quick, AdditionalProperties));
        }

        #endregion

        #region Machine Accounts
        public static ADMachineAccount RetrieveADMachineAccount(string Id, params string[] AdditionalProperties)
        {
            var domain = Context.GetDomainFromId(Id);
            return domain.GetAvailableDomainController().RetrieveADMachineAccount(Id, AdditionalProperties);
        }
        public static ADMachineAccount RetrieveADMachineAccount(string Id, System.Guid? NetbootGUID, params string[] AdditionalProperties)
        {
            var domain = Context.GetDomainFromId(Id);
            return domain.GetAvailableDomainController().RetrieveADMachineAccount(Id, NetbootGUID, AdditionalProperties);
        }
        public static ADMachineAccount RetrieveADMachineAccount(string Id, System.Guid? UUIDNetbootGUID, System.Guid? MacAddressNetbootGUID, params string[] AdditionalProperties)
        {
            var domain = Context.GetDomainFromId(Id);
            return domain.GetAvailableDomainController().RetrieveADMachineAccount(Id, UUIDNetbootGUID, MacAddressNetbootGUID, AdditionalProperties);
        }
        #endregion

        #region Groups

        public static ADGroup RetrieveADGroup(string Id, params string[] AdditionalProperties)
        {
            var domain = Context.GetDomainFromId(Id);
            return domain.GetAvailableDomainController().RetrieveADGroup(Id, AdditionalProperties);
        }
        public static ADGroup RetrieveADGroupByDistinguishedName(string DistinguishedName, params string[] AdditionalProperties)
        {
            var domain = Context.GetDomainFromDistinguishedName(DistinguishedName);
            return domain.GetAvailableDomainController().RetrieveADGroupByDistinguishedName(DistinguishedName, AdditionalProperties);
        }
        public static ADGroup RetrieveADGroupWithSecurityIdentifier(SecurityIdentifier SecurityIdentifier, params string[] AdditionalProperties)
        {
            var domain = Context.GetDomainFromSecurityIdentifier(SecurityIdentifier);
            return domain.GetAvailableDomainController().RetrieveADGroupWithSecurityIdentifier(SecurityIdentifier, AdditionalProperties);
        }

        public static IEnumerable<ADGroup> SearchADGroups(string Term, int? ResultLimit = ActiveDirectory.DefaultSearchResultLimit, params string[] AdditionalProperties)
        {
            if (string.IsNullOrWhiteSpace(Term))
                throw new ArgumentNullException("Term");

            ADDomain searchDomain;
            var term = RelevantSearchTerm(Term, out searchDomain);

            if (string.IsNullOrWhiteSpace(term))
                return Enumerable.Empty<ADGroup>();

            var ldapFilter = string.Format(ADGroup.LdapSearchFilterTemplate, ADHelpers.EscapeLdapQuery(term));

            IEnumerable<ADSearchResult> searchResults;
            if (searchDomain != null)
                searchResults = searchDomain.SearchScope(ldapFilter, ADGroup.LoadProperties, ResultLimit);
            else
                searchResults = Context.SearchScope(ldapFilter, ADGroup.LoadProperties, ResultLimit);

            return searchResults.Select(result => result.AsADGroup(AdditionalProperties));
        }

        #endregion

        #region Organisational Units
        public static IEnumerable<Tuple<ADDomain, List<ADOrganisationalUnit>>> RetrieveADOrganisationalUnitStructure()
        {
            return Context.Domains
                .Select(d => d.GetAvailableDomainController())
                .Select(dc => Tuple.Create(dc.Domain, dc.RetrieveADOrganisationalUnitStructure()));
        }
        #endregion

        #region Objects
        public static IADObject RetrieveADObject(string Id, bool Quick)
        {
            var domain = Context.GetDomainFromId(Id);
            return domain.GetAvailableDomainController().RetrieveADObject(Id, Quick);
        }
        #endregion

        #region Actions

        public static string OfflineDomainJoinProvision(string ComputerSamAccountName, string OrganisationalUnit, ref ADMachineAccount MachineAccount, out string DiagnosticInformation)
        {
            var domain = Context.GetDomainFromDistinguishedName(OrganisationalUnit);
            var writableDomainController = domain.GetAvailableDomainController(RequireWritable: true);

            return writableDomainController.OfflineDomainJoinProvision(ComputerSamAccountName, OrganisationalUnit, ref MachineAccount, out DiagnosticInformation);
        }

        #endregion

        #region Helpers

        public static string ParseDomainAccountId(string AccountId)
        {
            return ParseDomainAccountId(AccountId, null);
        }
        public static string ParseDomainAccountId(string AccountId, string AccountDomain)
        {
            string accountUsername;
            ADDomain domain;

            return ParseDomainAccountId(AccountId, AccountDomain, out accountUsername, out domain);
        }
        public static string ParseDomainAccountId(string AccountId, out string AccountUsername)
        {
            return ParseDomainAccountId(AccountId, null, out AccountUsername);
        }
        public static string ParseDomainAccountId(string AccountId, string AccountDomain, out string AccountUsername)
        {
            ADDomain domain;

            return ParseDomainAccountId(AccountId, AccountDomain, out AccountUsername, out domain);
        }
        public static string ParseDomainAccountId(string AccountId, out ADDomain Domain)
        {
            return ParseDomainAccountId(AccountId, null, out Domain);
        }
        public static string ParseDomainAccountId(string AccountId, string AccountDomain, out ADDomain Domain)
        {
            string accountUsername;

            return ParseDomainAccountId(AccountId, AccountDomain, out accountUsername, out Domain);
        }
        public static string ParseDomainAccountId(string AccountId, out string AccountUsername, out ADDomain Domain)
        {
            return ParseDomainAccountId(AccountId, null, out AccountUsername, out Domain);
        }
        public static string ParseDomainAccountId(string AccountId, string AccountDomain, out string AccountUsername, out ADDomain Domain)
        {
            if (string.IsNullOrWhiteSpace(AccountId))
                throw new ArgumentNullException("AccountId");

            var slashIndex = AccountId.IndexOf('\\');

            if (slashIndex < 0 && !string.IsNullOrWhiteSpace(AccountDomain))
            {
                AccountId = AccountDomain + @"\" + AccountId;
                slashIndex = AccountDomain.Length;
            }

            if (slashIndex < 0)
            {
                AccountUsername = AccountId;
                Domain = Context.PrimaryDomain;
            }
            else
            {
                AccountUsername = AccountId.Substring(slashIndex + 1);
                Domain = Context.GetDomainByNetBiosName(AccountId.Substring(0, slashIndex));
            }

            return string.Concat(Domain.NetBiosName, @"\", AccountUsername);
        }

        public static bool IsValidDomainAccountId(string AccountId)
        {
            string accountUsername;
            ADDomain domain;

            return IsValidDomainAccountId(AccountId, out accountUsername, out domain);
        }
        public static bool IsValidDomainAccountId(string AccountId, out string AccountUsername)
        {
            ADDomain domain;

            return IsValidDomainAccountId(AccountId, out AccountUsername, out domain);
        }
        public static bool IsValidDomainAccountId(string AccountId, out ADDomain Domain)
        {
            string accountUsername;

            return IsValidDomainAccountId(AccountId, out accountUsername, out Domain);
        }
        public static bool IsValidDomainAccountId(string AccountId, out string AccountUsername, out ADDomain Domain)
        {
            if (string.IsNullOrEmpty(AccountId))
            {
                AccountUsername = null;
                Domain = null;
                return false;
            }

            var slashIndex = AccountId.IndexOf('\\');
            if (slashIndex < 0)
            {
                AccountUsername = AccountId;
                Domain = null;
                return false;
            }
            else
            {
                AccountUsername = AccountId.Substring(slashIndex + 1);
                return ActiveDirectory.Context.TryGetDomainByNetBiosName(AccountId.Substring(0, slashIndex), out Domain);
            }
        }

        /// <summary>
        /// If the AccountId Domain matches the Primary Domain, returns the Account Username without the Domain specified
        /// </summary>
        /// <returns></returns>
        public static string FriendlyAccountId(string AccountId)
        {
            var slashIndex = AccountId.IndexOf('\\');

            if (slashIndex > 0 && AccountId.Substring(0, slashIndex).Equals(ActiveDirectory.Context.PrimaryDomain.NetBiosName, StringComparison.OrdinalIgnoreCase))
                return AccountId.Substring(slashIndex + 1);
            else
                return AccountId;
        }

        private static string RelevantSearchTerm(string Term, out ADDomain Domain)
        {
            Domain = null;

            if (string.IsNullOrWhiteSpace(Term))
                return null;

            var term = Term.Trim();

            var domainSeperatorIndex = term.IndexOf('\\');

            if (domainSeperatorIndex >= 0)
            {
                // Domain Search Restriction

                if (term.Length > domainSeperatorIndex + 1)
                {
                    var netbiosName = term.Substring(0, domainSeperatorIndex);
                    if (Context.TryGetDomainByNetBiosName(netbiosName, out Domain))
                    {
                        return term.Substring(domainSeperatorIndex + 1);
                    }
                    else
                    {
                        return null; // Unknown Domain
                    }
                }
                else
                {
                    return null; // No term to search, only Domain
                }
            }

            return term;
        }

        #endregion
    }
}
