using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Services.Interop.ActiveDirectory
{
    public class ADDirectoryEntry : IDisposable
    {
        public ADDomain Domain { get; private set; }
        public ADDomainController DomainController { get; private set; }
        public DirectoryEntry Entry { get; private set; }

        internal ADDirectoryEntry(ADDomain Domain, ADDomainController DomainController, DirectoryEntry Entry)
        {
            if (Domain == null)
                throw new ArgumentNullException("Domain");
            if (DomainController == null)
                throw new ArgumentNullException("DomainController");
            if (Entry == null)
                throw new ArgumentNullException("Entry");

            this.Domain = Domain;
            this.DomainController = DomainController;
            this.Entry = Entry;
        }

        public void Dispose()
        {
            if (Entry != null)
            {
                Entry.Dispose();
                Entry = null;
            }
        }

        public override string ToString()
        {
            if (this.Entry != null)
                return this.Entry.Path;
            else
                return base.ToString();
        }
    }
}
