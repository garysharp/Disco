using Disco.Models.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;

namespace Disco.Services.Interop.ActiveDirectory
{
    public class ADMachineAccount : IADObject
    {
        internal static readonly string[] LoadProperties = { "name", "distinguishedName", "sAMAccountName", "objectSid", "dNSHostName", "netbootGUID", "isCriticalSystemObject" };
        internal const string LdapSamAccountNameFilterTemplate = "(&(objectCategory=computer)(sAMAccountName={0}))";
        internal const string LdapNetbootGuidSingleFilterTemplate = "(&(objectCategory=computer)(netbootGUID={0}))";
        internal const string LdapNetbootGuidDoubleFilterTemplate = "(&(objectCategory=computer)(|(netbootGUID={0})(netbootGUID={1})))";

        public ADDomain Domain { get; private set; }

        public string DistinguishedName { get; private set; }
        public SecurityIdentifier SecurityIdentifier { get; private set; }
        public string Id { get { return string.Format(@"{0}\{1}", Domain.NetBiosName, SamAccountName); } }

        public string SamAccountName { get; private set; }

        public string Name { get; private set; }
        public string DisplayName { get { return this.Name; } }

        public string DnsName { get; private set; }
        public Guid NetbootGUID { get; private set; }

        public bool IsCriticalSystemObject { get; private set; }
        public Dictionary<string, object[]> LoadedProperties { get; private set; }

        public string ParentDistinguishedName
        {
            get
            {
                return DistinguishedName.Substring(DistinguishedName.IndexOf(',') + 1);
            }
        }

        private ADMachineAccount(ADDomain Domain, string DistinguishedName, SecurityIdentifier SecurityIdentifier, string SamAccountName, string Name, string DnsName, Guid NetbootGUID, bool IsCriticalSystemObject, Dictionary<string, object[]> LoadedProperties)
        {
            this.Domain = Domain;
            this.DistinguishedName = DistinguishedName;
            this.SecurityIdentifier = SecurityIdentifier;
            this.SamAccountName = SamAccountName;
            this.Name = Name;
            this.DnsName = DnsName;
            this.NetbootGUID = NetbootGUID;
            this.IsCriticalSystemObject = IsCriticalSystemObject;
            this.LoadedProperties = LoadedProperties;
        }

        public static ADMachineAccount FromSearchResult(ADSearchResult SearchResult, string[] AdditionalProperties)
        {
            if (SearchResult == null)
                throw new ArgumentNullException("SearchResult");

            string name = SearchResult.Value<string>("name");
            string sAMAccountName = SearchResult.Value<string>("sAMAccountName");
            string distinguishedName = SearchResult.Value<string>("distinguishedName");
            var objectSid = new SecurityIdentifier(SearchResult.Value<byte[]>("objectSid"), 0);

            var dNSName = SearchResult.Value<string>("dNSHostName");
            if (dNSName == null)
                dNSName = string.Format("{0}.{1}", sAMAccountName.TrimEnd('$'), SearchResult.Domain.Name);

            bool isCriticalSystemObject = SearchResult.Value<bool>("isCriticalSystemObject");

            var netbootGUID = default(Guid);
            byte[] netbootGuidBytes = SearchResult.Value<byte[]>("netbootGUID");
            if (netbootGuidBytes != null)
                netbootGUID = new Guid(netbootGuidBytes);

            // Additional Properties
            Dictionary<string, object[]> additionalProperties;
            if (AdditionalProperties != null)
                additionalProperties = AdditionalProperties
                    .Select(p => Tuple.Create(p, SearchResult.Values<object>(p).ToArray()))
                    .ToDictionary(t => t.Item1, t => t.Item2);
            else
            {
                additionalProperties = new Dictionary<string, object[]>();
            }

            return new ADMachineAccount(
                SearchResult.Domain,
                distinguishedName,
                objectSid,
                sAMAccountName,
                name,
                dNSName,
                netbootGUID,
                isCriticalSystemObject,
                additionalProperties);
        }

        public User ToRepositoryUser()
        {
            return new User
            {
                UserId = this.Id,
                DisplayName = this.Name
            };
        }

        public object GetPropertyValue(string PropertyName, int Index = 0)
        {
            switch (PropertyName.ToLower())
            {
                case "name":
                    return this.Name;
                case "samaccountname":
                    return this.SamAccountName;
                case "distinguishedname":
                    return this.DistinguishedName;
                case "objectsid":
                    return this.SecurityIdentifier.ToString();
                case "netbootguid":
                    return this.NetbootGUID;
                default:
                    object[] adProperty;
                    if (this.LoadedProperties.TryGetValue(PropertyName, out adProperty) && Index <= adProperty.Length)
                        return adProperty[Index];
                    else
                        return null;
            }
        }

        #region Actions

        public void DeleteAccount(ADDomainController WritableDomainController)
        {
            if (this.IsCriticalSystemObject)
                throw new InvalidOperationException(string.Format("This account [{0}] is a Critical System Active Directory Object and Disco refuses to modify it", this.DistinguishedName));

            if (!WritableDomainController.IsWritable)
                throw new InvalidOperationException(string.Format("The domain controller [{0}] is not writable. This action (Offline Domain Join Provision) requires a writable domain controller.", this.Name));

            using (ADDirectoryEntry entry = WritableDomainController.RetrieveDirectoryEntry(this.DistinguishedName))
            {
                entry.Entry.DeleteTree();
            }
        }
        public void DeleteAccount()
        {
            this.DeleteAccount(Domain.GetAvailableDomainController(RequireWritable: true));
        }

        private void SetNetbootGUID(ADDomainController WritableDomainController, System.Guid updatedNetbootGUID)
        {
            if (this.IsCriticalSystemObject)
                throw new InvalidOperationException(string.Format("This account {0} is a Critical System Active Directory Object and Disco refuses to modify it", this.DistinguishedName));

            using (var deAccount = WritableDomainController.RetrieveDirectoryEntry(this.DistinguishedName))
            {
                var netbootGUIDProp = deAccount.Entry.Properties["netbootGUID"];
                bool flag = netbootGUIDProp.Count > 0;
                if (flag)
                {
                    netbootGUIDProp.Clear();
                }
                netbootGUIDProp.Add(updatedNetbootGUID.ToByteArray());
                deAccount.Entry.CommitChanges();
            }
        }
        public void SetDescription(ADDomainController WritableDomainController, string Description)
        {
            using (var deAccount = WritableDomainController.RetrieveDirectoryEntry(this.DistinguishedName))
            {
                var descriptionProp = deAccount.Entry.Properties["description"];
                if (descriptionProp.Count > 0)
                {
                    descriptionProp.Clear();
                }
                if (!string.IsNullOrEmpty(Description))
                {
                    descriptionProp.Add(Description);
                }
                deAccount.Entry.CommitChanges();
            }
        }
        public void SetDescription(string Description)
        {
            this.SetDescription(Domain.GetAvailableDomainController(RequireWritable: true), Description);
        }

        public void SetDescription(ADDomainController WritableDomainController, Device Device)
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

            this.SetDescription(WritableDomainController, description);
        }
        public void SetDescription(Device Device)
        {
            this.SetDescription(Domain.GetAvailableDomainController(RequireWritable: true), Device);
        }

        public void DisableAccount(ADDomainController WritableDomainController)
        {
            if (this.IsCriticalSystemObject)
                throw new InvalidOperationException(string.Format("This account {0} is a Critical System Active Directory Object and Disco refuses to modify it", this.DistinguishedName));

            using (var deAccount = WritableDomainController.RetrieveDirectoryEntry(this.DistinguishedName))
            {
                int accountControl = (int)deAccount.Entry.Properties["userAccountControl"][0];
                int updatedAccountControl = (accountControl | 2);
                if (accountControl != updatedAccountControl)
                {
                    deAccount.Entry.Properties["userAccountControl"][0] = updatedAccountControl;
                    deAccount.Entry.CommitChanges();
                }
            }
        }
        public void DisableAccount()
        {
            this.DisableAccount(Domain.GetAvailableDomainController(RequireWritable: true));
        }

        public void EnableAccount(ADDomainController WritableDomainController)
        {
            if (this.IsCriticalSystemObject)
                throw new InvalidOperationException(string.Format("This account {0} is a Critical System Active Directory Object and Disco refuses to modify it", this.DistinguishedName));

            using (var deAccount = WritableDomainController.RetrieveDirectoryEntry(this.DistinguishedName))
            {
                int accountControl = (int)deAccount.Entry.Properties["userAccountControl"][0];
                if ((accountControl & 2) == 2)
                {
                    int updatedAccountControl = (accountControl ^ 2);
                    deAccount.Entry.Properties["userAccountControl"][0] = updatedAccountControl;
                    deAccount.Entry.CommitChanges();
                }
            }
        }
        public void EnableAccount()
        {
            this.EnableAccount(Domain.GetAvailableDomainController(RequireWritable: true));
        }

        public bool UpdateNetbootGUID(ADDomainController WritableDomainController, string UUID, string MACAddress)
        {
            if (this.IsCriticalSystemObject)
                throw new InvalidOperationException(string.Format("This account {0} is a Critical System Active Directory Object and Disco refuses to modify it", this.DistinguishedName));

            Guid netbootGUID = Guid.Empty;

            if (!string.IsNullOrWhiteSpace(UUID))
            {
                netbootGUID = NetbootGUIDFromUUID(UUID);
            }
            else if (!string.IsNullOrWhiteSpace(MACAddress))
            {
                netbootGUID = NetbootGUIDFromMACAddress(MACAddress);
            }

            if (netbootGUID != System.Guid.Empty && netbootGUID != this.NetbootGUID)
            {
                this.SetNetbootGUID(WritableDomainController, netbootGUID);
                return true;
            }
            else
            {
                return false;
            }
        }
        public void UpdateNetbootGUID(string UUID, string MACAddress)
        {
            this.UpdateNetbootGUID(Domain.GetAvailableDomainController(RequireWritable: true), UUID, MACAddress);
        }
        public static System.Guid NetbootGUIDFromUUID(string UUID)
        {
            return new Guid(UUID);
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

        public void MoveOrganisationalUnit(ADDomainController WritableDomainController, string NewOrganisationUnit)
        {
            if (this.IsCriticalSystemObject)
                throw new InvalidOperationException(string.Format("This account {0} is a Critical System Active Directory Object and Disco refuses to modify it", this.DistinguishedName));

            var parentDistinguishedName = this.ParentDistinguishedName;

            if (parentDistinguishedName != null && !parentDistinguishedName.Equals(NewOrganisationUnit, StringComparison.OrdinalIgnoreCase))
            {
                // If no OU provided, place in default Computers container
                if (string.IsNullOrWhiteSpace(NewOrganisationUnit))
                    NewOrganisationUnit = Domain.DefaultComputerContainer;

                if (!NewOrganisationUnit.EndsWith(Domain.DistinguishedName, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException(string.Format("Unable to move AD Account from one domain [{0}] to another [{1}].", this.DistinguishedName, NewOrganisationUnit));

                using (ADDirectoryEntry ou = WritableDomainController.RetrieveDirectoryEntry(NewOrganisationUnit))
                {
                    using (ADDirectoryEntry i = WritableDomainController.RetrieveDirectoryEntry(this.DistinguishedName))
                    {
                        i.Entry.UsePropertyCache = false;
                        i.Entry.MoveTo(ou.Entry);
                        
                        // Update Distinguished Name
                        this.DistinguishedName = i.Entry.Properties["distinguishedName"][0].ToString();
                    }
                }
            }
        }
        public void MoveOrganisationalUnit(string NewOrganisationUnit)
        {
            this.MoveOrganisationalUnit(Domain.GetAvailableDomainController(RequireWritable: true), NewOrganisationUnit);
        }

        #endregion

        public override string ToString()
        {
            return this.Id;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is ADMachineAccount))
                return false;
            else
                return this.DistinguishedName == ((ADMachineAccount)obj).DistinguishedName;
        }
        public override int GetHashCode()
        {
            return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(this.DistinguishedName);
        }
    }
}
