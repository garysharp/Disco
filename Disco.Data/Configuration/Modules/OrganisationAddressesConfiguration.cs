using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Disco.Models.BI.Config;
using Disco.Models.Repository;
using Newtonsoft.Json;

namespace Disco.Data.Configuration.Modules
{
    public class OrganisationAddressesConfiguration : ConfigurationBase
    {
        public OrganisationAddressesConfiguration(ConfigurationContext Context) : base(Context) { }

        public override string Scope
        {
            get { return "OrganisationAddresses"; }
        }

        public OrganisationAddress GetAddress(int Id)
        {
            var address = default(OrganisationAddress);
            var addressString = this.GetValue<string>(Id.ToString(), null);
            if (addressString != null)
            {
                if (addressString.StartsWith("{"))
                {
                    // Assume Json
                    address = JsonConvert.DeserializeObject<OrganisationAddress>(addressString);
                }
                else
                {
                    // Assume Old Storage Method
                    address = OrganisationAddress.FromConfigurationEntry(Id, addressString);
                }
            }
            return address;
        }
        public OrganisationAddress SetAddress(OrganisationAddress Address)
        {
            if (!Address.Id.HasValue)
            {
                Address.Id = NextOrganisationAddressId;
            }

            string addressString = JsonConvert.SerializeObject(Address);

            this.SetValue(Address.Id.ToString(), addressString); //Address.ToConfigurationEntry());
            return Address;
        }
        public void RemoveAddress(int Id)
        {
            // Set Config Item to null = Remove Configuration Item
            this.SetValue<string>(Id.ToString(), null);
        }

        public List<OrganisationAddress> Addresses
        {
            get
            {
                Dictionary<string, ConfigurationItem> configAddress = default(Dictionary<string, ConfigurationItem>);
                if (this.Context.ConfigurationDictionary(this.Scope).TryGetValue(this.Scope, out configAddress))
                    return configAddress.Select(
                        ca => ca.Value.Value.StartsWith("{") ?
                            JsonConvert.DeserializeObject<OrganisationAddress>(ca.Value.Value) :
                            OrganisationAddress.FromConfigurationEntry(int.Parse(ca.Key), ca.Value.Value)
                        ).ToList();
                else
                    return new List<OrganisationAddress>(); // Empty List - No Addresses
            }
        }

        private int NextOrganisationAddressId
        {
            get
            {
                int nextId = 0;
                while (true)
                {
                    if (this.Context.ConfigurationItem(this.Scope, nextId.ToString()) == null)
                        break;
                    nextId++;
                }
                return nextId;
            }
        }

    }
}
