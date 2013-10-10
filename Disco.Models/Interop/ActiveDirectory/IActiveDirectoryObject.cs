using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Models.Interop.ActiveDirectory
{
    public interface IActiveDirectoryObject
    {
        string DistinguishedName { get; set; }
        string SecurityIdentifier { get; set; }
        
        string SamAccountName { get; set; }

        string Name { get; set; }
    }
}
