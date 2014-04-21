using Disco.Data.Repository;
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
                searchResults = searchDomain.SearchScope(ldapFilter, ADGroup.LoadProperties, ResultLimit);
            else
                searchResults = Context.SearchScope(ldapFilter, ADGroup.LoadProperties, ResultLimit);

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

        public static ADGroup RetrieveADGroup(string Id)
        {
            var domain = Context.GetDomainFromId(Id);
            return domain.GetAvailableDomainController().RetrieveADGroup(Id);
        }
        public static ADGroup RetrieveADGroupByDistinguishedName(string DistinguishedName)
        {
            var domain = Context.GetDomainFromDistinguishedName(DistinguishedName);
            return domain.GetAvailableDomainController().RetrieveADGroupByDistinguishedName(DistinguishedName);
        }
        public static ADGroup RetrieveADGroupWithSecurityIdentifier(SecurityIdentifier SecurityIdentifier)
        {
            var domain = Context.GetDomainFromSecurityIdentifier(SecurityIdentifier);
            return domain.GetAvailableDomainController().RetrieveADGroupWithSecurityIdentifier(SecurityIdentifier);
        }

        public static IEnumerable<ADGroup> SearchADGroups(string Term, int? ResultLimit = ActiveDirectory.DefaultSearchResultLimit)
        {
            if (string.IsNullOrWhiteSpace(Term))
                throw new ArgumentNullException("Term");

            ADDomain searchDomain;
            var term = RelevantSearchTerm(Term, out searchDomain);

            if (string.IsNullOrWhiteSpace(term))
                return Enumerable.Empty<ADGroup>();

            var ldapFilter= string.Format(ADGroup.LdapSearchFilterTemplate, ADHelpers.EscapeLdapQuery(term));

            IEnumerable<ADSearchResult> searchResults;
            if (searchDomain != null)
                searchResults = searchDomain.SearchScope(ldapFilter, ADGroup.LoadProperties, ResultLimit);
            else
                searchResults = Context.SearchScope(ldapFilter, ADGroup.LoadProperties, ResultLimit);

            return searchResults.Select(result => result.AsADGroup());
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
