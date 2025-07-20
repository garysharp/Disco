﻿using Disco.Data.Repository;
using Disco.Services.Logging;
using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;

namespace Disco.Services.Interop.ActiveDirectory
{
    public class ActiveDirectoryContext
    {
        public ADSite Site { get; private set; }
        public ADDomain PrimaryDomain { get; private set; }
        public List<ADDomain> Domains { get; private set; }
        public ActiveDirectoryManagedGroups ManagedGroups { get; private set; }

        public List<string> AllServers
        {
            get
            {
                return ADDiscoverServers.LoadAllServersSync();
            }
        }

        private bool searchAllServers { get; set; }
        public bool SearchAllServers
        {
            get
            {
                var fs = ADDiscoverServers.AllServers;
                if (fs != null && fs.Count > ActiveDirectory.MaxAllServerSearch)
                    return false; // Never

                return searchAllServers;
            }
        }

        #region Constructor/Initializing

        private ActiveDirectoryContext()
        {
            ManagedGroups = new ActiveDirectoryManagedGroups();
        }

        internal ActiveDirectoryContext(DiscoDataContext Database) : this()
        {
            Initialize(Database);
        }

        private void Initialize(DiscoDataContext Database)
        {
            // Search Entire Directory (default: true)
            searchAllServers = Database.DiscoConfiguration.ActiveDirectory.SearchAllServers ?? true;

            // Set Search LDAP Filters
            InitializeWildcardSearchSufixOnly(Database.DiscoConfiguration.ActiveDirectory.SearchWildcardSuffixOnly);

            // Determine Site
            var computerSite = ActiveDirectorySite.GetComputerSite();
            Site = new ADSite(this, computerSite);

            // Determine Domains
            var computerDomain = Domain.GetComputerDomain();
            var domains = computerDomain.Forest.Domains
                .Cast<Domain>()
                .Select(d => new ADDomain(this, d))
                .ToList();

            // Try adding forest trust relationships
            try
            {
                var trustRelationships = computerDomain.Forest.GetAllTrustRelationships();

                foreach (TrustRelationshipInformation trustRelationship in trustRelationships)
                {
                    var sourceDomain = trustRelationship.SourceName;
                    if (!domains.Any(d => string.Equals(d.Name, sourceDomain, StringComparison.OrdinalIgnoreCase)))
                    {
                        try
                        {
                            var domain = Domain.GetDomain(new DirectoryContext(DirectoryContextType.Domain, sourceDomain));
                            domains.Add(new ADDomain(this, domain));
                        }
                        catch (Exception ex)
                        {
                            SystemLog.LogException($"ActiveDirectory Initialize Trust Domain: [{sourceDomain}]", ex);
                        }
                    }
                    var targetDomain = trustRelationship.TargetName;
                    if (!domains.Any(d => string.Equals(d.Name, targetDomain, StringComparison.OrdinalIgnoreCase)))
                    {
                        try
                        {
                            var domain = Domain.GetDomain(new DirectoryContext(DirectoryContextType.Domain, targetDomain));
                            domains.Add(new ADDomain(this, domain));
                        }
                        catch (Exception ex)
                        {
                            SystemLog.LogException($"ActiveDirectory Initialize Trust Domain: [{targetDomain}]", ex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SystemLog.LogException("ActiveDirectory Initialize Trust Domains", ex);
            }
            PrimaryDomain = domains.Where(d => d.Name == computerDomain.Name).First();
            Domains = domains;

            // Determine Search Scope Containers
            ReinitializeSearchContainers(Database.DiscoConfiguration.ActiveDirectory.SearchContainers);

            // Determine Domain Controllers
            var siteDomainControllers = computerSite.Servers
                .OfType<DomainController>()
                .Where(dc => dc.IsReachable())
                .Select(dc => new ADDomainController(this, dc, GetDomainByName(dc.Domain.Name), IsSiteServer: true, IsWritable: false));

            Site.UpdateDomainControllers(siteDomainControllers);
            Domains.ForEach(domain => domain.UpdateDomainControllers(siteDomainControllers.Where(dc => dc.Domain == domain)));
        }

        #endregion

        #region Domain Getters

        public bool TryGetDomainFromDistinguishedName(string DistinguishedName, out ADDomain Domain)
        {
            // Find closest match
            Domain = Domains.Where(d => DistinguishedName.EndsWith(d.DistinguishedName, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(d => d.DistinguishedName.Length).FirstOrDefault();

            return (Domain != null);
        }
        public ADDomain GetDomainFromDistinguishedName(string DistinguishedName)
        {
            if (!TryGetDomainFromDistinguishedName(DistinguishedName, out var domain))
                throw new ArgumentException($"The distinguished name is from an unknown domain: [{DistinguishedName}]", "DistinguishedName");
            return domain;
        }

        public bool TryGetDomainByNetBiosName(string NetBiosName, out ADDomain Domain)
        {
            Domain = Domains.FirstOrDefault(d => d.NetBiosName.Equals(NetBiosName, StringComparison.OrdinalIgnoreCase));
            return (Domain != null);
        }
        public ADDomain GetDomainByNetBiosName(string NetBiosName)
        {
            if (!TryGetDomainByNetBiosName(NetBiosName, out var domain))
                throw new ArgumentException($"The domain for specified NetBios name is unknown [{NetBiosName}]", "NetBiosName");
            return domain;
        }

        public bool TryGetDomainByName(string Name, out ADDomain Domain)
        {
            Domain = Domains.FirstOrDefault(d => d.Name.Equals(Name, StringComparison.OrdinalIgnoreCase));
            return (Domain != null);
        }
        public ADDomain GetDomainByName(string Name)
        {
            if (!TryGetDomainByName(Name, out var domain))
                throw new ArgumentException($"The domain for specified DNS name is unknown [{Name}]", "Name");
            return domain;
        }

        public bool TryGetDomainFromSecurityIdentifier(SecurityIdentifier SecurityIdentifier, out ADDomain Domain)
        {
            Domain = Domains.FirstOrDefault(d => d.SecurityIdentifier.IsEqualDomainSid(SecurityIdentifier));
            return (Domain != null);
        }
        public ADDomain GetDomainFromSecurityIdentifier(SecurityIdentifier SecurityIdentifier)
        {
            if (!TryGetDomainFromSecurityIdentifier(SecurityIdentifier, out var domain))
                throw new ArgumentException($"The domain for specified Security Identifier is unknown [{SecurityIdentifier.ToString()}]", "SecurityIdentifier");
            return domain;
        }

        public bool TryGetDomainFromId(string Id, out ADDomain Domain)
        {
            if (string.IsNullOrWhiteSpace(Id))
                throw new ArgumentNullException("Id");

            var slashIndex = Id.IndexOf('\\');

            if (slashIndex < 0)
                throw new ArgumentException($"The Id must include the Domain [{Id}]", "Id");

            return TryGetDomainByNetBiosName(Id.Substring(0, slashIndex), out Domain);
        }
        public ADDomain GetDomainFromId(string Id)
        {
            if (string.IsNullOrWhiteSpace(Id))
                throw new ArgumentNullException("Id");

            var slashIndex = Id.IndexOf('\\');

            if (slashIndex < 0)
                throw new ArgumentException($"The Id must include the Domain [{Id}]", "Id");

            return GetDomainByNetBiosName(Id.Substring(0, slashIndex));
        }

        #endregion

        public ADDirectoryEntry RetrieveDirectoryEntry(string DistinguishedName, string[] LoadProperties = null)
        {
            if (string.IsNullOrWhiteSpace(DistinguishedName))
                throw new ArgumentNullException("DistinguishedName");

            var d = GetDomainFromDistinguishedName(DistinguishedName);
            var dc = d.GetAvailableDomainController();

            return dc.RetrieveDirectoryEntry(DistinguishedName, LoadProperties);
        }

        #region Searching

        public IEnumerable<ADSearchResult> SearchEntireDirectory(string LdapFilter, string[] LoadProperties, int? ResultLimit = null)
        {
            var queries = Domains.Select(d => Tuple.Create(d, d.DistinguishedName));

            return SearchInternal(queries, LdapFilter, LoadProperties, ResultLimit);
        }

        public IEnumerable<ADSearchResult> SearchScope(string LdapFilter, string[] LoadProperties, int? ResultLimit = null)
        {
            var queries = Domains.SelectMany(
                d => d.SearchContainers ?? new List<string>() { d.DistinguishedName },
                (d, scope) => Tuple.Create(d, scope));

            return SearchInternal(queries, LdapFilter, LoadProperties, ResultLimit);
        }

        internal IEnumerable<ADSearchResult> SearchInternal(IEnumerable<Tuple<ADDomain, string>> Queries, string LdapFilter, string[] LoadProperties, int? ResultLimit)
        {
            var queries = Queries.ToList();

            switch (queries.Count)
            {
                case 0: // Nothing Queried                 
                    return Enumerable.Empty<ADSearchResult>();

                case 1: // Single-search
                    var querySingle = queries[0];
                    return querySingle.Item1.SearchInternal(querySingle.Item2, LdapFilter, LoadProperties, ResultLimit);

                default: // Multi-search - Parallelize

                    var queryTasks = queries.Select(query =>
                        Task<IEnumerable<ADSearchResult>>.Factory.StartNew(() =>
                             query.Item1.SearchInternal(query.Item2, LdapFilter, LoadProperties, ResultLimit))).ToArray();

                    // Block
                    Task.WaitAll(queryTasks);

                    var results = queryTasks.SelectMany(t => t.Result);
                    if (ResultLimit.HasValue)
                        results = results.Take(ResultLimit.Value);
                    return results;
            }
        }

        #endregion

        #region Configuration

        public void UpdateWildcardSearchSuffixOnly(DiscoDataContext Database, bool SearchWildcardSuffixOnly)
        {
            Database.DiscoConfiguration.ActiveDirectory.SearchWildcardSuffixOnly = SearchWildcardSuffixOnly;
            InitializeWildcardSearchSufixOnly(SearchWildcardSuffixOnly);
        }

        private void InitializeWildcardSearchSufixOnly(bool SearchWildcardSuffixOnly)
        {
            if (SearchWildcardSuffixOnly)
            {
                ADGroup.LdapSearchFilterTemplate = "(&(objectCategory=Group)(|(sAMAccountName={0}*)(name={0}*)(cn={0}*)))";
                ADUserAccount.LdapSearchFilterTemplate = "(&(objectCategory=Person)(objectClass=user)(|(sAMAccountName={0}*)(displayName={0}*)(sn={0}*)(givenName={0}*)))";
            }
            else
            {
                ADGroup.LdapSearchFilterTemplate = "(&(objectCategory=Group)(|(sAMAccountName=*{0}*)(name=*{0}*)(cn=*{0}*)))";
                ADUserAccount.LdapSearchFilterTemplate = "(&(objectCategory=Person)(objectClass=user)(|(sAMAccountName=*{0}*)(displayName=*{0}*)(sn=*{0}*)(givenName=*{0}*)))";
            }
        }

        public bool UpdateSearchAllServers(DiscoDataContext Database, bool SearchAllServers)
        {
            if (SearchAllServers == false)
            {
                Database.DiscoConfiguration.ActiveDirectory.SearchAllServers = false;
                searchAllServers = false;
                return true;
            }
            else
            {
                var allServers = ADDiscoverServers.LoadAllServersSync();
                if (allServers.Count <= ActiveDirectory.MaxAllServerSearch)
                {
                    Database.DiscoConfiguration.ActiveDirectory.SearchAllServers = true;
                    searchAllServers = true;
                    return true;
                }
                else
                {
                    Database.DiscoConfiguration.ActiveDirectory.SearchAllServers = false;
                    searchAllServers = false;
                    return false;
                }
            }
        }

        public void UpdateSearchContainers(DiscoDataContext Database, IEnumerable<string> Containers)
        {
            Dictionary<string, List<string>> searchContainers = null;

            if (Containers != null)
            {
                searchContainers = Containers
                    .Where(c => !string.IsNullOrWhiteSpace(c))
                    .Distinct()
                    .Select(c =>
                    {
                        if (TryGetDomainFromDistinguishedName(c, out var d))
                            return Tuple.Create(d, c);
                        else
                            return null;
                    }).Where(i => i != null)
                    .GroupBy(i => i.Item1)
                    .ToDictionary(g => g.Key.Name.ToLower(), g => g.Select(i => i.Item2).ToList());
            }

            if (searchContainers == null || searchContainers.Count == 0)
            {
                Database.DiscoConfiguration.ActiveDirectory.SearchContainers = null;
                ReinitializeSearchContainers(null);
            }
            else
            {
                Database.DiscoConfiguration.ActiveDirectory.SearchContainers = searchContainers;
                ReinitializeSearchContainers(searchContainers);
            }
        }

        private void ReinitializeSearchContainers(Dictionary<string, List<string>> Containers)
        {
            if (Containers == null)
            {
                // No search restrictions (search entire domain)
                foreach (var domain in Domains)
                    domain.UpdateSearchEntireDomain();
            }
            else
            {
                // Restrict search containers
                var searchContainerDomains = Containers.Join(Domains, ok => ok.Key, ik => ik.Name, (o, i) => Tuple.Create(o, i), StringComparer.OrdinalIgnoreCase);
                foreach (var domainContainers in searchContainerDomains)
                    domainContainers.Item2.UpdateSearchContainers(domainContainers.Item1.Value);

                // Ignore domains without configured containers
                var unconfiguredContainers = Domains.Except(searchContainerDomains.Select(sc => sc.Item2));
                foreach (var domain in unconfiguredContainers)
                    domain.UpdateSearchContainers(new List<string>());
            }
        }

        #endregion
    }
}
