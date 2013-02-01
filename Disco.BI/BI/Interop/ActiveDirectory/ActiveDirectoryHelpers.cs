using System;
using System.Collections;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Disco.BI.Interop.ActiveDirectory
{
    internal static class ActiveDirectoryHelpers
    {
        #region Static Cached Properties
        private static string _DefaultDomainName;
        private static string _DefaultDomainPDCName;
        private static System.Collections.Generic.List<string> _DefaultDomainDCNames;
        private static string _DefaultDomainNetBiosName;
        private static string _DefaultDomainQualifiedName;
        private static string _DefaultLdapPath;
        private static bool _DetermineDomainProperties_Loaded = false;
        private static object _DetermineDomainProperties_Lock = new object();
        internal static string DefaultDomainName
        {
            get
            {
                ActiveDirectoryHelpers.DetermineDomainProperties();
                return ActiveDirectoryHelpers._DefaultDomainName;
            }
        }
        internal static string DefaultDomainPDCName
        {
            get
            {
                ActiveDirectoryHelpers.DetermineDomainProperties();
                return ActiveDirectoryHelpers._DefaultDomainPDCName;
            }
        }
        internal static System.Collections.Generic.List<string> DefaultDomainDCNames
        {
            get
            {
                ActiveDirectoryHelpers.DetermineDomainProperties();
                return ActiveDirectoryHelpers._DefaultDomainDCNames;
            }
        }
        internal static string DefaultDomainNetBiosName
        {
            get
            {
                ActiveDirectoryHelpers.DetermineDomainProperties();
                return ActiveDirectoryHelpers._DefaultDomainNetBiosName;
            }
        }
        internal static string DefaultDomainQualifiedName
        {
            get
            {
                ActiveDirectoryHelpers.DetermineDomainProperties();
                return ActiveDirectoryHelpers._DefaultDomainQualifiedName;
            }
        }
        internal static string DefaultLdapPath
        {
            get
            {
                ActiveDirectoryHelpers.DetermineDomainProperties();
                return ActiveDirectoryHelpers._DefaultLdapPath;
            }
        }
        internal static string DefaultDCLdapPath(string DC)
        {
            return string.Format("LDAP://{0}/", DC);
        }
        internal static DirectoryEntry DefaultLdapRoot
        {
            get
            {
                return new DirectoryEntry(string.Concat(ActiveDirectoryHelpers.DefaultLdapPath, ActiveDirectoryHelpers.DefaultDomainQualifiedName));
            }
        }
        internal static DirectoryEntry DefaultDCLdapRoot(string DC)
        {
            return new DirectoryEntry(string.Concat(ActiveDirectoryHelpers.DefaultDCLdapPath(DC), ActiveDirectoryHelpers.DefaultDomainQualifiedName));
        }

        private static void DetermineDomainProperties()
        {
            if (!ActiveDirectoryHelpers._DetermineDomainProperties_Loaded)
            {
                lock (ActiveDirectoryHelpers._DetermineDomainProperties_Lock)
                {

                    if (!ActiveDirectoryHelpers._DetermineDomainProperties_Loaded)
                    {
                        using (Domain domain = Domain.GetDomain(new DirectoryContext(DirectoryContextType.Domain)))
                        {
                            ActiveDirectoryHelpers._DefaultDomainName = domain.Name;
                            ActiveDirectoryHelpers._DefaultDomainPDCName = domain.PdcRoleOwner.Name;
                            ActiveDirectoryHelpers._DefaultDomainDCNames = new System.Collections.Generic.List<string>(domain.DomainControllers.Count);
                            foreach (DomainController dc in domain.DomainControllers)
                            {
                                ActiveDirectoryHelpers._DefaultDomainDCNames.Add(dc.Name);
                            }
                        }
                        ActiveDirectoryHelpers._DefaultDomainQualifiedName = string.Format("DC={0}", ActiveDirectoryHelpers._DefaultDomainName.Replace(".", ",DC="));
                        ActiveDirectoryHelpers._DefaultLdapPath = string.Format("LDAP://{0}/", ActiveDirectoryHelpers._DefaultDomainPDCName);
                        using (DirectoryEntry entry = new DirectoryEntry(string.Format("{0}CN=Partitions,CN=Configuration,{1}", ActiveDirectoryHelpers._DefaultLdapPath, ActiveDirectoryHelpers._DefaultDomainQualifiedName)))
                        {
                            using (DirectorySearcher searcher = new DirectorySearcher(entry, "(&(objectClass=crossRef)(nETBIOSName=*))", new string[] { "nETBIOSName" }))
                            {
                                SearchResult result = searcher.FindOne();
                                if (result != null)
                                {
                                    ActiveDirectoryHelpers._DefaultDomainNetBiosName = result.Properties["nETBIOSName"][0].ToString();
                                }
                                else
                                {
                                    ActiveDirectoryHelpers._DefaultDomainNetBiosName = ActiveDirectoryHelpers._DefaultDomainQualifiedName;
                                }
                            }
                        }
                    }
                    ActiveDirectoryHelpers._DetermineDomainProperties_Loaded = true;
                }
            }
        }
        #endregion

        [System.Runtime.InteropServices.DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool ConvertSidToStringSid(byte[] pSID, ref System.Text.StringBuilder ptrSid);
        internal static string ConvertBytesToSIDString(byte[] SID)
        {
            System.Text.StringBuilder sidString = new System.Text.StringBuilder();
            bool flag = ActiveDirectoryHelpers.ConvertSidToStringSid(SID, ref sidString);
            string ConvertBytesToSIDString;
            if (flag)
            {
                ConvertBytesToSIDString = sidString.ToString();
            }
            else
            {
                ConvertBytesToSIDString = null;
            }
            return ConvertBytesToSIDString;
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
    }
}
