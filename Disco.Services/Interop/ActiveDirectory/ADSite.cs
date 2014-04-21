using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;

namespace Disco.Services.Interop.ActiveDirectory
{
    public class ADSite
    {
        private ActiveDirectoryContext context;

        public ActiveDirectorySite Site { get; private set; }

        public string Name { get; private set; }

        public List<ADDomainController> DomainControllers { get; private set; }

        public ADSite(ActiveDirectoryContext Context, ActiveDirectorySite Site)
        {
            this.context = Context;

            this.Site = Site;

            this.Name = Site.Name;
            this.DomainControllers = null;
        }

        internal void UpdateDomainControllers(IEnumerable<ADDomainController> DomainControllers)
        {
            this.DomainControllers = DomainControllers.ToList();
        }

        public override string ToString()
        {
            return this.Name;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is ADSite))
                return false;
            else
                return this.Name == ((ADSite)obj).Name;
        }
        public override int GetHashCode()
        {
            return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(this.Name);
        }
    }
}
