using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Principal;
using System.Text;

namespace Disco.Services.Interop.ActiveDirectory
{
    public class ADDomainController
    {
        private const string LdapPathTemplate = @"LDAP://{0}/{1}";
        private ActiveDirectoryContext context;

        public DomainController DomainController { get; private set; }
        public ADDomain Domain { get; private set; }
        public string Name { get; private set; }
        public string SiteName { get; private set; }

        public bool IsSiteServer { get; private set; }
        public bool IsWritable { get; internal set; }
        public DateTime? AvailableWhen { get; private set; }

        public bool IsAvailable
        {
            get
            {
                var aw = AvailableWhen;
                if (aw.HasValue && aw.Value < DateTime.Now)
                    AvailableWhen = null;

                return !aw.HasValue;
            }
            internal set
            {
                if (value)
                    AvailableWhen = null;
                else
                    AvailableWhen = DateTime.Now.AddMinutes(ActiveDirectory.DomainControllerUnavailableMinutes);
            }
        }

        public ADDomainController(ActiveDirectoryContext Context, DomainController DomainController, ADDomain Domain, bool IsSiteServer, bool IsWritable)
        {
            context = Context;

            this.Domain = Domain;
            this.DomainController = DomainController;

            Name = DomainController.Name;
            SiteName = DomainController.SiteName;

            this.IsSiteServer = IsSiteServer;
            this.IsWritable = IsWritable;

            AvailableWhen = null;
        }

        public ADDirectoryEntry RetrieveDirectoryEntry(string DistinguishedName, string[] LoadProperties = null)
        {
            if (string.IsNullOrWhiteSpace(DistinguishedName))
                throw new ArgumentNullException("DistinguishedName");

            if (!DistinguishedName.EndsWith(Domain.DistinguishedName, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException($"The Distinguished Name ({DistinguishedName}) isn't a member of this domain [{Domain.Name}]", "DistinguishedName");

            var entry = new DirectoryEntry(string.Format(LdapPathTemplate, Name, ADHelpers.EscapeDistinguishedName(DistinguishedName)));

            if (LoadProperties != null)
                entry.RefreshCache(LoadProperties);

            return new ADDirectoryEntry(Domain, this, entry);
        }

        #region Searching
        public IEnumerable<ADSearchResult> SearchEntireDomain(string LdapFilter, string[] LoadProperties, int? ResultLimit = null)
        {
            return SearchInternal(Domain.DistinguishedName, LdapFilter, LoadProperties, ResultLimit);
        }

        public IEnumerable<ADSearchResult> SearchScope(string LdapFilter, string[] LoadProperties, int? ResultLimit = null)
        {
            var searchScope = Domain.SearchContainers;

            // No scope set, search entire domain
            if (searchScope == null)
                return SearchEntireDomain(LdapFilter, LoadProperties, ResultLimit);

            // Ignore domain
            if (searchScope.Count == 0)
                return Enumerable.Empty<ADSearchResult>();

            // Multi-search
            var results = searchScope.SelectMany(scope => SearchInternal(scope, LdapFilter, LoadProperties, ResultLimit));
            if (ResultLimit.HasValue)
                results = results.Take(ResultLimit.Value);
            return results;
        }

        internal IEnumerable<ADSearchResult> SearchInternal(string SearchRoot, string LdapFilter, string[] LoadProperties, int? ResultLimit)
        {
            if (string.IsNullOrEmpty(SearchRoot))
                throw new ArgumentNullException("SearchRoot");
            if (string.IsNullOrEmpty(LdapFilter))
                throw new ArgumentNullException("LdapFilter");
            if (ResultLimit.HasValue && ResultLimit.Value < 1)
                throw new ArgumentOutOfRangeException("ResultLimit", "The ResultLimit must be 1 or greater");

            using (ADDirectoryEntry rootEntry = RetrieveDirectoryEntry(SearchRoot))
            {
                using (DirectorySearcher searcher = new DirectorySearcher(rootEntry.Entry, LdapFilter, LoadProperties, System.DirectoryServices.SearchScope.Subtree))
                {
                    searcher.PageSize = 500;

                    if (ResultLimit.HasValue)
                        searcher.SizeLimit = ResultLimit.Value;

                    return searcher.FindAll().Cast<SearchResult>().Select(result => new ADSearchResult(Domain, this, SearchRoot, LdapFilter, result));
                }
            }
        }
        #endregion

        #region AD Objects

        #region User Accounts
        public ADUserAccount RetrieveADUserAccount(string Id, string[] AdditionalProperties = null)
        {
            string[] loadProperites = (AdditionalProperties != null && AdditionalProperties.Length > 0)
                ? ADUserAccount.LoadProperties.Concat(AdditionalProperties).ToArray()
                : ADUserAccount.LoadProperties;

            var result = RetrieveBySamAccountName(Id, ADUserAccount.LdapSamAccountNameFilterTemplate, loadProperites);

            if (result == null)
                return null;
            else
                return result.AsADUserAccount(false, AdditionalProperties);
        }
        #endregion

        #region Machine Accounts
        public ADMachineAccount RetrieveADMachineAccount(string Id, Guid? UUIDNetbootGUID, Guid? MacAddressNetbootGUID, string[] AdditionalProperties = null)
        {
            if (string.IsNullOrWhiteSpace(Id))
                throw new ArgumentNullException("Id");

            // Add $ identifier for machine accounts
            if (!Id.EndsWith("$"))
                Id += "$";

            string[] loadProperites = (AdditionalProperties != null && AdditionalProperties.Length > 0)
                ? ADMachineAccount.LoadProperties.Concat(AdditionalProperties).ToArray()
                : ADMachineAccount.LoadProperties;

            ADSearchResult adResult;

            adResult = RetrieveBySamAccountName(Id, ADMachineAccount.LdapSamAccountNameFilterTemplate, loadProperites);
            if (adResult == null && (UUIDNetbootGUID.HasValue || MacAddressNetbootGUID.HasValue))
            {
                string ldapFilter;
                if (UUIDNetbootGUID.HasValue && MacAddressNetbootGUID.HasValue)
                {
                    ldapFilter = string.Format(ADMachineAccount.LdapNetbootGuidDoubleFilterTemplate, UUIDNetbootGUID.Value.ToLdapQueryFormat(), MacAddressNetbootGUID.Value.ToLdapQueryFormat());
                }
                else if (UUIDNetbootGUID.HasValue)
                {
                    ldapFilter = string.Format(ADMachineAccount.LdapNetbootGuidSingleFilterTemplate, UUIDNetbootGUID.Value.ToLdapQueryFormat());
                }
                else // MacAddressNetbootGUID.HasValue
                {
                    ldapFilter = string.Format(ADMachineAccount.LdapNetbootGuidSingleFilterTemplate, MacAddressNetbootGUID.Value.ToLdapQueryFormat());
                }
                adResult = SearchEntireDomain(ldapFilter, loadProperites, ActiveDirectory.SingleSearchResult).FirstOrDefault();
            }

            if (adResult != null)
                return adResult.AsADMachineAccount(AdditionalProperties);
            else
                return null; // Not Found
        }
        public ADMachineAccount RetrieveADMachineAccount(string Id, string[] AdditionalProperties = null)
        {
            return RetrieveADMachineAccount(Id, null, null, AdditionalProperties);
        }
        public ADMachineAccount RetrieveADMachineAccount(string Id, Guid? NetbootGUID, string[] AdditionalProperties = null)
        {
            return RetrieveADMachineAccount(Id, NetbootGUID, null, AdditionalProperties);
        }
        #endregion

        #region Groups
        public ADGroup RetrieveADGroup(string Id, string[] AdditionalProperties = null)
        {
            string[] loadProperites = (AdditionalProperties != null && AdditionalProperties.Length > 0)
                ? ADGroup.LoadProperties.Concat(AdditionalProperties).ToArray()
                : ADGroup.LoadProperties;

            var result = RetrieveBySamAccountName(Id, ADGroup.LdapSamAccountNameFilterTemplate, loadProperites);

            if (result == null)
                return null;
            else
                return result.AsADGroup(AdditionalProperties);
        }
        public ADGroup RetrieveADGroupByDistinguishedName(string DistinguishedName, string[] AdditionalProperties = null)
        {
            string[] loadProperites = (AdditionalProperties != null && AdditionalProperties.Length > 0)
                ? ADGroup.LoadProperties.Concat(AdditionalProperties).ToArray()
                : ADGroup.LoadProperties;

            using (var groupEntry = RetrieveDirectoryEntry(DistinguishedName, loadProperites))
            {
                if (groupEntry == null)
                    return null;

                return groupEntry.AsADGroup(AdditionalProperties);
            }
        }
        public ADGroup RetrieveADGroupWithSecurityIdentifier(SecurityIdentifier SecurityIdentifier, string[] AdditionalProperties = null)
        {
            if (SecurityIdentifier == null)
                throw new ArgumentNullException("SecurityIdentifier");
            if (!SecurityIdentifier.IsEqualDomainSid(Domain.SecurityIdentifier))
                throw new ArgumentException($"The specified Security Identifier [{SecurityIdentifier.ToString()}] does not belong to this domain [{Domain.Name}]", "SecurityIdentifier");

            var sidBinaryString = SecurityIdentifier.ToBinaryString();

            string ldapFilter = string.Format(ADGroup.LdapSecurityIdentifierFilterTemplate, sidBinaryString);
            string[] loadProperites = (AdditionalProperties != null && AdditionalProperties.Length > 0)
                ? ADGroup.LoadProperties.Concat(AdditionalProperties).ToArray()
                : ADGroup.LoadProperties;

            var result = SearchEntireDomain(ldapFilter, loadProperites, ActiveDirectory.SingleSearchResult).FirstOrDefault();
            if (result == null)
                return null;
            else
                return result.AsADGroup(AdditionalProperties);
        }
        #endregion

        #region Object
        private const string ObjectLdapSamAccountNameFilter = "(&(|(objectCategory=Person)(objectCategory=Computer)(objectCategory=Group))(sAMAccountName={0}))";
        private static readonly string[] ObjectLoadProperties = { "objectCategory" };
        private static readonly string[] ObjectLoadPropertiesAll = ObjectLoadProperties.Concat(ADUserAccount.LoadProperties).Concat(ADMachineAccount.LoadProperties).Concat(ADGroup.LoadProperties).Distinct().ToArray();

        public IADObject RetrieveADObject(string Id, bool Quick, string[] AdditionalProperties = null)
        {
            var result = RetrieveBySamAccountName(Id, ObjectLdapSamAccountNameFilter, ObjectLoadPropertiesAll);

            if (result == null)
                return null;
            else
            {
                var objectCategory = result.Value<string>("objectCategory");

                if (objectCategory == null || objectCategory.Length == 0)
                    throw new InvalidOperationException("objectCategory is null or empty");

                if (objectCategory.StartsWith("CN=Person,", StringComparison.OrdinalIgnoreCase))
                    return result.AsADUserAccount(Quick, AdditionalProperties);
                else if (objectCategory.StartsWith("CN=Computer,", StringComparison.OrdinalIgnoreCase))
                    return result.AsADMachineAccount(AdditionalProperties);
                else if (objectCategory.StartsWith("CN=Group,", StringComparison.OrdinalIgnoreCase))
                    return result.AsADGroup(AdditionalProperties);
                else if (objectCategory.StartsWith("CN=Foreign-Security-Principal,", StringComparison.OrdinalIgnoreCase))
                    return null;
                else
                    throw new InvalidOperationException("Unexpected objectCategory");
            }
        }

        public IADObject RetrieveADObjectByDistinguishedName(string distinguishedName, bool quick, string[] additionalProperties = null)
        {
            // ignore foreign security principals
            var containerIndex = distinguishedName.IndexOf(',') + 1;
            var container = distinguishedName.Substring(containerIndex, distinguishedName.IndexOf(',', containerIndex) - containerIndex);
            if (string.Equals("CN=ForeignSecurityPrincipals", container, StringComparison.OrdinalIgnoreCase))
                return null;

            using (var entry = RetrieveDirectoryEntry(distinguishedName, additionalProperties))
            {
                if (entry == null)
                    return null;
                else
                    return entry.AsADObject(quick, additionalProperties);
            }
        }
        #endregion

        #region Organisational Units
        private const string OrganisationalUnitsLdapFilter = "(objectCategory=organizationalUnit)";
        private static readonly string[] OrganisationalUnitsLoadProperties = { "name", "distinguishedName" };

        public List<ADOrganisationalUnit> RetrieveADOrganisationalUnitStructure()
        {
            Dictionary<string, List<ADOrganisationalUnit>> resultTree = new Dictionary<string, List<ADOrganisationalUnit>>();

            var unsortedOrganisationalUnits = SearchEntireDomain(OrganisationalUnitsLdapFilter, OrganisationalUnitsLoadProperties)
                .Select(r => r.AsADOrganisationalUnit()).ToList();

            var indexedOrganisationalUnits = unsortedOrganisationalUnits.ToDictionary(k => k.DistinguishedName);

            var indexedChildren = unsortedOrganisationalUnits
                .GroupBy(ou => ou.DistinguishedName.Substring(ou.DistinguishedName.IndexOf(',') + 1))
                .ToDictionary(g => g.Key, g => g.ToList());

            // Link Children
            foreach (var ouChildren in indexedChildren)
            {
                if (indexedOrganisationalUnits.TryGetValue(ouChildren.Key, out var ouParent))
                {
                    ouParent.Children = ouChildren.Value.OrderBy(o => o.Name).ToList();
                }
            }

            return indexedChildren[Domain.DistinguishedName];
        }
        #endregion

        private ADSearchResult RetrieveBySamAccountName(string Id, string LdapFilterTemplate, string[] LoadProperties)
        {
            var slashIndex = Id.IndexOf('\\');

            if (!Domain.NetBiosName.Equals(Id.Substring(0, slashIndex), StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException($"The Id [{Id}] is invalid for this domain [{Domain.Name}]", "Id");

            var ldapFilter = string.Format(LdapFilterTemplate, Id.Substring(slashIndex + 1));

            return SearchEntireDomain(ldapFilter, LoadProperties, ActiveDirectory.SingleSearchResult).FirstOrDefault();
        }
        #endregion

        #region Actions
        public bool IsReachable()
        {
            using (Ping p = new Ping())
            {
                var pr = p.Send(Name, 1000);
                return (pr.Status == IPStatus.Success);
            }
        }

        public string OfflineDomainJoinProvision(string ComputerSamAccountName, string OrganisationalUnit, ref ADMachineAccount MachineAccount, out string DiagnosticInformation)
        {
            if (MachineAccount != null && MachineAccount.IsCriticalSystemObject)
                throw new InvalidOperationException($"This account {MachineAccount.DistinguishedName} is a Critical System Active Directory Object and Disco ICT refuses to modify it");

            if (!IsWritable)
                throw new InvalidOperationException($"The domain controller [{Name}] is not writable. This action (Offline Domain Join Provision) requires a writable domain controller.");

            StringBuilder diagnosticInfo = new StringBuilder();
            string DJoinResult = null;

            if (!string.IsNullOrWhiteSpace(ComputerSamAccountName))
                ComputerSamAccountName = ComputerSamAccountName.TrimEnd('$');
            if (!string.IsNullOrWhiteSpace(ComputerSamAccountName) && ComputerSamAccountName.Contains('\\'))
                ComputerSamAccountName = ComputerSamAccountName.Substring(ComputerSamAccountName.IndexOf('\\') + 1);

            // NetBIOS Limit (16 characters; "{ComputerName}$"; 15 characters allowed)
            if (string.IsNullOrWhiteSpace(ComputerSamAccountName) || ComputerSamAccountName.Length > 15)
                throw new ArgumentException("Invalid Computer Name; > 0 and <= 15", "ComputerName");

            // Ensure Specified OU Exists
            if (!string.IsNullOrEmpty(OrganisationalUnit))
            {
                try
                {
                    using (var deOU = RetrieveDirectoryEntry(OrganisationalUnit, new string[] { "distinguishedName" }))
                    {
                        if (deOU == null)
                            throw new Exception($"OU's Directory Entry couldn't be found at [{OrganisationalUnit}]");
                    }
                }
                catch (Exception ex)
                {
                    throw new ArgumentException($"An error occurred while trying to locate the specified OU: {OrganisationalUnit}", "OrganisationalUnit", ex);
                }
            }

            if (MachineAccount != null)
                MachineAccount.DeleteAccount(this);

            string tempFileName = System.IO.Path.GetTempFileName();
            string argumentOU = (!string.IsNullOrWhiteSpace(OrganisationalUnit)) ? $" /MACHINEOU \"{OrganisationalUnit}\"" : string.Empty;
            string arguments = $"/PROVISION /DOMAIN \"{Domain.Name}\" /DCNAME \"{Name}\" /MACHINE \"{ComputerSamAccountName}\"{argumentOU} /REUSE /SAVEFILE \"{tempFileName}\"";
            ProcessStartInfo commandStarter = new ProcessStartInfo("DJOIN.EXE", arguments)
            {
                CreateNoWindow = true,
                ErrorDialog = false,
                LoadUserProfile = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };
            diagnosticInfo.AppendFormat($"DJOIN.EXE {arguments}");
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
                throw new InvalidOperationException(
$@"Domain Join Unsuccessful
Error: {stdError}
Output: {stdOutput}");

            DiagnosticInformation = diagnosticInfo.ToString();

            // Reload Machine Account
            MachineAccount = RetrieveADMachineAccount($@"{Domain.NetBiosName}\{ComputerSamAccountName}", (MachineAccount == null ? null : MachineAccount.LoadedProperties.Keys.ToArray()));

            return DJoinResult;
        }
        #endregion

        public override string ToString()
        {
            return Name;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is ADDomainController))
                return false;
            else
                return Name == ((ADDomainController)obj).Name;
        }
        public override int GetHashCode()
        {
            return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(Name);
        }
    }
}
