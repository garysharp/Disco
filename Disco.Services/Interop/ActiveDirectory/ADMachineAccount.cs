using Disco.Models.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;

namespace Disco.Services.Interop.ActiveDirectory
{
    public class ADMachineAccount : IADObject
    {
        internal static readonly string[] LoadProperties = { "name", "distinguishedName", "sAMAccountName", "objectSid", "userAccountControl", "dNSHostName", "description", "netbootGUID", "isCriticalSystemObject" };
        internal const string LdapSamAccountNameFilterTemplate = "(&(objectCategory=computer)(sAMAccountName={0}))";
        internal const string LdapNetbootGuidSingleFilterTemplate = "(&(objectCategory=computer)(netbootGUID={0}))";
        internal const string LdapNetbootGuidDoubleFilterTemplate = "(&(objectCategory=computer)(|(netbootGUID={0})(netbootGUID={1})))";

        public ADDomain Domain { get; private set; }

        public string DistinguishedName { get; private set; }
        public SecurityIdentifier SecurityIdentifier { get; private set; }
        public string Id { get { return string.Format(@"{0}\{1}", Domain.NetBiosName, SamAccountName); } }

        public string SamAccountName { get; private set; }

        public string Name { get; private set; }
        public string DisplayName { get { return Name; } }
        public string Description { get; private set; }

        public string DnsName { get; private set; }
        public Guid NetbootGUID { get; private set; }

        public ADUserAccountControlFlags UserAccountControl { get; private set; }

        public bool IsCriticalSystemObject { get; private set; }

        public Dictionary<string, object[]> LoadedProperties { get; private set; }

        public bool IsDisabled { get { return UserAccountControl.HasFlag(ADUserAccountControlFlags.ADS_UF_ACCOUNTDISABLE); } }

        public string ParentDistinguishedName
        {
            get
            {
                return DistinguishedName.Substring(DistinguishedName.IndexOf(',') + 1);
            }
        }

        private ADMachineAccount(ADDomain Domain, string DistinguishedName, SecurityIdentifier SecurityIdentifier, string SamAccountName, string Name, string Description, string DnsName, Guid NetbootGUID, ADUserAccountControlFlags UserAccountControl, bool IsCriticalSystemObject, Dictionary<string, object[]> LoadedProperties)
        {
            this.Domain = Domain;
            this.DistinguishedName = DistinguishedName;
            this.SecurityIdentifier = SecurityIdentifier;
            this.SamAccountName = SamAccountName;
            this.Name = Name;
            this.Description = Description;
            this.DnsName = DnsName;
            this.NetbootGUID = NetbootGUID;
            this.UserAccountControl = UserAccountControl;
            this.IsCriticalSystemObject = IsCriticalSystemObject;
            this.LoadedProperties = LoadedProperties;
        }

        public static ADMachineAccount FromSearchResult(ADSearchResult SearchResult, string[] AdditionalProperties)
        {
            if (SearchResult == null)
                throw new ArgumentNullException("SearchResult");

            var name = SearchResult.Value<string>("name");
            var description = SearchResult.Value<string>("description");
            var sAMAccountName = SearchResult.Value<string>("sAMAccountName");
            var distinguishedName = SearchResult.Value<string>("distinguishedName");
            var objectSid = new SecurityIdentifier(SearchResult.Value<byte[]>("objectSid"), 0);

            var dNSName = SearchResult.Value<string>("dNSHostName");
            if (dNSName == null)
                dNSName = string.Format("{0}.{1}", sAMAccountName.TrimEnd('$'), SearchResult.Domain.Name);

            var userAccountControl = (ADUserAccountControlFlags)SearchResult.Value<int>("userAccountControl");
            var isCriticalSystemObject = SearchResult.Value<bool>("isCriticalSystemObject");

            var netbootGUID = default(Guid);
            var netbootGuidBytes = SearchResult.Value<byte[]>("netbootGUID");
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
                description,
                dNSName,
                netbootGUID,
                userAccountControl,
                isCriticalSystemObject,
                additionalProperties);
        }

        public User ToRepositoryUser()
        {
            return new User
            {
                UserId = Id,
                DisplayName = Name
            };
        }

        [Obsolete("Use generic equivalents: GetPropertyValue<T>(string PropertyName)")]
        public object GetPropertyValue(string PropertyName, int Index = 0)
        {
            return GetPropertyValues<object>(PropertyName).Skip(Index).FirstOrDefault();
        }
        public T GetPropertyValue<T>(string PropertyName)
        {
            return GetPropertyValues<T>(PropertyName).FirstOrDefault();
        }
        public IEnumerable<T> GetPropertyValues<T>(string PropertyName)
        {
            switch (PropertyName.ToLower())
            {
                case "name":
                    return new string[] { Name }.OfType<T>();
                case "description":
                    return new string[] { Description }.OfType<T>();
                case "samaccountname":
                    return new string[] { SamAccountName }.OfType<T>();
                case "distinguishedname":
                    return new string[] { DistinguishedName }.OfType<T>();
                case "objectsid":
                    return new SecurityIdentifier[] { SecurityIdentifier }.OfType<T>();
                case "netbootguid":
                    return new Guid[] { NetbootGUID }.OfType<T>();
                case "userAccountControl":
                    return new int[] { (int)UserAccountControl }.OfType<T>();
                default:
                    object[] adProperty;
                    if (LoadedProperties.TryGetValue(PropertyName, out adProperty))
                        return adProperty.OfType<T>();
                    else
                        return Enumerable.Empty<T>();
            }
        }

        #region Actions

        public void DeleteAccount(ADDomainController WritableDomainController)
        {
            if (IsCriticalSystemObject)
                throw new InvalidOperationException(string.Format("This account [{0}] is a Critical System Active Directory Object and Disco ICT refuses to modify it", DistinguishedName));

            if (!WritableDomainController.IsWritable)
                throw new InvalidOperationException(string.Format("The domain controller [{0}] is not writable. This action (Delete Account) requires a writable domain controller.", Name));

            using (ADDirectoryEntry entry = WritableDomainController.RetrieveDirectoryEntry(DistinguishedName))
            {
                entry.Entry.DeleteTree();
            }
        }
        public void DeleteAccount()
        {
            DeleteAccount(Domain.GetAvailableDomainController(RequireWritable: true));
        }

        private void SetNetbootGUID(ADDomainController WritableDomainController, Guid updatedNetbootGUID)
        {
            if (IsCriticalSystemObject)
                throw new InvalidOperationException(string.Format("This account {0} is a Critical System Active Directory Object and Disco refuses to modify it", DistinguishedName));

            if (NetbootGUID != updatedNetbootGUID)
            {
                using (var deAccount = WritableDomainController.RetrieveDirectoryEntry(DistinguishedName))
                {
                    var netbootGUIDProp = deAccount.Entry.Properties["netbootGUID"];
                    bool flag = netbootGUIDProp.Count > 0;
                    if (flag)
                    {
                        netbootGUIDProp.Clear();
                    }
                    netbootGUIDProp.Add(updatedNetbootGUID.ToByteArray());
                    deAccount.Entry.CommitChanges();
                    NetbootGUID = updatedNetbootGUID;
                }
            }
        }
        public void SetDescription(ADDomainController WritableDomainController, string Description)
        {
            if (IsCriticalSystemObject)
                throw new InvalidOperationException(string.Format("This account {0} is a Critical System Active Directory Object and Disco refuses to modify it", DistinguishedName));

            if (this.Description != Description)
            {
                using (var deAccount = WritableDomainController.RetrieveDirectoryEntry(DistinguishedName))
                {
                    var descriptionProp = deAccount.Entry.Properties["description"];
                    if (descriptionProp.Count != 1 || (descriptionProp[0] as string) != Description)
                    {
                        if (descriptionProp.Count > 0)
                        {
                            descriptionProp.Clear();
                        }
                        if (!string.IsNullOrEmpty(Description))
                        {
                            descriptionProp.Add(Description);
                        }
                        deAccount.Entry.CommitChanges();
                        this.Description = Description;
                    }
                }
            }
        }
        public void SetDescription(string Description)
        {
            if (Description != this.Description)
            {
                SetDescription(Domain.GetAvailableDomainController(RequireWritable: true), Description);
            }
        }

        public void SetDescription(ADDomainController WritableDomainController, Device Device)
        {
            var descriptionBuilder = new StringBuilder();

            if (Device.DecommissionedDate.HasValue)
            {
                descriptionBuilder.Append("Decommissioned: ")
                    .Append(Device.DecommissionReason.ReasonMessage())
                    .Append(" (").Append(Device.DecommissionedDate.Value.ToString("yyyy-MM-dd")).Append(')');
            }
            else
            {
                if (Device.AssignedUserId != null)
                {
                    descriptionBuilder.Append(Device.AssignedUser.UserId).Append(" (").Append(Device.AssignedUser.DisplayName).Append("); ");
                }

                if (Device.DeviceModelId.HasValue)
                {
                    descriptionBuilder.Append(Device.DeviceModel.Description).Append("; ");
                }

                descriptionBuilder.Append(Device.DeviceProfile.Description).Append(";");
            }

            string description = descriptionBuilder.ToString().Trim();
            if (description.Length > 1024)
                description = description.Substring(0, 1024);

            if (description != Description)
            {
                SetDescription(WritableDomainController, description);
            }
        }
        public void SetDescription(Device Device)
        {
            SetDescription(Domain.GetAvailableDomainController(RequireWritable: true), Device);
        }

        public void DisableAccount(ADDomainController WritableDomainController)
        {
            if (IsCriticalSystemObject)
                throw new InvalidOperationException(string.Format("This account {0} is a Critical System Active Directory Object and Disco ICT refuses to modify it", DistinguishedName));

            if (!IsDisabled)
            {
                using (var deAccount = WritableDomainController.RetrieveDirectoryEntry(DistinguishedName))
                {
                    var accountControl = (ADUserAccountControlFlags)deAccount.Entry.Properties["userAccountControl"][0];
                    if (!accountControl.HasFlag(ADUserAccountControlFlags.ADS_UF_ACCOUNTDISABLE))
                    {
                        var updatedAccountControl = (accountControl | ADUserAccountControlFlags.ADS_UF_ACCOUNTDISABLE);
                        deAccount.Entry.Properties["userAccountControl"][0] = (int)updatedAccountControl;
                        deAccount.Entry.CommitChanges();
                        UserAccountControl = updatedAccountControl;
                    }
                }
            }
        }
        public void DisableAccount()
        {
            if (!IsDisabled)
            {
                DisableAccount(Domain.GetAvailableDomainController(RequireWritable: true));
            }
        }

        public void EnableAccount(ADDomainController WritableDomainController)
        {
            if (IsCriticalSystemObject)
                throw new InvalidOperationException(string.Format("This account {0} is a Critical System Active Directory Object and Disco refuses to modify it", DistinguishedName));

            if (IsDisabled)
            {
                using (var deAccount = WritableDomainController.RetrieveDirectoryEntry(DistinguishedName))
                {
                    var accountControl = (ADUserAccountControlFlags)deAccount.Entry.Properties["userAccountControl"][0];
                    if (accountControl.HasFlag(ADUserAccountControlFlags.ADS_UF_ACCOUNTDISABLE))
                    {
                        var updatedAccountControl = (accountControl ^ ADUserAccountControlFlags.ADS_UF_ACCOUNTDISABLE);
                        deAccount.Entry.Properties["userAccountControl"][0] = (int)updatedAccountControl;
                        deAccount.Entry.CommitChanges();
                        UserAccountControl = updatedAccountControl;
                    }
                }
            }
        }
        public void EnableAccount()
        {
            if (IsDisabled)
            {
                EnableAccount(Domain.GetAvailableDomainController(RequireWritable: true));
            }
        }

        public bool UpdateNetbootGUID(ADDomainController WritableDomainController, string UUID, string MACAddress)
        {
            if (IsCriticalSystemObject)
                throw new InvalidOperationException(string.Format("This account {0} is a Critical System Active Directory Object and Disco refuses to modify it", DistinguishedName));

            Guid netbootGUID = Guid.Empty;

            if (!string.IsNullOrWhiteSpace(UUID))
            {
                netbootGUID = NetbootGUIDFromUUID(UUID);
            }
            else if (!string.IsNullOrWhiteSpace(MACAddress))
            {
                netbootGUID = NetbootGUIDFromMACAddress(MACAddress);
            }

            if (netbootGUID != System.Guid.Empty && netbootGUID != NetbootGUID)
            {
                SetNetbootGUID(WritableDomainController, netbootGUID);
                return true;
            }
            else
            {
                return false;
            }
        }
        public void UpdateNetbootGUID(string UUID, string MACAddress)
        {
            UpdateNetbootGUID(Domain.GetAvailableDomainController(RequireWritable: true), UUID, MACAddress);
        }
        public static Guid NetbootGUIDFromUUID(string UUID)
        {
            return new Guid(UUID);
        }
        public static Guid NetbootGUIDFromMACAddress(string MACAddress)
        {
            string strippedMACAddress = MACAddress.Trim().Replace(":", string.Empty).Replace("-", string.Empty);
            bool flag = strippedMACAddress.Length == 12;
            Guid NetbootGUIDFromMACAddress;
            if (flag)
            {
                Guid guid = new Guid(string.Format("00000000-0000-0000-0000-{0}", strippedMACAddress));
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
            if (IsCriticalSystemObject)
                throw new InvalidOperationException(string.Format("This account {0} is a Critical System Active Directory Object and Disco refuses to modify it", DistinguishedName));

            var parentDistinguishedName = ParentDistinguishedName;

            if (parentDistinguishedName != null && !parentDistinguishedName.Equals(NewOrganisationUnit, StringComparison.OrdinalIgnoreCase))
            {
                // If no OU provided, place in default Computers container
                if (string.IsNullOrWhiteSpace(NewOrganisationUnit))
                    NewOrganisationUnit = Domain.DefaultComputerContainer;

                if (!NewOrganisationUnit.EndsWith(Domain.DistinguishedName, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException(string.Format("Unable to move AD Account from one domain [{0}] to another [{1}].", DistinguishedName, NewOrganisationUnit));

                using (ADDirectoryEntry ou = WritableDomainController.RetrieveDirectoryEntry(NewOrganisationUnit))
                {
                    using (ADDirectoryEntry i = WritableDomainController.RetrieveDirectoryEntry(DistinguishedName))
                    {
                        i.Entry.UsePropertyCache = false;
                        i.Entry.MoveTo(ou.Entry);

                        // Update Distinguished Name
                        DistinguishedName = i.Entry.Properties["distinguishedName"][0].ToString();
                    }
                }
            }
        }
        public void MoveOrganisationalUnit(string NewOrganisationUnit)
        {
            MoveOrganisationalUnit(Domain.GetAvailableDomainController(RequireWritable: true), NewOrganisationUnit);
        }

        #endregion

        public override string ToString()
        {
            return Id;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is ADMachineAccount))
                return false;
            else
                return DistinguishedName == ((ADMachineAccount)obj).DistinguishedName;
        }
        public override int GetHashCode()
        {
            return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(DistinguishedName);
        }
    }
}
