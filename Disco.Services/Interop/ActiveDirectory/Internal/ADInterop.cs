using Disco.Data.Repository;
using Disco.Models.Interop.ActiveDirectory;
using Disco.Services.Tasks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Services.Interop.ActiveDirectory.Internal
{
    internal static class ADInterop
    {
        public static List<ActiveDirectoryDomain> Domains { get; private set; }
        public static ActiveDirectoryDomain PrimaryDomain { get; private set; }
        public static ActiveDirectorySite Site { get; private set; }
        internal static List<string> _ForestServers { get; set; }
        private static bool _SearchEntireForest { get; set; }
        private static bool _Initialized = false;
        private static object _InitializeLock = new object();

        #region Initialization

        public static void Initialize(DiscoDataContext Database)
        {
            if (!_Initialized)
            {
                lock (_InitializeLock)
                {
                    if (!_Initialized)
                    {
                        _SearchEntireForest = Database.DiscoConfiguration.ActiveDirectory.SearchEntireForest ?? true; // Default True

                        using (var computerDomain = Domain.GetComputerDomain())
                        {
                            PrimaryDomain = GetDomainInternal(computerDomain, Database);

                            Domains = computerDomain.Forest.Domains.Cast<Domain>().Select(d =>
                            {
                                if (d.Name == PrimaryDomain.DnsName)
                                    return PrimaryDomain;
                                else
                                    return GetDomainInternal(d, Database);
                            }).ToList();
                        }

                        Site = ActiveDirectorySite.GetComputerSite();
                    }
                }
            }
        }

        private static ActiveDirectoryDomain GetDomainInternal(Domain d, DiscoDataContext Database)
        {
            string ldapPath = string.Format("LDAP://{0}/", d.Name);
            string defaultNamingContext;
            string configurationNamingContext;
            string netBiosName;

            using (var adRootDSE = new DirectoryEntry(ldapPath + "RootDSE"))
            {
                defaultNamingContext = adRootDSE.Properties["defaultNamingContext"][0].ToString();
                configurationNamingContext = adRootDSE.Properties["configurationNamingContext"][0].ToString();
            }

            using (var configSearchRoot = new DirectoryEntry(ldapPath + "CN=Partitions," + configurationNamingContext))
            {
                var configSearchFilter = string.Format("(&(objectcategory=Crossref)(dnsRoot={0})(netBIOSName=*))", d.Name);
                var configSearchLoadProperites = new string[] { "NetBIOSName" };

                using (var configSearcher = new DirectorySearcher(configSearchRoot, configSearchFilter, configSearchLoadProperites, System.DirectoryServices.SearchScope.OneLevel))
                {
                    SearchResult configResult = configSearcher.FindOne();

                    if (configResult != null)
                        netBiosName = configResult.Properties["NetBIOSName"][0].ToString();
                    else
                        netBiosName = null;
                }
            }

            // Search Containers
            var searchContainersAll = Database.DiscoConfiguration.ActiveDirectory.SearchContainers;
            List<string> searchContainers = null;

            if (searchContainersAll == null || searchContainersAll.Count == 0 || !searchContainersAll.TryGetValue(d.Name.ToLower(), out searchContainers))
                searchContainers = new List<string>() { defaultNamingContext }; // No search constraints set - search entire tree

            return new ActiveDirectoryDomain(d.Name, netBiosName, defaultNamingContext, searchContainers);
        }

        public static void UpdateSearchContainers(DiscoDataContext Database, IEnumerable<string> Containers)
        {
            Dictionary<string, List<string>> searchContainers = null;

            if (Containers != null)
            {
                searchContainers = Containers
                    .Where(c => !string.IsNullOrWhiteSpace(c))
                    .Distinct()
                    .Select(c =>
                    {
                        ActiveDirectoryDomain d;
                        if (TryGetDomainByDistinguishedName(c, out d))
                            return Tuple.Create(d, c);
                        else
                            return null;
                    }).Where(i => i != null)
                    .GroupBy(i => i.Item1)
                    .ToDictionary(g => g.Key.DnsName.ToLower(), g => g.Select(i => i.Item2).ToList());
            }

            if (searchContainers == null || searchContainers.Count == 0)
            {
                Database.DiscoConfiguration.ActiveDirectory.SearchContainers = null;

                // No search constraints set - search entire tree
                Domains.ForEach(d => d.UpdateSearchContainers(new string[] { d.DistinguishedName }));
            }
            else
            {
                Database.DiscoConfiguration.ActiveDirectory.SearchContainers = searchContainers;

                Domains.ForEach(d =>
                {
                    List<string> domainContainers;
                    if (searchContainers.TryGetValue(d.DnsName.ToLower(), out domainContainers))
                        d.UpdateSearchContainers(domainContainers);
                    else
                        d.UpdateSearchContainers(Enumerable.Empty<string>());
                });
            }
        }

        public static bool UpdateSearchEntireForest(DiscoDataContext Database, bool SearchEntireForest)
        {
            if (SearchEntireForest == false)
            {
                Database.DiscoConfiguration.ActiveDirectory.SearchEntireForest = false;
                ADInterop._SearchEntireForest = false;
                return true;
            }
            else
            {
                var forestServers = LoadForestServers();
                if (forestServers.Count <= ActiveDirectory.MaxForestServerSearch)
                {
                    Database.DiscoConfiguration.ActiveDirectory.SearchEntireForest = true;
                    ADInterop._SearchEntireForest = true;
                    return true;
                }
                else
                {
                    Database.DiscoConfiguration.ActiveDirectory.SearchEntireForest = false;
                    ADInterop._SearchEntireForest = false;
                    return false;
                }
            }
        }

        #endregion

        #region Domain Getters

        public static bool TryGetDomainByDistinguishedName(string DistinguishedName, out ActiveDirectoryDomain Domain)
        {
            // Find closest match
            Domain = ADInterop.Domains.Where(d => DistinguishedName.EndsWith(d.DistinguishedName, StringComparison.InvariantCultureIgnoreCase))
                .OrderByDescending(d => d.DistinguishedName.Length).FirstOrDefault();

            return (Domain != null);
        }
        public static ActiveDirectoryDomain GetDomainByDistinguishedName(string DistinguishedName)
        {
            ActiveDirectoryDomain domain;
            if (!TryGetDomainByDistinguishedName(DistinguishedName, out domain))
                throw new ArgumentException(string.Format("The domain is unknown distinguished name: [{0}]", DistinguishedName), "Id");
            return domain;
        }

        public static bool TryGetDomainByNetBiosName(string NetBiosName, out ActiveDirectoryDomain Domain)
        {
            Domain = ADInterop.Domains.FirstOrDefault(d => d.NetBiosName.Equals(NetBiosName, StringComparison.InvariantCultureIgnoreCase));
            return (Domain != null);
        }
        public static ActiveDirectoryDomain GetDomainByNetBiosName(string NetBiosName)
        {
            ActiveDirectoryDomain domain;
            if (!TryGetDomainByNetBiosName(NetBiosName, out domain))
                throw new ArgumentException(string.Format("The specified domain is unknown [{0}]", NetBiosName), "Id");
            return domain;
        }

        public static bool TryGetDomainByDnsName(string DnsName, out ActiveDirectoryDomain Domain)
        {
            Domain = ADInterop.Domains.FirstOrDefault(d => d.DnsName.Equals(DnsName, StringComparison.InvariantCultureIgnoreCase));
            return (Domain != null);
        }
        public static ActiveDirectoryDomain GetDomainByDnsName(string DnsName)
        {
            ActiveDirectoryDomain domain;
            if (!TryGetDomainByDnsName(DnsName, out domain))
                throw new ArgumentException(string.Format("The specified domain is unknown [{0}]", DnsName), "Id");
            return domain;
        }

        public static bool TryGetDomainFromId(string Id, out ActiveDirectoryDomain Domain)
        {
            if (string.IsNullOrWhiteSpace(Id))
                throw new ArgumentNullException("Id");

            var idSplit = UserExtensions.SplitUserId(Id);

            if (idSplit.Item1 == null)
                throw new ArgumentException(string.Format("The Id must include the Domain [{0}]", Id), "Id");

            if (string.IsNullOrWhiteSpace(idSplit.Item1))
                throw new ArgumentException(string.Format("The Id Domain was empty [{0}]", Id), "Id");
            if (string.IsNullOrWhiteSpace(idSplit.Item2))
                throw new ArgumentException(string.Format("The Id Name was empty [{0}]", Id), "Id");

            return TryGetDomainByNetBiosName(idSplit.Item1, out Domain);
        }
        public static ActiveDirectoryDomain GetDomainFromId(string Id)
        {
            if (string.IsNullOrWhiteSpace(Id))
                throw new ArgumentNullException("Id");

            var idSplit = UserExtensions.SplitUserId(Id);

            if (idSplit.Item1 == null)
                throw new ArgumentException(string.Format("The Id must include the Domain [{0}]", Id), "Id");

            if (string.IsNullOrWhiteSpace(idSplit.Item1))
                throw new ArgumentException(string.Format("The Id Domain was empty [{0}]", Id), "Id");
            if (string.IsNullOrWhiteSpace(idSplit.Item2))
                throw new ArgumentException(string.Format("The Id Name was empty [{0}]", Id), "Id");

            return GetDomainByNetBiosName(idSplit.Item1);
        }

        public static List<string> LoadForestServers()
        {
            if (_ForestServers == null)
            {
                lock (_InitializeLock)
                {
                    if (_ForestServers == null)
                    {
                        var status = ADDiscoverForestServers.ScheduleNow();
                        status.CompletionTask.Wait();
                    }
                }
            }
            return _ForestServers;
        }
        public static Task<List<string>> LoadForestServersAsync()
        {
            if (_ForestServers != null)
                return Task.FromResult(_ForestServers);

            lock (_InitializeLock)
            {
                if (_ForestServers != null)
                    return Task.FromResult(_ForestServers);

                // Invoke Scheduled Task
                var status = ADDiscoverForestServers.ScheduleNow();

                return status.CompletionTask.ContinueWith(t =>
                {
                    return ADInterop._ForestServers.ToList();
                });
            }
        }

        public static bool SearchEntireForest
        {
            get
            {
                if (_ForestServers != null && _ForestServers.Count > ActiveDirectory.MaxForestServerSearch)
                    return false; // Never

                return _SearchEntireForest;
            }
        }

        #endregion

        #region Searching

        public static IEnumerable<ActiveDirectorySearchResult> SearchAll(string LdapFilter, string[] LoadProperties)
        {
            return SearchAll(Domains, LdapFilter, LoadProperties);
        }
        public static IEnumerable<ActiveDirectorySearchResult> SearchAll(string LdapFilter, int ResultLimit, string[] LoadProperties)
        {
            return SearchAll(Domains, LdapFilter, ResultLimit, LoadProperties);
        }
        public static IEnumerable<ActiveDirectorySearchResult> SearchAll(IEnumerable<Tuple<ActiveDirectoryDomain, DomainController>> DomainsWithController, string LdapFilter, string[] LoadProperties)
        {
            return SearchAll(DomainsWithController, LdapFilter, null, LoadProperties);
        }
        public static IEnumerable<ActiveDirectorySearchResult> SearchAll(IEnumerable<ActiveDirectoryDomain> Domains, string LdapFilter, string[] LoadProperties)
        {
            return SearchAll(Domains, LdapFilter, null, LoadProperties);
        }
        public static IEnumerable<ActiveDirectorySearchResult> SearchAll(ActiveDirectoryDomain Domain, DomainController DomainController, string LdapFilter, string[] LoadProperties)
        {
            return SearchAll(Domain, DomainController, LdapFilter, null, LoadProperties);
        }
        public static IEnumerable<ActiveDirectorySearchResult> SearchAll(ActiveDirectoryDomain Domain, string LdapFilter, string[] LoadProperties)
        {
            return SearchAll(Domain, LdapFilter, null, LoadProperties);
        }
        public static IEnumerable<ActiveDirectorySearchResult> SearchAll(IEnumerable<Tuple<ActiveDirectoryDomain, DomainController>> DomainsWithController, string LdapFilter, int? ResultLimit, string[] LoadProperties)
        {
            var query = DomainsWithController
                .SelectMany(d => SearchAll(d.Item1, d.Item2, LdapFilter, ResultLimit, LoadProperties));

            if (ResultLimit.HasValue)
                query = query.Take(ResultLimit.Value);

            return query.ToList();
        }
        public static IEnumerable<ActiveDirectorySearchResult> SearchAll(IEnumerable<ActiveDirectoryDomain> Domains, string LdapFilter, int? ResultLimit, string[] LoadProperties)
        {
            var query = Domains
                .SelectMany(domain => SearchAll(domain, LdapFilter, ResultLimit, LoadProperties));

            if (ResultLimit.HasValue)
                query = query.Take(ResultLimit.Value);

            return query.ToList();
        }
        public static IEnumerable<ActiveDirectorySearchResult> SearchAll(ActiveDirectoryDomain Domain, DomainController DomainController, string LdapFilter, int? ResultLimit, string[] LoadProperties)
        {
            return SearchDomain(Domain, DomainController, null, LdapFilter, ResultLimit, LoadProperties);
        }
        public static IEnumerable<ActiveDirectorySearchResult> SearchAll(ActiveDirectoryDomain Domain, string LdapFilter, int? ResultLimit, string[] LoadProperties)
        {
            return SearchDomain(Domain, null, LdapFilter, ResultLimit, LoadProperties);
        }
        public static IEnumerable<ActiveDirectorySearchResult> SearchScope(string LdapFilter, string[] LoadProperties)
        {
            return SearchScope(Domains, LdapFilter, LoadProperties);
        }
        public static IEnumerable<ActiveDirectorySearchResult> SearchScope(string LdapFilter, int ResultLimit, string[] LoadProperties)
        {
            return SearchScope(Domains, LdapFilter, ResultLimit, LoadProperties);
        }
        public static IEnumerable<ActiveDirectorySearchResult> SearchScope(IEnumerable<ActiveDirectoryDomain> Domains, string LdapFilter, string[] LoadProperties)
        {
            return SearchScope(Domains, LdapFilter, null, LoadProperties);
        }
        public static IEnumerable<ActiveDirectorySearchResult> SearchScope(IEnumerable<Tuple<ActiveDirectoryDomain, DomainController>> DomainsWithController, string LdapFilter, string[] LoadProperties)
        {
            return SearchScope(DomainsWithController, LdapFilter, null, LoadProperties);
        }
        public static IEnumerable<ActiveDirectorySearchResult> SearchScope(ActiveDirectoryDomain Domain, string LdapFilter, string[] LoadProperties)
        {
            return SearchScope(Domain, LdapFilter, null, LoadProperties);
        }
        public static IEnumerable<ActiveDirectorySearchResult> SearchScope(ActiveDirectoryDomain Domain, DomainController DomainController, string LdapFilter, string[] LoadProperties)
        {
            return SearchScope(Domain, DomainController, LdapFilter, null, LoadProperties);
        }
        public static IEnumerable<ActiveDirectorySearchResult> SearchScope(IEnumerable<Tuple<ActiveDirectoryDomain, DomainController>> DomainsWithController, string LdapFilter, int? ResultLimit, string[] LoadProperties)
        {
            var query = DomainsWithController
                .SelectMany(d => SearchScope(d.Item1, d.Item2, LdapFilter, ResultLimit, LoadProperties));

            if (ResultLimit.HasValue)
                query = query.Take(ResultLimit.Value);

            return query.ToList();
        }
        public static IEnumerable<ActiveDirectorySearchResult> SearchScope(IEnumerable<ActiveDirectoryDomain> Domains, string LdapFilter, int? ResultLimit, string[] LoadProperties)
        {
            var query = Domains
                .SelectMany(domain => SearchScope(domain, LdapFilter, ResultLimit, LoadProperties));

            if (ResultLimit.HasValue)
                query = query.Take(ResultLimit.Value);

            return query.ToList();
        }
        public static IEnumerable<ActiveDirectorySearchResult> SearchScope(ActiveDirectoryDomain Domain, string LdapFilter, int? ResultLimit, string[] LoadProperties)
        {
            return SearchScope(Domain, null, LdapFilter, ResultLimit, LoadProperties);
        }
        public static IEnumerable<ActiveDirectorySearchResult> SearchScope(ActiveDirectoryDomain Domain, DomainController DomainController, string LdapFilter, int? ResultLimit, string[] LoadProperties)
        {
            if (Domain.SearchContainers == null)
                return Enumerable.Empty<ActiveDirectorySearchResult>();

            var query = Domain.SearchContainers
                .SelectMany(container => SearchDomain(Domain, DomainController, container, LdapFilter, ResultLimit, LoadProperties));

            if (ResultLimit.HasValue)
                query = query.Take(ResultLimit.Value);

            return query.ToList();
        }

        public static IEnumerable<ActiveDirectorySearchResult> SearchDomain(ActiveDirectoryDomain Domain, string SearchRoot, string LdapFilter, int? ResultLimit, string[] LoadProperties)
        {
            return SearchDomain(Domain, null, SearchRoot, LdapFilter, ResultLimit, LoadProperties);
        }
        public static IEnumerable<ActiveDirectorySearchResult> SearchDomain(ActiveDirectoryDomain Domain, DomainController DomainController, string SearchRoot, string LdapFilter, int? ResultLimit, string[] LoadProperties)
        {
            string ldapServer = DomainController == null ? Domain.DnsName : DomainController.Name;
            string searchRoot = SearchRoot ?? Domain.DistinguishedName;
            string ldapPath = string.Format(@"LDAP://{0}/{1}", ldapServer, searchRoot);

            using (DirectoryEntry rootEntry = new DirectoryEntry(ldapPath))
            {
                using (DirectorySearcher searcher = new DirectorySearcher(rootEntry, LdapFilter, LoadProperties, System.DirectoryServices.SearchScope.Subtree))
                {
                    searcher.PageSize = 500;

                    if (ResultLimit.HasValue)
                        searcher.SizeLimit = ResultLimit.Value;
                    return searcher.FindAll().Cast<SearchResult>().Select(result => new ActiveDirectorySearchResult()
                    {
                        Domain = Domain,
                        SearchRoot = searchRoot,
                        Result = result,
                    });
                }
            }
        }

        #endregion

        public static string OfflineDomainJoinProvision(ActiveDirectoryDomain Domain, DomainController DomainController, string ComputerSamAccountName, string OrganisationalUnit, out string DiagnosticInformation)
        {
            StringBuilder diagnosticInfo = new StringBuilder();
            string DJoinResult = null;

            if (!string.IsNullOrWhiteSpace(ComputerSamAccountName))
                ComputerSamAccountName = ComputerSamAccountName.TrimEnd('$');
            if (string.IsNullOrWhiteSpace(ComputerSamAccountName) || ComputerSamAccountName.Length > 24)
                throw new System.ArgumentException("Invalid Computer Name; > 0 and <= 24", "ComputerName");

            // Ensure Specified OU Exists
            if (!string.IsNullOrEmpty(OrganisationalUnit))
            {
                try
                {
                    using (var deOU = DomainController.RetrieveDirectoryEntry(OrganisationalUnit))
                    {
                        if (deOU == null)
                            throw new Exception(string.Format("OU's Directory Entry couldn't be found at [{0}]", OrganisationalUnit));
                    }
                }
                catch (Exception ex)
                {
                    throw new ArgumentException(string.Format("An error occurred while trying to locate the specified OU: {0}", OrganisationalUnit), "OrganisationalUnit", ex);
                }
            }

            string tempFileName = System.IO.Path.GetTempFileName();
            string argumentOU = (!string.IsNullOrWhiteSpace(OrganisationalUnit)) ? string.Format(" /MACHINEOU \"{0}\"", OrganisationalUnit) : string.Empty;
            string arguments = string.Format("/PROVISION /DOMAIN \"{0}\" /DCNAME \"{1}\" /MACHINE \"{2}\"{3} /REUSE /SAVEFILE \"{4}\"",
                Domain.DnsName,
                DomainController.Name,
                ComputerSamAccountName,
                argumentOU,
                tempFileName
            );
            ProcessStartInfo commandStarter = new ProcessStartInfo("DJOIN.EXE", arguments)
            {
                CreateNoWindow = true,
                ErrorDialog = false,
                LoadUserProfile = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };
            diagnosticInfo.AppendFormat("{0} {1}", "DJOIN.EXE", arguments);
            diagnosticInfo.AppendLine();

            string stdOutput;
            string stdError;
            using (Process commandProc = Process.Start(commandStarter))
            {
                commandProc.WaitForExit(20000);
                stdOutput = commandProc.StandardOutput.ReadToEnd();
                stdError = commandProc.StandardError.ReadToEnd();
            }
            if (!string.IsNullOrWhiteSpace(stdOutput))
                diagnosticInfo.AppendLine(stdOutput);
            if (!string.IsNullOrWhiteSpace(stdError))
                diagnosticInfo.AppendLine(stdError);

            if (System.IO.File.Exists(tempFileName))
            {
                DJoinResult = System.Convert.ToBase64String(System.IO.File.ReadAllBytes(tempFileName));
                System.IO.File.Delete(tempFileName);
            }
            if (string.IsNullOrWhiteSpace(DJoinResult))
                throw new System.InvalidOperationException(string.Format("Domain Join Unsuccessful{0}Error: {1}{0}Output: {2}", System.Environment.NewLine, stdError, stdOutput));

            DiagnosticInformation = diagnosticInfo.ToString();

            return DJoinResult;
        }

        public static DirectoryEntry RetrieveDirectoryEntry(string DistinguishedName, out ActiveDirectoryDomain Domain)
        {
            if (string.IsNullOrWhiteSpace(DistinguishedName))
                throw new ArgumentNullException("DistinguishedName");

            // Find Domain
            var domain = GetDomainByDistinguishedName(DistinguishedName);

            Domain = domain;

            return new DirectoryEntry(string.Format(@"LDAP://{0}/{1}", domain.DnsName, DistinguishedName));
        }

        public static DomainController RetrieveWritableDomainController(this ActiveDirectoryDomain domain)
        {
            var adContext = new DirectoryContext(DirectoryContextType.Domain, domain.DnsName);
            using (var adDomain = Domain.GetDomain(adContext))
            {
                return adDomain.FindDomainController(LocatorOptions.WriteableRequired);
            }
        }

        public static DirectoryEntry RetrieveDirectoryEntry(this DomainController domainController, string DistinguishedName)
        {
            return new DirectoryEntry(string.Format(@"LDAP://{0}/{1}", domainController.Name, DistinguishedName));
        }

        public static void DeleteObjectRecursive(this DirectoryEntry directoryEntry)
        {
            DeleteObjectRecursiveInternal(directoryEntry);

            using (var deParent = directoryEntry.Parent)
            {
                deParent.Children.Remove(directoryEntry);
            }
        }
        private static void DeleteObjectRecursiveInternal(DirectoryEntry directoryEntry)
        {
            List<DirectoryEntry> children = directoryEntry.Children.Cast<DirectoryEntry>().ToList();

            foreach (var child in children)
            {
                DeleteObjectRecursive(child);
                directoryEntry.Children.Remove(child);
                child.Dispose();
            }
        }

        #region Helpers

        internal static string ConvertBytesToSDDLString(byte[] SID)
        {
            SecurityIdentifier sID = new SecurityIdentifier(SID, 0);

            return sID.ToString();
        }

        internal static byte[] ConvertSDDLStringToBytes(string SidSsdlString)
        {
            SecurityIdentifier sID = new SecurityIdentifier(SidSsdlString);

            var sidBytes = new byte[sID.BinaryLength];

            sID.GetBinaryForm(sidBytes, 0);

            return sidBytes;
        }

        internal static byte[] BuildPrimaryGroupSid(byte[] UserSID, int PrimaryGroupId)
        {
            var groupSid = (byte[])UserSID.Clone();

            int ridOffset = groupSid.Length - 4;
            int groupId = PrimaryGroupId;
            for (int i = 0; i < 4; i++)
            {
                groupSid[ridOffset + i] = (byte)(groupId & 0xFF);
                groupId >>= 8;
            }

            return groupSid;
        }

        internal static string ConvertBytesToBinarySidString(byte[] SID)
        {
            StringBuilder escapedSid = new StringBuilder();

            foreach (var sidByte in SID)
            {
                escapedSid.Append('\\');
                escapedSid.Append(sidByte.ToString("x2"));
            }

            return escapedSid.ToString();
        }

        internal static string EscapeLdapQuery(string query)
        {
            return query.Replace("*", "\\2a").Replace("(", "\\28").Replace(")", "\\29").Replace("\\", "\\5c").Replace("NUL", "\\00").Replace("/", "\\2f");
        }
        internal static string FormatGuidForLdapQuery(System.Guid g)
        {
            checked
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                byte[] array = g.ToByteArray();
                for (int i = 0; i < array.Length; i++)
                {
                    byte b = array[i];
                    sb.Append("\\");
                    sb.Append(b.ToString("X2"));
                }
                return sb.ToString();
            }
        }

        #endregion
    }
}
