using Disco.Models.Interop.ActiveDirectory;
using Disco.Models.Repository;
using System;
using System.Collections;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Text;
using System.Net.NetworkInformation;
using System.Management;

namespace Disco.BI.Interop.ActiveDirectory
{
    public static class ActiveDirectoryMachineAccountExtensions
    {
        public static void DeleteAccount(this ActiveDirectoryMachineAccount account)
        {
            if (account.IsCriticalSystemObject)
                throw new InvalidOperationException(string.Format("This account {0} is a Critical System Active Directory Object and Disco refuses to modify it", account.DistinguishedName));

            using (DirectoryEntry machineDE = new DirectoryEntry(account.Path))
            {
                DeleteAccountRecursive(machineDE);

                using (var machineDEParent = machineDE.Parent)
                {
                    machineDEParent.Children.Remove(machineDE);
                }
            }
        }
        private static void DeleteAccountRecursive(DirectoryEntry parent)
        {
            List<DirectoryEntry> children = new List<DirectoryEntry>();
            foreach (DirectoryEntry child in parent.Children)
                children.Add(child);

            foreach (var child in children)
            {
                DeleteAccountRecursive(child);
                parent.Children.Remove(child);
                child.Dispose();
            }
        }
        private static void SetNetbootGUID(this ActiveDirectoryMachineAccount account, System.Guid updatedNetbootGUID)
        {
            if (account.IsCriticalSystemObject)
                throw new InvalidOperationException(string.Format("This account {0} is a Critical System Active Directory Object and Disco refuses to modify it", account.DistinguishedName));

            using (DirectoryEntry machineDE = new DirectoryEntry(account.Path))
            {
                PropertyValueCollection netbootGUIDProp = machineDE.Properties["netbootGUID"];
                bool flag = netbootGUIDProp.Count > 0;
                if (flag)
                {
                    netbootGUIDProp.Clear();
                }
                netbootGUIDProp.Add(updatedNetbootGUID.ToByteArray());
                machineDE.CommitChanges();
            }
        }
        public static void SetDescription(this ActiveDirectoryMachineAccount account, string Description)
        {
            using (DirectoryEntry machineDE = new DirectoryEntry(account.Path))
            {
                PropertyValueCollection descriptionProp = machineDE.Properties["description"];
                bool flag = descriptionProp.Count > 0;
                if (flag)
                {
                    descriptionProp.Clear();
                }
                flag = !string.IsNullOrEmpty(Description);
                if (flag)
                {
                    descriptionProp.Add(Description);
                }
                machineDE.CommitChanges();
            }
        }
        public static void SetDescription(this ActiveDirectoryMachineAccount account, Device Device)
        {
            System.Text.StringBuilder descriptionBuilder = new System.Text.StringBuilder();
            bool flag = Device.AssignedUserId != null;
            if (flag)
            {
                descriptionBuilder.Append(Device.AssignedUser.Id);
                descriptionBuilder.Append(" (");
                descriptionBuilder.Append(Device.AssignedUser.DisplayName);
                descriptionBuilder.Append("); ");
            }
            flag = Device.DeviceModelId.HasValue;
            if (flag)
            {
                descriptionBuilder.Append(Device.DeviceModel.Description);
                descriptionBuilder.Append("; ");
            }
            descriptionBuilder.Append(Device.DeviceProfile.Description);
            descriptionBuilder.Append("; ");
            string description = descriptionBuilder.ToString().Trim();
            flag = (description.Length > 1024);
            if (flag)
            {
                description = description.Substring(0, 1024);
            }
            account.SetDescription(description);
        }

        public static void DisableAccount(this ActiveDirectoryMachineAccount account)
        {
            if (account.IsCriticalSystemObject)
                throw new InvalidOperationException(string.Format("This account {0} is a Critical System Active Directory Object and Disco refuses to modify it", account.DistinguishedName));

            using (DirectoryEntry machineDE = new DirectoryEntry(account.Path))
            {
                int accountControl = (int)machineDE.Properties["userAccountControl"][0];
                int updatedAccountControl = (accountControl | 2);
                if (accountControl != updatedAccountControl)
                {
                    machineDE.Properties["userAccountControl"][0] = updatedAccountControl;
                    machineDE.CommitChanges();
                }
            }
        }
        public static void EnableAccount(this ActiveDirectoryMachineAccount account)
        {
            if (account.IsCriticalSystemObject)
                throw new InvalidOperationException(string.Format("This account {0} is a Critical System Active Directory Object and Disco refuses to modify it", account.DistinguishedName));

            using (DirectoryEntry machineDE = new DirectoryEntry(account.Path))
            {
                int accountControl = (int)machineDE.Properties["userAccountControl"][0];
                if ((accountControl & 2) == 2)
                {
                    int updatedAccountControl = (accountControl ^ 2);
                    machineDE.Properties["userAccountControl"][0] = updatedAccountControl;
                    machineDE.CommitChanges();
                }
            }
        }

        public static bool UpdateNetbootGUID(this ActiveDirectoryMachineAccount account, string UUID, string MACAddress)
        {
            if (account.IsCriticalSystemObject)
                throw new InvalidOperationException(string.Format("This account {0} is a Critical System Active Directory Object and Disco refuses to modify it", account.DistinguishedName));

            System.Guid netbootGUID = System.Guid.Empty;
            bool flag = !string.IsNullOrWhiteSpace(UUID);
            if (flag)
            {
                netbootGUID = ActiveDirectoryMachineAccountExtensions.NetbootGUIDFromUUID(UUID);
            }
            else
            {
                flag = !string.IsNullOrWhiteSpace(MACAddress);
                if (flag)
                {
                    netbootGUID = ActiveDirectoryMachineAccountExtensions.NetbootGUIDFromMACAddress(MACAddress);
                }
            }
            flag = (netbootGUID != System.Guid.Empty && netbootGUID != account.NetbootGUID);
            bool UpdateNetbootGUID;
            if (flag)
            {
                account.SetNetbootGUID(netbootGUID);
                UpdateNetbootGUID = true;
            }
            else
            {
                UpdateNetbootGUID = false;
            }
            return UpdateNetbootGUID;
        }
        internal static System.Guid NetbootGUIDFromMACAddress(string MACAddress)
        {
            string strippedMACAddress = MACAddress.Trim().Replace(":", string.Empty).Replace("-", string.Empty);
            bool flag = strippedMACAddress.Length == 12;
            System.Guid NetbootGUIDFromMACAddress;
            if (flag)
            {
                System.Guid guid = new System.Guid(string.Format("00000000-0000-0000-0000-{0}", strippedMACAddress));
                NetbootGUIDFromMACAddress = guid;
            }
            else
            {
                NetbootGUIDFromMACAddress = System.Guid.Empty;
            }
            return NetbootGUIDFromMACAddress;
        }
        internal static System.Guid NetbootGUIDFromUUID(string UUID)
        {
            System.Guid result = new System.Guid(UUID);
            return result;
        }

        public static object GetPropertyValue(this ActiveDirectoryMachineAccount account, string PropertyName, int Index = 0)
        {
            switch (PropertyName.ToLower())
            {
                case "name":
                    return account.Name;
                case "samaccountname":
                    return account.sAMAccountName;
                case "distinguishedname":
                    return account.DistinguishedName;
                case "objectsid":
                    return account.ObjectSid;
                case "netbootguid":
                    return account.NetbootGUID;
                default:
                    object[] adProperty;
                    if (account.LoadedProperties.TryGetValue(PropertyName, out adProperty) && Index <= adProperty.Length)
                        return adProperty[Index];
                    else
                        return null;
            }
        }

        public static IPStatus PingComputer(this ActiveDirectoryMachineAccount account, int Timeout = 2000)
        {
            using (var p = new Ping())
            {
                PingReply reply = p.Send(account.DnsName, Timeout);
                return reply.Status;
            }
        }

        // Didn't Work - WMI Limitation?
        // G# - 2012-06-18
        //public static void OnlineRenameComputer(this ActiveDirectoryMachineAccount account, string NewComputerName)
        //{
        //    if (account.IsCriticalSystemObject)
        //        throw new InvalidOperationException(string.Format("This account {0} is a Critical System Active Directory Object and Disco refuses to modify it", account.DistinguishedName));

        //    try
        //    {
        //        IPStatus pingResult = account.PingComputer();
        //        if (pingResult != IPStatus.Success)
        //            throw new Exception(string.Format("Ping Error Result: {0}", pingResult.ToString()));
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(string.Format("Error trying to Ping the Device: {0}; {1}", account.DnsName, ex.Message), ex);
        //    }

        //    ConnectionOptions wmiConnectionOptions = new ConnectionOptions()
        //    {
        //        Authentication = AuthenticationLevel.PacketPrivacy,
        //        Impersonation = ImpersonationLevel.Impersonate,
        //        EnablePrivileges = true,
        //        Timeout = new TimeSpan(0, 0, 6)
        //    };
        //    ManagementPath wmiPath = new ManagementPath()
        //    {
        //        Server = account.DnsName,
        //        NamespacePath = @"root\cimv2",
        //        ClassName = "Win32_ComputerSystem"
        //    };

        //    ManagementScope wmiScope = new ManagementScope(wmiPath, wmiConnectionOptions);

        //    ObjectGetOptions wmiGetOptions = new ObjectGetOptions() { Timeout = new TimeSpan(0, 1, 0) };

        //    using (ManagementClass wmiClass = new ManagementClass(wmiScope, wmiPath, wmiGetOptions))
        //    {
        //        foreach (ManagementObject wmiWin32ComputerSystem in wmiClass.GetInstances())
        //        {
        //            UInt32 result = (UInt32)wmiWin32ComputerSystem.InvokeMethod("Rename", new object[] { NewComputerName });
        //            if (result != 0)
        //                throw new Exception(string.Format("Error Renaming Computer; WMI Remote Method 'Rename' returned: {0}", result));
        //        }
        //    }
        //}

        public static void MoveOrganisationUnit(this ActiveDirectoryMachineAccount account, string NewOrganisationUnit)
        {
            if (account.IsCriticalSystemObject)
                throw new InvalidOperationException(string.Format("This account {0} is a Critical System Active Directory Object and Disco refuses to modify it", account.DistinguishedName));

            if (!account.ParentDistinguishedName.Equals(NewOrganisationUnit, StringComparison.InvariantCultureIgnoreCase))
            {
                string ouPath;
                if (string.IsNullOrWhiteSpace(NewOrganisationUnit))
                    ouPath = string.Format("{0}CN=Computers,{1}", ActiveDirectoryHelpers.DefaultLdapPath, ActiveDirectoryHelpers.DefaultDomainQualifiedName);
                else
                    ouPath = string.Format("{0}{1},{2}", ActiveDirectoryHelpers.DefaultLdapPath, NewOrganisationUnit, ActiveDirectoryHelpers.DefaultDomainQualifiedName);

                using (DirectoryEntry ou = new DirectoryEntry(ouPath))
                {
                    using (DirectoryEntry i = new DirectoryEntry(account.Path) { UsePropertyCache = false })
                    {
                        i.MoveTo(ou);
                    }
                }
            }
        }

    }
}
