using System.Collections.Generic;

namespace Disco.Services.Interop.ActiveDirectory
{
    public class ADOrganisationalUnit
    {
        public ADDomain Domain { get; private set; }
        public string DistinguishedName { get; private set; }

        public string Name { get; private set; }
        public List<ADOrganisationalUnit> Children { get; internal set; }

        private ADOrganisationalUnit(ADDomain Domain, string DistinguishedName, string Name, List<ADOrganisationalUnit> Children)
        {
            this.Domain = Domain;
            this.DistinguishedName = DistinguishedName;
            this.Name = Name;
            this.Children = Children;
        }

        public static ADOrganisationalUnit FromSearchResult(ADSearchResult SearchResult)
        {
            string distinguishedName = SearchResult.Value<string>("distinguishedName");
            string name = SearchResult.Value<string>("name");

            return new ADOrganisationalUnit(SearchResult.Domain, distinguishedName, name, null);
        }

        public override string ToString()
        {
            return this.Name;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is ADOrganisationalUnit))
                return false;
            else
                return this.DistinguishedName == ((ADOrganisationalUnit)obj).DistinguishedName;
        }
        public override int GetHashCode()
        {
            return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(this.DistinguishedName);
        }
    }
}
