using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Disco.Models.BI.Config;
using Disco.Models.Repository;
using Newtonsoft.Json;
using Disco.Data.Repository;

namespace Disco.Data.Configuration.Modules
{
    public class OrganisationAddressesConfiguration : ConfigurationBase
    {
        public OrganisationAddressesConfiguration(DiscoDataContext dbContext) : base(dbContext) { }

        public override string Scope { get { return "OrganisationAddresses"; } }

        public OrganisationAddress GetAddress(int Id)
        {
            var address = default(OrganisationAddress);
            var addressString = this.Get<string>(null, Id.ToString());
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

            this.Set(addressString, Address.Id.ToString()); //Address.ToConfigurationEntry());
            return Address;
        }
        public void RemoveAddress(int Id)
        {
            // Set Config Item to null = Remove Configuration Item
            this.Set<string>(null, Id.ToString());
        }

        public List<OrganisationAddress> Addresses
        {
            get
            {
                return this.Items.Select(ca => ca.Value.StartsWith("{") ?
                    JsonConvert.DeserializeObject<OrganisationAddress>(ca.Value) :
                    OrganisationAddress.FromConfigurationEntry(int.Parse(ca.Key), ca.Value)
                    ).ToList();
            }
        }

        private int NextOrganisationAddressId
        {
            get
            {
                int nextId = 0;
                while (true)
                {
                    if (this.Get<string>(null, nextId.ToString()) == null)
                        break;
                    nextId++;
                }
                return nextId;
            }
        }

    }
}
