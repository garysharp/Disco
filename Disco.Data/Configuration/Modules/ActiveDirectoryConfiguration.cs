using Disco.Data.Repository;
using System.Collections.Generic;

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
                return Get<Dictionary<string, List<string>>>(null);
            }
            set
            {
                Set(value);
            }
        }

        public bool? SearchAllForestServers
        {
            get { return Get<bool?>(null); }
            set { Set(value); }
        }

        /// <summary>
        /// If true LDAP filters contain wildcards only at the end of the search term.
        /// This greatly improves performance in very large AD environments (ie: EDU001/EDU002)
        /// </summary>
        public bool SearchWildcardSuffixOnly
        {
            get { return Get(true); }
            set { Set(value); }
        }
    }
}
