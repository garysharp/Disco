using Disco.Data.Repository;
using Disco.Models.BI.Config;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Disco.Data.Configuration.Modules
{
    public class OrganisationAddressesConfiguration : ConfigurationBase
    {
        private const string scope = "OrganisationAddresses";

        public OrganisationAddressesConfiguration(DiscoDataContext Database) : base(Database) { }

        public override string Scope { get { return scope; } }

        public OrganisationAddress GetAddress(int Id)
        {
            return Get<OrganisationAddress>(null, Id.ToString());
        }
        public OrganisationAddress SetAddress(OrganisationAddress Address)
        {
            if (!Address.Id.HasValue)
            {
                Address.Id = NextOrganisationAddressId;
            }

            Set(Address, Address.Id.ToString());

            return Address;
        }
        public void RemoveAddress(int Id)
        {
            // Remove Configuration Item
            RemoveItem(Id.ToString());
        }

        public List<OrganisationAddress> Addresses
        {
            get
            {
                return ItemKeys.Select(key => Get<OrganisationAddress>(null, key)).ToList();
            }
        }

        private int NextOrganisationAddressId
        {
            get
            {
                int nextId = 0;
                while (true)
                {
                    if (Get<string>(null, nextId.ToString()) == null)
                        break;
                    nextId++;
                }
                return nextId;
            }
        }

        internal static void MigrateDatabase(DiscoDataContext Database)
        {
            // Migrate all organisation addresses to JSON
            if (Database.ConfigurationItems.Count(i => i.Scope == scope && !i.Value.StartsWith("{")) > 0)
            {
                var items = Database.ConfigurationItems.Where(i => i.Scope == scope && !i.Value.StartsWith("{")).ToList();
                items.ForEach(i =>
                {
                    i.Value = JsonConvert.SerializeObject(OrganisationAddress.FromConfigurationEntry(int.Parse(i.Key), i.Value));
                });
                Database.SaveChanges();
            }
        }

    }
}
