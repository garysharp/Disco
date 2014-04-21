using Disco.Data.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Data.Configuration.Modules
{
    public class ActiveDirectoryConfiguration : ConfigurationBase
    {
        public ActiveDirectoryConfiguration(DiscoDataContext Database) : base(Database) { }

        public override string Scope
        {
            get { return "ActiveDirectory"; }
        }

        public Dictionary<string, List<string>> SearchContainers
        {
            get
            {
                return GetFromJson<Dictionary<string, List<string>>>(null);
            }
            set
            {
                SetAsJson(value);
            }
        }

        public bool? SearchAllForestServers
        {
            get { return GetFromJson<bool?>(null); }
            set { SetAsJson(value); }
        }
    }
}
