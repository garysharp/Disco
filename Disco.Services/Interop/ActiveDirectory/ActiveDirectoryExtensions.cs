using Disco.Models.Interop.ActiveDirectory;
using Disco.Models.Repository;
using Disco.Services.Interop.ActiveDirectory.Internal;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Services.Interop.ActiveDirectory
{
    public static class ActiveDirectoryExtensions
    {
        #region Domain/Directory Extensions

        public static DomainController RetrieveWritableDomainController(this ActiveDirectoryDomain domain)
        {
            return ADInterop.RetrieveWritableDomainController(domain);
        }

        public static IEnumerable<DomainController> RetrieveReachableDomainControllers(this ActiveDirectorySite site, ActiveDirectoryDomain domain)
        {
            return site.Servers.OfType<DomainController>().Where(dc => dc.Reachable() && dc.Domain.Name.Equals(domain.DnsName));
        }

        public static IEnumerable<DomainController> RetrieveReachableDomainControllers(this ActiveDirectoryDomain domain)
        {
            var d = Domain.GetDomain(new DirectoryContext(DirectoryContextType.Domain, domain.DnsName));
            return d.FindAllDomainControllers().OfType<DomainController>().Where(dc => dc.Reachable());
        }

        public static bool Reachable(this DirectoryServer ds)
        {
            using (Ping p = new Ping())
            {
                var pr = p.Send(ds.Name, 500);
                return (pr.Status == IPStatus.Success);
            }
        }

        public static string GetFriendlyOrganisationalUnitName(this ActiveDirectoryDomain domain, string DistinguishedName)
        {
            if (!DistinguishedName.EndsWith(domain.DistinguishedName, StringComparison.InvariantCultureIgnoreCase))
                throw new ArgumentException(string.Format("The Distinguished Name [{0}] doesn't exist within this domain [{1}]", DistinguishedName, domain.DistinguishedName));

            StringBuilder name = new StringBuilder();

            name.Append('[').Append(domain.NetBiosName).Append(']');

            var subDN = DistinguishedName.Substring(0, DistinguishedName.Length - domain.DistinguishedName.Length);
            var subDNComponents = subDN.Split(',');

            subDNComponents
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Reverse()
                .Select(c => c.Substring(c.IndexOf('=') + 1))
                .ToList()
                .ForEach(c => name.Append(" > ").Append(c));

            return name.ToString();
        }

        public static string GetDefaultComputerContainer(this ActiveDirectoryDomain domain)
        {
            return string.Format("CN=Computers,{0}", domain.DistinguishedName);
        }

        #endregion

        #region User Account Extensions
        public static object GetPropertyValue(this ActiveDirectoryUserAccount account, string PropertyName, int Index = 0)
        {
            switch (PropertyName.ToLower())
            {
                case "name":
                    return account.Name;
                case "samaccountname":
                    return account.SamAccountName;
                case "distinguishedname":
                    return account.DistinguishedName;
                case "objectsid":
                    return account.SecurityIdentifier;
                case "sn":
                    return account.Surname;
                case "givenname":
                    return account.GivenName;
                case "mail":
                    return account.Email;
                case "telephonenumber":
                    return account.Phone;
                default:
                    object[] adProperty;
                    if (account.LoadedProperties.TryGetValue(PropertyName, out adProperty) && Index <= adProperty.Length)
                        return adProperty[Index];
                    else
                        return null;
            }
        }
        #endregion

        #region Machine Account Extensions

        public static void DeleteAccount(this ActiveDirectoryMachineAccount account, DomainController DomainController)
        {
            if (account.IsCriticalSystemObject)
                throw new InvalidOperationException(string.Format("This account {0} is a Critical System Active Directory Object and Disco refuses to modify it", account.DistinguishedName));

            using (DirectoryEntry deAccount = DomainController.RetrieveDirectoryEntry(account.DistinguishedName))
            {
                deAccount.DeleteObjectRecursive();
            }
        }
        public static void DeleteAccount(this ActiveDirectoryMachineAccount account)
        {
            var domain = account.GetDomain();

            using (var domainController = domain.RetrieveWritableDomainController())
            {
                account.DeleteAccount(domainController);
            }
        }

        private static void SetNetbootGUID(this ActiveDirectoryMachineAccount account, DomainController DomainController, System.Guid updatedNetbootGUID)
        {
            if (account.IsCriticalSystemObject)
                throw new InvalidOperationException(string.Format("This account {0} is a Critical System Active Directory Object and Disco refuses to modify it", account.DistinguishedName));

            using (DirectoryEntry deAccount = DomainController.RetrieveDirectoryEntry(account.DistinguishedName))
            {
                PropertyValueCollection netbootGUIDProp = deAccount.Properties["netbootGUID"];
                bool flag = netbootGUIDProp.Count > 0;
                if (flag)
                {
                    netbootGUIDProp.Clear();
                }
                netbootGUIDProp.Add(updatedNetbootGUID.ToByteArray());
                deAccount.CommitChanges();
            }
        }
        public static void SetDescription(this ActiveDirectoryMachineAccount account, DomainController DomainController, string Description)
        {
            using (DirectoryEntry deAccount = DomainController.RetrieveDirectoryEntry(account.DistinguishedName))
            {
                PropertyValueCollection descriptionProp = deAccount.Properties["description"];
                if (descriptionProp.Count > 0)
                {
                    descriptionProp.Clear();
                }
                if (!string.IsNullOrEmpty(Description))
                {
                    descriptionProp.Add(Description);
                }
                deAccount.CommitChanges();
            }
        }
        public static void SetDescription(this ActiveDirectoryMachineAccount account, string Description)
        {
            var domain = account.GetDomain();

            using (var domainController = domain.RetrieveWritableDomainController())
            {
                account.SetDescription(domainController, Description);
            }
        }

        public static void SetDescription(this ActiveDirectoryMachineAccount account, DomainController DomainController, Device Device)
        {
            System.Text.StringBuilder descriptionBuilder = new System.Text.StringBuilder();

            if (Device.AssignedUserId != null)
            {
                descriptionBuilder.Append(Device.AssignedUser.UserId).Append(" (").Append(Device.AssignedUser.DisplayName).Append("); ");
            }

            if (Device.DeviceModelId.HasValue)
            {
                descriptionBuilder.Append(Device.DeviceModel.Description).Append("; ");
            }

            descriptionBuilder.Append(Device.DeviceProfile.Description).Append(";");

            string description = descriptionBuilder.ToString().Trim();
            if (description.Length > 1024)
                description = description.Substring(0, 1024);

            account.SetDescription(DomainController, description);
        }
        public static void SetDescription(this ActiveDirectoryMachineAccount account, Device Device)
        {
            var domain = account.GetDomain();

            using (var domainController = domain.RetrieveWritableDomainController())
            {
                account.SetDescription(domainController, Device);
            }
        }

        public static void DisableAccount(this ActiveDirectoryMachineAccount account, DomainController DomainController)
        {
            if (account.IsCriticalSystemObject)
                throw new InvalidOperationException(string.Format("This account {0} is a Critical System Active Directory Object and Disco refuses to modify it", account.DistinguishedName));

            using (DirectoryEntry deAccount = DomainController.RetrieveDirectoryEntry(account.DistinguishedName))
            {
                int accountControl = (int)deAccount.Properties["userAccountControl"][0];
                int updatedAccountControl = (accountControl | 2);
                if (accountControl != updatedAccountControl)
                {
                    deAccount.Properties["userAccountControl"][0] = updatedAccountControl;
                    deAccount.CommitChanges();
                }
            }
        }
        public static void DisableAccount(this ActiveDirectoryMachineAccount account)
        {
            var domain = account.GetDomain();

            using (var domainController = domain.RetrieveWritableDomainController())
            {
                account.DisableAccount(domainController);
            }
        }
        public static void EnableAccount(this ActiveDirectoryMachineAccount account, DomainController DomainController)
        {
            if (account.IsCriticalSystemObject)
                throw new InvalidOperationException(string.Format("This account {0} is a Critical System Active Directory Object and Disco refuses to modify it", account.DistinguishedName));

            using (DirectoryEntry deAccount = DomainController.RetrieveDirectoryEntry(account.DistinguishedName))
            {
                int accountControl = (int)deAccount.Properties["userAccountControl"][0];
                if ((accountControl & 2) == 2)
                {
                    int updatedAccountControl = (accountControl ^ 2);
                    deAccount.Properties["userAccountControl"][0] = updatedAccountControl;
                    deAccount.CommitChanges();
                }
            }
        }
        public static void EnableAccount(this ActiveDirectoryMachineAccount account)
        {
            var domain = account.GetDomain();

            using (var domainController = domain.RetrieveWritableDomainController())
            {
                account.EnableAccount(domainController);
            }
        }

        public static bool UpdateNetbootGUID(this ActiveDirectoryMachineAccount account, DomainController DomainController, string UUID, string MACAddress)
        {
            if (account.IsCriticalSystemObject)
                throw new InvalidOperationException(string.Format("This account {0} is a Critical System Active Directory Object and Disco refuses to modify it", account.DistinguishedName));

            Guid netbootGUID = Guid.Empty;

            if (!string.IsNullOrWhiteSpace(UUID))
            {
                netbootGUID = ActiveDirectoryExtensions.NetbootGUIDFromUUID(UUID);
            }
            else if (!string.IsNullOrWhiteSpace(MACAddress))
            {
                netbootGUID = ActiveDirectoryExtensions.NetbootGUIDFromMACAddress(MACAddress);
            }

            if (netbootGUID != System.Guid.Empty && netbootGUID != account.NetbootGUID)
            {
                account.SetNetbootGUID(DomainController, netbootGUID);
                return true;
            }
            else
            {
                return false;
            }
        }
        public static bool UpdateNetbootGUID(this ActiveDirectoryMachineAccount account, string UUID, string MACAddress)
        {
            var domain = account.GetDomain();

            using (var domainController = domain.RetrieveWritableDomainController())
            {
                return account.UpdateNetbootGUID(domainController, UUID, MACAddress);
            }
        }
        public static System.Guid NetbootGUIDFromMACAddress(string MACAddress)
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
        public static System.Guid NetbootGUIDFromUUID(string UUID)
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
                    return account.SamAccountName;
                case "distinguishedname":
                    return account.DistinguishedName;
                case "objectsid":
                    return account.SecurityIdentifier;
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

        public static void MoveOrganisationalUnit(this ActiveDirectoryMachineAccount account, DomainController DomainController, string NewOrganisationUnit)
        {
            if (account.IsCriticalSystemObject)
                throw new InvalidOperationException(string.Format("This account {0} is a Critical System Active Directory Object and Disco refuses to modify it", account.DistinguishedName));

            var parentDistinguishedName = account.ParentDistinguishedName();

            if (parentDistinguishedName != null && !parentDistinguishedName.Equals(NewOrganisationUnit, StringComparison.InvariantCultureIgnoreCase))
            {
                var domain = account.GetDomain();

                // If no OU provided, place in default Computers container
                if (string.IsNullOrWhiteSpace(NewOrganisationUnit))
                    NewOrganisationUnit = domain.GetDefaultComputerContainer();

                if (!NewOrganisationUnit.EndsWith(domain.DistinguishedName, StringComparison.InvariantCultureIgnoreCase))
                    throw new InvalidOperationException(string.Format("Unable to move AD Account from one domain [{0}] to another [{1}].", account.DistinguishedName, NewOrganisationUnit));

                using (DirectoryEntry ou = DomainController.RetrieveDirectoryEntry(NewOrganisationUnit))
                {
                    using (DirectoryEntry i = DomainController.RetrieveDirectoryEntry(account.DistinguishedName))
                    {
                        i.UsePropertyCache = false;
                        i.MoveTo(ou);
                    }
                }
            }
        }

        public static string ParentDistinguishedName(this ActiveDirectoryMachineAccount account)
        {
            // Determine Parent
            if (!string.IsNullOrWhiteSpace(account.DistinguishedName))
                return account.DistinguishedName.Substring(account.DistinguishedName.IndexOf(",") + 1);
            else
                return null;
        }

        public static ActiveDirectoryDomain GetDomain(this ActiveDirectoryMachineAccount account)
        {
            var domain = ActiveDirectory.GetDomainByNetBiosName(account.Domain);

            if (domain == null)
                throw new InvalidOperationException(string.Format("Unable to find Domain [{0}] for account [{1}]", account.Domain, account.Name));
            else
                return domain;
        }

        #endregion
    }
}
