using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Models.Interop.ActiveDirectory
{
    public class ActiveDirectoryOrganisationalUnit
    {
        public string Name { get; set; }
        public string Domain { get; set; }
        public string DistinguishedName { get; set; }
        public List<ActiveDirectoryOrganisationalUnit> Children { get; set; }
    }
}
