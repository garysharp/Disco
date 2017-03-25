using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;

namespace Disco.Services.Interop.ActiveDirectory
{
    public class ADSearchResult
    {
        private SearchResult _result;

        public ADDomain Domain { get; private set; }
        public ADDomainController DomainController { get; private set; }

        public string SearchPath { get; private set; }
        public string LdapFilter { get; private set; }
        public string LdapPath { get; private set; }
        public string DistinguishedName { get; private set; }

        public ADSearchResult(ADDomain Domain, ADDomainController DomainController, string SearchPath, string LdapFilter, SearchResult Result)
        {
            _result = Result;
            this.Domain = Domain;
            this.DomainController = DomainController;
            this.SearchPath = SearchPath;
            this.LdapFilter = LdapFilter;

            LdapPath = _result.Path;
            DistinguishedName = Value<string>("dn");
        }

        public bool Contains(string PropertyName)
        {
            return _result.Properties.Contains(PropertyName);
        }

        public T Value<T>(string PropertyName)
        {
            var p = Values<T>(PropertyName);
            return p.FirstOrDefault();
        }

        public IEnumerable<T> Values<T>(string PropertyName)
        {
            var p = _result.Properties[PropertyName];
            return p.OfType<T>();
        }

        public override string ToString()
        {
            return LdapPath;
        }
    }
}
