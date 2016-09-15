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
            this.context = Context;

            this.Domain = Domain;
            this.DomainController = DomainController;

            this.Name = DomainController.Name;
            this.SiteName = DomainController.SiteName;

            this.IsSiteServer = IsSiteServer;
            this.IsWritable = IsWritable;

            this.AvailableWhen = null;
        }

        public ADDirectoryEntry RetrieveDirectoryEntry(string DistinguishedName, string[] LoadProperties = null)
        {
            if (string.IsNullOrWhiteSpace(DistinguishedName))
                throw new ArgumentNullException("DistinguishedName");

            if (!DistinguishedName.EndsWith(this.Domain.DistinguishedName, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException(string.Format("The Distinguished Name ({0}) isn't a member of this domain [{1}]", DistinguishedName, this.Domain.Name), "DistinguishedName");

            var entry = new DirectoryEntry(string.Format(LdapPathTemplate, this.Name, ADHelpers.EscapeDistinguishedName(DistinguishedName)));

            if (LoadProperties != null)
                entry.RefreshCache(LoadProperties);

            return new ADDirectoryEntry(this.Domain, this, entry);
        }

        #region Searching
        public IEnumerable<ADSearchResult> SearchEntireDomain(string LdapFilter, string[] LoadProperties, int? ResultLimit = null)
        {
            return SearchInternal(Domain.DistinguishedName, LdapFilter, LoadProperties, ResultLimit);
        }

        public IEnumerable<ADSearchResult> SearchScope(string LdapFilter, string[] LoadProperties, int? ResultLimit = null)
        {
            var searchScope = this.Domain.SearchContainers;

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

            using (ADDirectoryEntry rootEntry = this.RetrieveDirectoryEntry(SearchRoot))
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
        public ADMachineAccount RetrieveADMachineAccount(string Id, System.Guid? UUIDNetbootGUID, System.Guid? MacAddressNetbootGUID, string[] AdditionalProperties = null)
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
                adResult = this.SearchEntireDomain(ldapFilter, loadProperites, ActiveDirectory.SingleSearchResult).FirstOrDefault();
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
        public ADMachineAccount RetrieveADMachineAccount(string Id, System.Guid? NetbootGUID, string[] AdditionalProperties = null)
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

            using (var groupEntry = this.RetrieveDirectoryEntry(DistinguishedName, loadProperites))
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
            if (!SecurityIdentifier.IsEqualDomainSid(this.Domain.SecurityIdentifier))
                throw new ArgumentException(string.Format("The specified Security Identifier [{0}] does not belong to this domain [{1}]", SecurityIdentifier.ToString(), this.Domain.Name), "SecurityIdentifier");

            var sidBinaryString = SecurityIdentifier.ToBinaryString();

            string ldapFilter = string.Format(ADGroup.LdapSecurityIdentifierFilterTemplate, sidBinaryString);
            string[] loadProperites = (AdditionalProperties != null && AdditionalProperties.Length > 0)
                ? ADGroup.LoadProperties.Concat(AdditionalProperties).ToArray()
                : ADGroup.LoadProperties;

            var result = this.SearchEntireDomain(ldapFilter, loadProperites, ActiveDirectory.SingleSearchResult).FirstOrDefault();
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
                objectCategory = objectCategory.Substring(0, objectCategory.IndexOf(',')).ToLower();
                switch (objectCategory)
                {
                    case "cn=person":
                        return result.AsADUserAccount(Quick, AdditionalProperties);
                    case "cn=computer":
                        return result.AsADMachineAccount(AdditionalProperties);
                    case "cn=group":
                        return result.AsADGroup(AdditionalProperties);
                    default:
                        throw new InvalidOperationException("Unexpected objectCategory");
                }
            }
        }
        #endregion

        #region Organisational Units
        private const string OrganisationalUnitsLdapFilter = "(objectCategory=organizationalUnit)";
        private static readonly string[] OrganisationalUnitsLoadProperties = { "name", "distinguishedName" };

        public List<ADOrganisationalUnit> RetrieveADOrganisationalUnitStructure()
        {
            Dictionary<string, List<ADOrganisationalUnit>> resultTree = new Dictionary<string, List<ADOrganisationalUnit>>();

            var unsortedOrganisationalUnits = this.SearchEntireDomain(OrganisationalUnitsLdapFilter, OrganisationalUnitsLoadProperties)
                .Select(r => r.AsADOrganisationalUnit()).ToList();

            var indexedOrganisationalUnits = unsortedOrganisationalUnits.ToDictionary(k => k.DistinguishedName);

            var indexedChildren = unsortedOrganisationalUnits
                .GroupBy(ou => ou.DistinguishedName.Substring(ou.DistinguishedName.IndexOf(',') + 1))
                .ToDictionary(g => g.Key, g => g.ToList());

            // Link Children
            foreach (var ouChildren in indexedChildren)
            {
                ADOrganisationalUnit ouParent;
                if (indexedOrganisationalUnits.TryGetValue(ouChildren.Key, out ouParent))
                {
                    ouParent.Children = ouChildren.Value;
                }
            }

            return indexedChildren[Domain.DistinguishedName];
        }
        #endregion

        private ADSearchResult RetrieveBySamAccountName(string Id, string LdapFilterTemplate, string[] LoadProperties)
        {
            var slashIndex = Id.IndexOf('\\');

            if (!this.Domain.NetBiosName.Equals(Id.Substring(0, slashIndex), StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException(string.Format("The Id [{0}] is invalid for this domain [{1}]", Id, this.Domain.Name), "Id");

            var ldapFilter = string.Format(LdapFilterTemplate, Id.Substring(slashIndex + 1));

            return this.SearchEntireDomain(ldapFilter, LoadProperties, ActiveDirectory.SingleSearchResult).FirstOrDefault();
        }
        #endregion

        #region Actions
        public bool IsReachable()
        {
            using (Ping p = new Ping())
            {
                var pr = p.Send(this.Name, 1000);
                return (pr.Status == IPStatus.Success);
            }
        }

        public string OfflineDomainJoinProvision(string ComputerSamAccountName, string OrganisationalUnit, ref ADMachineAccount MachineAccount, out string DiagnosticInformation)
        {
            if (MachineAccount != null && MachineAccount.IsCriticalSystemObject)
                throw new InvalidOperationException(string.Format("This account {0} is a Critical System Active Directory Object and Disco refuses to modify it", MachineAccount.DistinguishedName));

            if (!this.IsWritable)
                throw new InvalidOperationException(string.Format("The domain controller [{0}] is not writable. This action (Offline Domain Join Provision) requires a writable domain controller.", this.Name));

            StringBuilder diagnosticInfo = new StringBuilder();
            string DJoinResult = null;

            if (!string.IsNullOrWhiteSpace(ComputerSamAccountName))
                ComputerSamAccountName = ComputerSamAccountName.TrimEnd('$');
            if (!string.IsNullOrWhiteSpace(ComputerSamAccountName) && ComputerSamAccountName.Contains('\\'))
                ComputerSamAccountName = ComputerSamAccountName.Substring(ComputerSamAccountName.IndexOf('\\') + 1);
            if (string.IsNullOrWhiteSpace(ComputerSamAccountName) || ComputerSamAccountName.Length > 24)
                throw new System.ArgumentException("Invalid Computer Name; > 0 and <= 24", "ComputerName");

            // Ensure Specified OU Exists
            if (!string.IsNullOrEmpty(OrganisationalUnit))
            {
                try
                {
                    using (var deOU = this.RetrieveDirectoryEntry(OrganisationalUnit, new string[] { "distinguishedName" }))
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

            if (MachineAccount != null)
                MachineAccount.DeleteAccount(this);

            string tempFileName = System.IO.Path.GetTempFileName();
            string argumentOU = (!string.IsNullOrWhiteSpace(OrganisationalUnit)) ? string.Format(" /MACHINEOU \"{0}\"", OrganisationalUnit) : string.Empty;
            string arguments = string.Format("/PROVISION /DOMAIN \"{0}\" /DCNAME \"{1}\" /MACHINE \"{2}\"{3} /REUSE /SAVEFILE \"{4}\"",
                this.Domain.Name,
                this.Name,
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

            // Reload Machine Account
            MachineAccount = this.RetrieveADMachineAccount(string.Format(@"{0}\{1}", this.Domain.NetBiosName, ComputerSamAccountName), (MachineAccount == null ? null : MachineAccount.LoadedProperties.Keys.ToArray()));

            return DJoinResult;
        }
        #endregion

        public override string ToString()
        {
            return this.Name;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is ADDomainController))
                return false;
            else
                return this.Name == ((ADDomainController)obj).Name;
        }
        public override int GetHashCode()
        {
            return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(this.Name);
        }
    }
}
