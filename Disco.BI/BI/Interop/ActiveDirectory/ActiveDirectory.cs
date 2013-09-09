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
                var loadProperties = new List<string> { "name", "distinguishedName", "sAMAccountName", "objectSid", "dNSHostName", "netbootGUID", "isCriticalSystemObject" };
                loadProperties.AddRange(AdditionalProperties);
                using (DirectorySearcher dSearcher = new DirectorySearcher(dRootEntry, string.Format("(&(objectClass=computer)(sAMAccountName={0}))", ActiveDirectoryHelpers.EscapeLdapQuery(sAMAccountName)), loadProperties.ToArray(), SearchScope.Subtree))
                {
                    SearchResult dResult = dSearcher.FindOne();
                    if (dResult != null)
                    {
                        return ActiveDirectory.DirectorySearchResultToMachineAccount(dResult, AdditionalProperties);
                    }
                }

                if (UUIDNetbootGUID.HasValue)
                {
                    using (DirectorySearcher dSearcher = new DirectorySearcher(dRootEntry, string.Format("(&(objectClass=computer)(netbootGUID={0}))", ActiveDirectoryHelpers.FormatGuidForLdapQuery(UUIDNetbootGUID.Value)), loadProperties.ToArray(), SearchScope.Subtree))
                    {
                        SearchResult dResult = dSearcher.FindOne();
                        if (dResult != null)
                        {
                            return ActiveDirectory.DirectorySearchResultToMachineAccount(dResult, AdditionalProperties);
                        }
                    }
                }
                if (MacAddressNetbootGUID.HasValue)
                {
                    using (DirectorySearcher dSearcher = new DirectorySearcher(dRootEntry, string.Format("(&(objectClass=computer)(netbootGUID={0}))", ActiveDirectoryHelpers.FormatGuidForLdapQuery(MacAddressNetbootGUID.Value)), loadProperties.ToArray(), SearchScope.Subtree))
                    {
                        SearchResult dResult = dSearcher.FindOne();
                        if (dResult != null)
                        {
                            return ActiveDirectory.DirectorySearchResultToMachineAccount(dResult, AdditionalProperties);
                        }
                    }
                }

            }

            return null;
        }
        private static ActiveDirectoryMachineAccount DirectorySearchResultToMachineAccount(SearchResult result, params string[] AdditionalProperties)
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
                sAMAccountName = sAMAccountName,
                ObjectSid = objectSid,
                NetbootGUID = netbootGUIDResult,
                Path = result.Path,
                Domain = ActiveDirectoryHelpers.DefaultDomainNetBiosName,
                DnsName = dNSName,
                IsCriticalSystemObject = isCriticalSystemObject,
                LoadedProperties = additionalProperties
            };
        }
        private static ActiveDirectoryUserAccount SearchResultToActiveDirectoryUserAccount(SearchResult result, params string[] AdditionalProperties)
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
            var groupCNs = result.Properties["memberOf"].Cast<string>().ToList();
            groupCNs.Add(ActiveDirectoryCachedGroups.GetGroupsCnForSid(primaryGroupSid));
            List<string> groups = ActiveDirectoryCachedGroups.GetGroups(groupCNs).Select(g => g.ToLower()).ToList();

            //foreach (string groupCN in result.Properties["memberOf"])
            //{
            // Removed 2012-11-30 G# - Moved to Recursive Cache
            //var groupCNlower = groupCN.ToLower();
            //if (groupCNlower.StartsWith("cn="))
            //    groups.Add(groupCNlower.Substring(3, groupCNlower.IndexOf(",") - 3));
            // End Removed 2012-11-30 G#
            //}

            string type = null;
            if (groups.Contains("domain admins") || groups.Contains("disco admins"))
            {
                type = "Admin";
            }
            else
            {
                if (groups.Contains("staff"))
                {
                    type = "Staff";
                }
                else
                {
                    if (groups.Contains("students"))
                    {
                        type = "Student";
                    }
                }
            }

            // Additional Properties
            Dictionary<string, object[]> additionalProperties = new Dictionary<string, object[]>();
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
                sAMAccountName = username,
                DisplayName = displayName,
                ObjectSid = objectSidSDDL,
                Type = type,
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
                var loadProperties = new List<string> {
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
                loadProperties.AddRange(AdditionalProperties);
                using (DirectorySearcher dSearcher = new DirectorySearcher(dRootEntry, string.Format("(&(objectClass=user)(sAMAccountName={0}))", ActiveDirectoryHelpers.EscapeLdapQuery(sAMAccountName)), loadProperties.ToArray(), SearchScope.Subtree))
                {
                    SearchResult dResult = dSearcher.FindOne();
                    if (dResult != null)
                        return ActiveDirectory.SearchResultToActiveDirectoryUserAccount(dResult, AdditionalProperties);
                    else
                        return null;
                }
            }
        }
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

        public static List<ActiveDirectoryUserAccount> SearchUsers(string term)
        {
            List<ActiveDirectoryUserAccount> users = new List<ActiveDirectoryUserAccount>();
            string defaultQualifiedDomainName = ActiveDirectoryHelpers.DefaultDomainQualifiedName;
            string defaultNetBiosDomainName = ActiveDirectoryHelpers.DefaultDomainNetBiosName;
            term = ActiveDirectoryHelpers.EscapeLdapQuery(term);
            using (DirectoryEntry entry = new DirectoryEntry(string.Format("LDAP://{0}", defaultQualifiedDomainName)))
            {
                using (DirectorySearcher searcher = new DirectorySearcher(entry, string.Format("(&(objectClass=User)(objectCategory=Person)(|(sAMAccountName=*{0}*)(displayName=*{0}*)))", term), new string[]
				{
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
				}, SearchScope.Subtree))
                {
                    searcher.SizeLimit = 30;
                    SearchResultCollection results = searcher.FindAll();
                    foreach (SearchResult result in results)
                    {
                        users.Add(ActiveDirectory.SearchResultToActiveDirectoryUserAccount(result));
                    }
                }
            }
            return users;
        }
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
    }
}
