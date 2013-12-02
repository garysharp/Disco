using Disco.Models.Interop.ActiveDirectory;
using Disco.BI.DeviceBI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices;
using System.Linq;
using System.IO;

namespace Disco.BI.Interop.ActiveDirectory
{
    public static class ActiveDirectory
    {
        #region Machine Accounts

        private static readonly string[] MachineLoadProperties = {
                                                                     "name",
                                                                     "distinguishedName",
                                                                     "sAMAccountName",
                                                                     "objectSid",
                                                                     "dNSHostName",
                                                                     "netbootGUID",
                                                                     "isCriticalSystemObject"
                                                                 };
        public static ActiveDirectoryMachineAccount GetMachineAccount(string ComputerName, System.Guid? UUIDNetbootGUID = null, System.Guid? MacAddressNetbootGUID = null, params string[] AdditionalProperties)
        {
            if (string.IsNullOrWhiteSpace(ComputerName))
                throw new System.ArgumentException("Invalid Computer Name - Empty", "ComputerName");
            if (ComputerName.Contains("\\"))
                ComputerName = ComputerName.Substring(checked(ComputerName.IndexOf("\\") + 1));
            if (ComputerName.Length > 24)
                throw new System.ArgumentException("Invalid Computer Name - Length > 24", "ComputerName");
            string sAMAccountName = ComputerName;
            if (!sAMAccountName.EndsWith("$"))
                sAMAccountName = string.Format("{0}$", sAMAccountName);

            using (DirectoryEntry dRootEntry = ActiveDirectoryHelpers.DefaultLdapRoot)
            {
                var loadProperties = AdditionalProperties == null ? MachineLoadProperties : MachineLoadProperties.Concat(AdditionalProperties).ToArray();
                
                using (DirectorySearcher dSearcher = new DirectorySearcher(dRootEntry, string.Format("(&(objectCategory=computer)(sAMAccountName={0}))", ActiveDirectoryHelpers.EscapeLdapQuery(sAMAccountName)), loadProperties, SearchScope.Subtree))
                {
                    SearchResult dResult = dSearcher.FindOne();
                    if (dResult != null)
                    {
                        return ActiveDirectory.ActiveDirectoryMachineAccountFromSearchResult(dResult, AdditionalProperties);
                    }
                }

                if (UUIDNetbootGUID.HasValue)
                {
                    using (DirectorySearcher dSearcher = new DirectorySearcher(dRootEntry, string.Format("(&(objectCategory=computer)(netbootGUID={0}))", ActiveDirectoryHelpers.FormatGuidForLdapQuery(UUIDNetbootGUID.Value)), loadProperties, SearchScope.Subtree))
                    {
                        SearchResult dResult = dSearcher.FindOne();
                        if (dResult != null)
                        {
                            return ActiveDirectory.ActiveDirectoryMachineAccountFromSearchResult(dResult, AdditionalProperties);
                        }
                    }
                }
                if (MacAddressNetbootGUID.HasValue)
                {
                    using (DirectorySearcher dSearcher = new DirectorySearcher(dRootEntry, string.Format("(&(objectCategory=computer)(netbootGUID={0}))", ActiveDirectoryHelpers.FormatGuidForLdapQuery(MacAddressNetbootGUID.Value)), loadProperties, SearchScope.Subtree))
                    {
                        SearchResult dResult = dSearcher.FindOne();
                        if (dResult != null)
                        {
                            return ActiveDirectory.ActiveDirectoryMachineAccountFromSearchResult(dResult, AdditionalProperties);
                        }
                    }
                }

            }

            return null;
        }
        private static ActiveDirectoryMachineAccount ActiveDirectoryMachineAccountFromSearchResult(SearchResult result, params string[] AdditionalProperties)
        {
            string name = result.Properties["name"][0].ToString();
            string sAMAccountName = result.Properties["sAMAccountName"][0].ToString();
            string distinguishedName = result.Properties["distinguishedName"][0].ToString();
            string objectSid = ActiveDirectoryHelpers.ConvertBytesToSDDLString((byte[])result.Properties["objectSid"][0]);

            var dNSNameProperty = result.Properties["dNSHostName"];
            string dNSName = null;
            if (dNSNameProperty.Count > 0)
                dNSName = dNSNameProperty[0].ToString();
            else
                dNSName = string.Format("{0}.{1}", sAMAccountName.TrimEnd('$'), ActiveDirectoryHelpers.DefaultDomainQualifiedName);

            bool isCriticalSystemObject = (bool)result.Properties["isCriticalSystemObject"][0];

            System.Guid netbootGUIDResult = default(System.Guid);
            ResultPropertyValueCollection netbootGUIDProp = result.Properties["netbootGUID"];
            if (netbootGUIDProp.Count > 0)
            {
                netbootGUIDResult = new System.Guid((byte[])netbootGUIDProp[0]);
            }

            // Additional Properties
            Dictionary<string, object[]> additionalProperties = new Dictionary<string, object[]>();
            if (AdditionalProperties != null)
                foreach (string propertyName in AdditionalProperties)
                {
                    var property = result.Properties[propertyName];
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
                Path = result.Path,
                Domain = ActiveDirectoryHelpers.DefaultDomainNetBiosName,
                DnsName = dNSName,
                IsCriticalSystemObject = isCriticalSystemObject,
                LoadedProperties = additionalProperties
            };
        }

        #endregion

        public static string OfflineDomainJoinProvision(ref ActiveDirectoryMachineAccount ExistingAccount, string ComputerName, string OrganisationalUnit = null, string EnrolSessionId = null)
        {
            if (ExistingAccount != null && ExistingAccount.IsCriticalSystemObject)
                throw new InvalidOperationException(string.Format("This account {0} is a Critical System Active Directory Object and Disco refuses to modify it", ExistingAccount.DistinguishedName));

            string DJoinResult = null;
            if (string.IsNullOrWhiteSpace(ComputerName) || ComputerName.Length > 24)
                throw new System.ArgumentException("Invalid Computer Name; > 0 and <= 24", "ComputerName");

            // Added 2012-10-25 G#
            // Ensure Specified OU Exists
            if (!string.IsNullOrEmpty(OrganisationalUnit))
            {
                var ouPath = string.Format("{0}{1},{2}", ActiveDirectoryHelpers.DefaultLdapPath, OrganisationalUnit, ActiveDirectoryHelpers.DefaultDomainQualifiedName);
                try
                {
                    using (DirectoryEntry ou = new DirectoryEntry(ouPath))
                    {
                        if (ou == null)
                        {
                            throw new Exception("OU's Directory Entry couldn't be found");
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new ArgumentException(string.Format("An error occurred while trying to locate the specified OU: {0}", ouPath), "OrganisationalUnit", ex);
                }
            }
            // End Added 2012-10-25 G#

            // Delete Existing
            if (ExistingAccount != null)
                ExistingAccount.DeleteAccount();

            string tempFileName = System.IO.Path.GetTempFileName();
            string argumentOU = (!string.IsNullOrWhiteSpace(OrganisationalUnit)) ? string.Format(" /MACHINEOU \"{0},{1}\"", OrganisationalUnit, ActiveDirectoryHelpers.DefaultDomainQualifiedName) : string.Empty;
            string arguments = string.Format("/PROVISION /DOMAIN \"{0}\" /DCNAME \"{1}\" /MACHINE \"{2}\"{3} /REUSE /SAVEFILE \"{4}\"",
                ActiveDirectoryHelpers.DefaultDomainName,
                ActiveDirectoryHelpers.DefaultDomainPDCName,
                ComputerName,
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
            if (EnrolSessionId != null)
            {
                EnrolmentLog.LogSessionDiagnosticInformation(EnrolSessionId, string.Format("{0} {1}{2}", "DJOIN.EXE", arguments, System.Environment.NewLine));
            }

            string stdOutput;
            string stdError;
            using (Process commandProc = Process.Start(commandStarter))
            {
                commandProc.WaitForExit(20000);
                stdOutput = commandProc.StandardOutput.ReadToEnd();
                stdError = commandProc.StandardError.ReadToEnd();
            }
            if (EnrolSessionId != null)
            {
                if (!string.IsNullOrWhiteSpace(stdOutput))
                    EnrolmentLog.LogSessionDiagnosticInformation(EnrolSessionId, stdOutput + System.Environment.NewLine);
                if (!string.IsNullOrWhiteSpace(stdError))
                    EnrolmentLog.LogSessionDiagnosticInformation(EnrolSessionId, stdError + System.Environment.NewLine);
            }

            if (System.IO.File.Exists(tempFileName))
            {
                DJoinResult = System.Convert.ToBase64String(System.IO.File.ReadAllBytes(tempFileName));
                System.IO.File.Delete(tempFileName);
            }
            if (string.IsNullOrWhiteSpace(DJoinResult))
                throw new System.InvalidOperationException(string.Format("Domain Join Unsuccessful{0}Error: {1}{0}Output: {2}", System.Environment.NewLine, stdError, stdOutput));
            ExistingAccount = ActiveDirectory.GetMachineAccount(ComputerName);
            return DJoinResult;
        }

        #region Users

        private static readonly string[] UserLoadProperties = {
                                                                   "name", 
                                                                   "distinguishedName", 
                                                                   "sAMAccountName", 
                                                                   "objectSid", 
                                                                   "displayName", 
                                                                   "sn", 
                                                                   "givenName", 
                                                                   "memberOf", 
                                                                   "primaryGroupID",
                                                                   "mail", 
                                                                   "telephoneNumber"
                                                               };
        public static List<ActiveDirectoryUserAccount> SearchUsers(string term)
        {
            List<ActiveDirectoryUserAccount> users = new List<ActiveDirectoryUserAccount>();
            string defaultQualifiedDomainName = ActiveDirectoryHelpers.DefaultDomainQualifiedName;
            string defaultNetBiosDomainName = ActiveDirectoryHelpers.DefaultDomainNetBiosName;
            term = ActiveDirectoryHelpers.EscapeLdapQuery(term);
            using (DirectoryEntry entry = new DirectoryEntry(string.Format("LDAP://{0}", defaultQualifiedDomainName)))
            {
                using (DirectorySearcher searcher = new DirectorySearcher(entry, string.Format("(&(objectCategory=Person)(objectClass=user)(|(sAMAccountName=*{0}*)(displayName=*{0}*)))", term), UserLoadProperties, SearchScope.Subtree))
                {
                    searcher.SizeLimit = 30;
                    SearchResultCollection results = searcher.FindAll();
                    foreach (SearchResult result in results)
                    {
                        users.Add(ActiveDirectory.ActiveDirectoryUserAccountFromSearchResult(result));
                    }
                }
            }
            return users;
        }
        private static ActiveDirectoryUserAccount ActiveDirectoryUserAccountFromSearchResult(SearchResult result, params string[] AdditionalProperties)
        {
            string name = result.Properties["name"][0].ToString();
            string username = result.Properties["sAMAccountName"][0].ToString();
            string distinguishedName = result.Properties["distinguishedName"][0].ToString();
            byte[] objectSid = (byte[])result.Properties["objectSid"][0];
            string objectSidSDDL = ActiveDirectoryHelpers.ConvertBytesToSDDLString(objectSid);

            ResultPropertyValueCollection displayNameProp = result.Properties["displayName"];
            string displayName = username;
            if (displayNameProp.Count > 0)
                displayName = displayNameProp[0].ToString();
            string surname = null;
            ResultPropertyValueCollection surnameProp = result.Properties["sn"];
            if (surnameProp.Count > 0)
                surname = surnameProp[0].ToString();
            string givenName = null;
            ResultPropertyValueCollection givenNameProp = result.Properties["givenName"];
            if (givenNameProp.Count > 0)
                givenName = givenNameProp[0].ToString();
            string email = null;
            ResultPropertyValueCollection emailProp = result.Properties["mail"];
            if (emailProp.Count > 0)
                email = emailProp[0].ToString();
            string phone = null;
            ResultPropertyValueCollection phoneProp = result.Properties["telephoneNumber"];
            if (phoneProp.Count > 0)
                phone = phoneProp[0].ToString();

            int primaryGroupID = (int)result.Properties["primaryGroupID"][0];
            string primaryGroupSid = ActiveDirectoryHelpers.ConvertBytesToSDDLString(ActiveDirectoryHelpers.BuildPrimaryGroupSid(objectSid, primaryGroupID));
            var groupDistinguishedNames = result.Properties["memberOf"].Cast<string>().ToList();
            groupDistinguishedNames.Add(ActiveDirectoryCachedGroups.GetGroupsDistinguishedNameForSecurityIdentifier(primaryGroupSid));
            List<string> groups = ActiveDirectoryCachedGroups.GetGroups(groupDistinguishedNames).ToList();

            //foreach (string groupCN in result.Properties["memberOf"])
            //{
            // Removed 2012-11-30 G# - Moved to Recursive Cache
            //var groupCNlower = groupCN.ToLower();
            //if (groupCNlower.StartsWith("cn="))
            //    groups.Add(groupCNlower.Substring(3, groupCNlower.IndexOf(",") - 3));
            // End Removed 2012-11-30 G#
            //}

            // Additional Properties
            Dictionary<string, object[]> additionalProperties = new Dictionary<string, object[]>();
            if (AdditionalProperties != null)
                foreach (string propertyName in AdditionalProperties)
                {
                    var property = result.Properties[propertyName];
                    var propertyValues = new List<object>();
                    for (int index = 0; index < property.Count; index++)
                        propertyValues.Add(property[index]);
                    additionalProperties.Add(propertyName, propertyValues.ToArray());
                }

            return new ActiveDirectoryUserAccount
            {
                Domain = ActiveDirectoryHelpers.DefaultDomainNetBiosName,
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
                Path = result.Path,
                LoadedProperties = additionalProperties
            };
        }
        public static ActiveDirectoryUserAccount GetUserAccount(string Username, params string[] AdditionalProperties)
        {
            if (string.IsNullOrWhiteSpace(Username))
                throw new System.ArgumentException("Invalid User Account", "Username");
            string sAMAccountName = Username;
            if (sAMAccountName.Contains("\\"))
                sAMAccountName = sAMAccountName.Substring(checked(sAMAccountName.IndexOf("\\") + 1));

            using (DirectoryEntry dRootEntry = ActiveDirectoryHelpers.DefaultLdapRoot)
            {
                var loadProperties = AdditionalProperties == null ? UserLoadProperties : UserLoadProperties.Concat(AdditionalProperties).ToArray();

                using (DirectorySearcher dSearcher = new DirectorySearcher(dRootEntry, string.Format("(&(objectCategory=Person)(sAMAccountName={0}))", ActiveDirectoryHelpers.EscapeLdapQuery(sAMAccountName)), loadProperties, SearchScope.Subtree))
                {
                    SearchResult dResult = dSearcher.FindOne();
                    if (dResult != null)
                        return ActiveDirectory.ActiveDirectoryUserAccountFromSearchResult(dResult, AdditionalProperties);
                    else
                        return null;
                }
            }
        }

        #endregion

        #region Organisation Units

        public static List<ActiveDirectoryOrganisationalUnit> GetOrganisationalUnitStructure()
        {
            ActiveDirectoryOrganisationalUnit DomainOUs = new ActiveDirectoryOrganisationalUnit
            {
                Children = new System.Collections.Generic.List<ActiveDirectoryOrganisationalUnit>()
            };
            string defaultQualifiedDomainName = ActiveDirectoryHelpers.DefaultDomainQualifiedName;

            using (DirectoryEntry entry = new DirectoryEntry(string.Format("LDAP://{0}", defaultQualifiedDomainName)))
            {
                ActiveDirectory.GetOrganisationalUnitStructure_Recursive(ref DomainOUs, entry);
            }
            return DomainOUs.Children;
        }
        private static void GetOrganisationalUnitStructure_Recursive(ref ActiveDirectoryOrganisationalUnit ParentOU, DirectoryEntry Container)
        {
            using (DirectorySearcher searcher = new DirectorySearcher(Container, "(objectCategory=organizationalUnit)", new string[]
			{
				"name", 
				"distinguishedName"
			}, SearchScope.OneLevel))
            {
                using (SearchResultCollection results = searcher.FindAll())
                {
                    foreach (SearchResult result in results)
                    {
                        string i = result.Properties["name"][0].ToString();
                        string dn = result.Properties["distinguishedName"][0].ToString();
                        ActiveDirectoryOrganisationalUnit ChildOU = new ActiveDirectoryOrganisationalUnit
                        {
                            Name = i,
                            Path = dn.Substring(0, dn.IndexOf(",DC=")),
                            Children = new List<ActiveDirectoryOrganisationalUnit>()
                        };
                        ActiveDirectory.GetOrganisationalUnitStructure_Recursive(ref ChildOU, result.GetDirectoryEntry());
                        if (ChildOU.Children.Count == 0)
                            ChildOU.Children = null;
                        ParentOU.Children.Add(ChildOU);
                    }
                }
            }

        }

        #endregion

        #region Groups

        private static readonly string[] GroupLoadProperties = {
                                                                   "name", 
                                                                   "distinguishedName", 
                                                                   "cn", 
                                                                   "sAMAccountName", 
                                                                   "objectSid", 
                                                                   "memberOf"
                                                               };
        public static ActiveDirectoryGroup GetGroup(string SamAccountName)
        {
            if (string.IsNullOrWhiteSpace(SamAccountName))
                throw new System.ArgumentException("Invalid Group Account", "SamAccountName");
            string sAMAccountName = SamAccountName;
            if (sAMAccountName.Contains("\\"))
                sAMAccountName = sAMAccountName.Substring(checked(sAMAccountName.IndexOf("\\") + 1));

            using (DirectoryEntry dRootEntry = ActiveDirectoryHelpers.DefaultLdapRoot)
            {
                using (DirectorySearcher dSearcher = new DirectorySearcher(dRootEntry, string.Format("(&(objectCategory=Group)(objectSid={0}))", ActiveDirectoryHelpers.EscapeLdapQuery(sAMAccountName)), GroupLoadProperties, SearchScope.Subtree))
                {
                    SearchResult dResult = dSearcher.FindOne();
                    if (dResult != null)
                    {
                        return ActiveDirectoryGroupFromSearchResult(dResult);
                    }
                    else
                        return null;
                }
            }
        }
        public static ActiveDirectoryGroup GetGroupFromDistinguishedName(string DistinguishedName)
        {
            ActiveDirectoryGroup group = null;

            using (DirectoryEntry groupDE = new DirectoryEntry(string.Concat(ActiveDirectoryHelpers.DefaultLdapPath, DistinguishedName)))
            {
                if (groupDE != null)
                {
                    return ActiveDirectoryGroupFromDirectoryEntry(groupDE);
                }
            }

            return group;
        }
        public static ActiveDirectoryGroup GetGroupFromSecurityIdentifier(string SecurityIdentifier)
        {
            using (DirectoryEntry dRootEntry = ActiveDirectoryHelpers.DefaultLdapRoot)
            {
                var sidBytes = ActiveDirectoryHelpers.ConvertSDDLStringToBytes(SecurityIdentifier);
                var sidBinaryString = ActiveDirectoryHelpers.ConvertBytesToBinarySidString(sidBytes);

                using (DirectorySearcher dSearcher = new DirectorySearcher(dRootEntry, string.Format("(&(objectCategory=Group)(objectSid={0}))", sidBinaryString), GroupLoadProperties, SearchScope.Subtree))
                {
                    SearchResult dResult = dSearcher.FindOne();
                    if (dResult != null)
                    {
                        return ActiveDirectoryGroupFromSearchResult(dResult);
                    }
                    else
                        return null;
                }
            }
        }

        public static List<ActiveDirectoryGroup> SearchGroups(string term)
        {
            List<ActiveDirectoryGroup> results = new List<ActiveDirectoryGroup>();
            string defaultQualifiedDomainName = ActiveDirectoryHelpers.DefaultDomainQualifiedName;
            string defaultNetBiosDomainName = ActiveDirectoryHelpers.DefaultDomainNetBiosName;
            term = ActiveDirectoryHelpers.EscapeLdapQuery(term);
            using (DirectoryEntry entry = new DirectoryEntry(string.Format("LDAP://{0}", defaultQualifiedDomainName)))
            {
                using (DirectorySearcher searcher = new DirectorySearcher(entry, string.Format("(&(objectCategory=Group)(|(sAMAccountName=*{0}*)(name=*{0}*)(cn=*{0}*)))", term), GroupLoadProperties, SearchScope.Subtree))
                {
                    searcher.SizeLimit = 30;
                    SearchResultCollection searchResults = searcher.FindAll();
                    foreach (SearchResult result in searchResults)
                    {
                        results.Add(ActiveDirectory.ActiveDirectoryGroupFromSearchResult(result));
                    }
                }
            }
            return results;
        }

        private static ActiveDirectoryGroup ActiveDirectoryGroupFromDirectoryEntry(DirectoryEntry entry)
        {
            var name = (string)entry.Properties["name"].Value;
            var distinguishedName = (string)entry.Properties["distinguishedName"].Value;
            var cn = (string)entry.Properties["cn"].Value;
            var sAMAccountName = (string)entry.Properties["sAMAccountName"].Value;
            var objectSid = ActiveDirectoryHelpers.ConvertBytesToSDDLString((byte[])entry.Properties["objectSid"].Value);
            var memberOf = entry.Properties["memberOf"].Cast<string>().ToList();

            return new ActiveDirectoryGroup()
            {
                Name = name,
                DistinguishedName = distinguishedName,
                CommonName = cn,
                SamAccountName = sAMAccountName,
                SecurityIdentifier = objectSid,
                MemberOf = memberOf
            };
        }
        private static ActiveDirectoryGroup ActiveDirectoryGroupFromSearchResult(SearchResult result)
        {
            var name = (string)result.Properties["name"][0];
            var distinguishedName = (string)result.Properties["distinguishedName"][0];
            var cn = (string)result.Properties["cn"][0];
            var sAMAccountName = (string)result.Properties["sAMAccountName"][0];
            var objectSid = ActiveDirectoryHelpers.ConvertBytesToSDDLString((byte[])result.Properties["objectSid"][0]);
            var memberOf = result.Properties["memberOf"].Cast<string>().ToList();

            return new ActiveDirectoryGroup()
            {
                Name = name,
                DistinguishedName = distinguishedName,
                CommonName = cn,
                SamAccountName = sAMAccountName,
                SecurityIdentifier = objectSid,
                MemberOf = memberOf
            };
        }

        #endregion

        private static readonly string[] ObjectLoadProperties = { "objectCategory" };
        private static readonly string[] ObjectLoadPropertiesAll = ObjectLoadProperties.Concat(UserLoadProperties).Concat(MachineLoadProperties).Concat(GroupLoadProperties).Distinct().ToArray();

        public static IActiveDirectoryObject GetObject(string SamAccountName)
        {
            if (string.IsNullOrWhiteSpace(SamAccountName))
                throw new System.ArgumentException("Invalid Object Account Name", "SamAccountName");
            string sAMAccountName = SamAccountName;
            if (sAMAccountName.Contains("\\"))
                sAMAccountName = sAMAccountName.Substring(checked(sAMAccountName.IndexOf("\\") + 1));

            using (DirectoryEntry dRootEntry = ActiveDirectoryHelpers.DefaultLdapRoot)
            {
                using (DirectorySearcher dSearcher = new DirectorySearcher(dRootEntry, string.Format("(&(|(objectCategory=Person)(objectCategory=Computer)(objectCategory=Group))(sAMAccountName={0}))", ActiveDirectoryHelpers.EscapeLdapQuery(sAMAccountName)), ObjectLoadPropertiesAll, SearchScope.Subtree))
                {
                    SearchResult dResult = dSearcher.FindOne();
                    if (dResult != null)
                    {
                        var objectCategory = (string)dResult.Properties["objectCategory"][0];
                        objectCategory = objectCategory.Substring(0, objectCategory.IndexOf(',')).ToLower();
                        switch (objectCategory)
                        {
                            case "cn=person":
                                return ActiveDirectoryUserAccountFromSearchResult(dResult);
                            case "cn=computer":
                                return ActiveDirectoryMachineAccountFromSearchResult(dResult);
                            case "cn=group":
                                return ActiveDirectoryGroupFromSearchResult(dResult);
                            default:
                                throw new InvalidOperationException("Unexpected objectCategory");
                        }
                    }
                    else
                        return null;
                }
            }
        }
    }
}
