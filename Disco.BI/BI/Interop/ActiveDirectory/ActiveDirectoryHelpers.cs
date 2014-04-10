//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.DirectoryServices;
//using System.DirectoryServices.ActiveDirectory;
//using System.Runtime.CompilerServices;
//using System.Runtime.InteropServices;
//using System.Security.Principal;
//using System.Text;
//using System.Threading;

//namespace Disco.BI.Interop.ActiveDirectory
//{
//    internal static class ActiveDirectoryHelpers
//    {
//        #region Static Cached Properties
//        private static string _DefaultDomainName;
//        private static string _DefaultDomainPDCName;
//        private static System.Collections.Generic.List<string> _DefaultDomainDCNames;
//        private static string _DefaultDomainNetBiosName;
//        private static string _DefaultDomainQualifiedName;
//        private static string _DefaultLdapPath;
//        private static bool _DetermineDomainProperties_Loaded = false;
//        private static object _DetermineDomainProperties_Lock = new object();
//        internal static string DefaultDomainName
//        {
//            get
//            {
//                ActiveDirectoryHelpers.DetermineDomainProperties();
//                return ActiveDirectoryHelpers._DefaultDomainName;
//            }
//        }
//        internal static string DefaultDomainPDCName
//        {
//            get
//            {
//                ActiveDirectoryHelpers.DetermineDomainProperties();
//                return ActiveDirectoryHelpers._DefaultDomainPDCName;
//            }
//        }
//        internal static System.Collections.Generic.List<string> DefaultDomainDCNames
//        {
//            get
//            {
//                ActiveDirectoryHelpers.DetermineDomainProperties();
//                return ActiveDirectoryHelpers._DefaultDomainDCNames;
//            }
//        }
//        internal static string DefaultDomainNetBiosName
//        {
//            get
//            {
//                ActiveDirectoryHelpers.DetermineDomainProperties();
//                return ActiveDirectoryHelpers._DefaultDomainNetBiosName;
//            }
//        }
//        internal static string DefaultDomainQualifiedName
//        {
//            get
//            {
//                ActiveDirectoryHelpers.DetermineDomainProperties();
//                return ActiveDirectoryHelpers._DefaultDomainQualifiedName;
//            }
//        }
//        internal static string DefaultLdapPath
//        {
//            get
//            {
//                ActiveDirectoryHelpers.DetermineDomainProperties();
//                return ActiveDirectoryHelpers._DefaultLdapPath;
//            }
//        }
//        internal static string DefaultDCLdapPath(string DC)
//        {
//            return string.Format("LDAP://{0}/", DC);
//        }
//        internal static DirectoryEntry DefaultLdapRoot
//        {
//            get
//            {
//                return new DirectoryEntry(string.Concat(ActiveDirectoryHelpers.DefaultLdapPath, ActiveDirectoryHelpers.DefaultDomainQualifiedName));
//            }
//        }
//        internal static DirectoryEntry DefaultDCLdapRoot(string DC)
//        {
//            return new DirectoryEntry(string.Concat(ActiveDirectoryHelpers.DefaultDCLdapPath(DC), ActiveDirectoryHelpers.DefaultDomainQualifiedName));
//        }

//        private static void DetermineDomainProperties()
//        {
//            if (!ActiveDirectoryHelpers._DetermineDomainProperties_Loaded)
//            {
//                lock (ActiveDirectoryHelpers._DetermineDomainProperties_Lock)
//                {

//                    if (!ActiveDirectoryHelpers._DetermineDomainProperties_Loaded)
//                    {
//                        using (Domain domain = Domain.GetDomain(new DirectoryContext(DirectoryContextType.Domain)))
//                        {
//                            ActiveDirectoryHelpers._DefaultDomainName = domain.Name;
//                            ActiveDirectoryHelpers._DefaultDomainPDCName = domain.PdcRoleOwner.Name;
//                            ActiveDirectoryHelpers._DefaultDomainDCNames = new System.Collections.Generic.List<string>(domain.DomainControllers.Count);
//                            foreach (DomainController dc in domain.DomainControllers)
//                            {
//                                ActiveDirectoryHelpers._DefaultDomainDCNames.Add(dc.Name);
//                            }
//                        }
//                        ActiveDirectoryHelpers._DefaultDomainQualifiedName = string.Format("DC={0}", ActiveDirectoryHelpers._DefaultDomainName.Replace(".", ",DC="));
//                        ActiveDirectoryHelpers._DefaultLdapPath = string.Format("LDAP://{0}/", ActiveDirectoryHelpers._DefaultDomainPDCName);
//                        using (DirectoryEntry entry = new DirectoryEntry(string.Format("{0}CN=Partitions,CN=Configuration,{1}", ActiveDirectoryHelpers._DefaultLdapPath, ActiveDirectoryHelpers._DefaultDomainQualifiedName)))
//                        {
//                            using (DirectorySearcher searcher = new DirectorySearcher(entry, "(&(objectClass=crossRef)(nETBIOSName=*))", new string[] { "nETBIOSName" }))
//                            {
//                                SearchResult result = searcher.FindOne();
//                                if (result != null)
//                                {
//                                    ActiveDirectoryHelpers._DefaultDomainNetBiosName = result.Properties["nETBIOSName"][0].ToString();
//                                }
//                                else
//                                {
//                                    ActiveDirectoryHelpers._DefaultDomainNetBiosName = ActiveDirectoryHelpers._DefaultDomainQualifiedName;
//                                }
//                            }
//                        }
//                    }
//                    ActiveDirectoryHelpers._DetermineDomainProperties_Loaded = true;
//                }
//            }
//        }
//        #endregion

//        internal static string ConvertBytesToSDDLString(byte[] SID)
//        {
//            SecurityIdentifier sID = new SecurityIdentifier(SID, 0);

//            return sID.ToString();
//        }

//        internal static byte[] ConvertSDDLStringToBytes(string SidSsdlString)
//        {
//            SecurityIdentifier sID = new SecurityIdentifier(SidSsdlString);

//            var sidBytes = new byte[sID.BinaryLength];

//            sID.GetBinaryForm(sidBytes, 0);

//            return sidBytes;
//        }

//        internal static byte[] BuildPrimaryGroupSid(byte[] UserSID, int PrimaryGroupId)
//        {
//            var groupSid = (byte[])UserSID.Clone();

//            int ridOffset = groupSid.Length - 4;
//            int groupId = PrimaryGroupId;
//            for (int i = 0; i < 4; i++)
//            {
//                groupSid[ridOffset + i] = (byte)(groupId & 0xFF);
//                groupId >>= 8;
//            }

//            return groupSid;
//        }

//        internal static string ConvertBytesToBinarySidString(byte[] SID)
//        {
//            StringBuilder escapedSid = new StringBuilder();

//            foreach (var sidByte in SID)
//            {
//                escapedSid.Append('\\');
//                escapedSid.Append(sidByte.ToString("x2"));
//            }

//            return escapedSid.ToString();
//        }

//        internal static string EscapeLdapQuery(string query)
//        {
//            return query.Replace("*", "\\2a").Replace("(", "\\28").Replace(")", "\\29").Replace("\\", "\\5c").Replace("NUL", "\\00").Replace("/", "\\2f");
//        }
//        internal static string FormatGuidForLdapQuery(System.Guid g)
//        {
//            checked
//            {
//                System.Text.StringBuilder sb = new System.Text.StringBuilder();
//                byte[] array = g.ToByteArray();
//                for (int i = 0; i < array.Length; i++)
//                {
//                    byte b = array[i];
//                    sb.Append("\\");
//                    sb.Append(b.ToString("X2"));
//                }
//                return sb.ToString();
//            }
//        }
//    }
//}
