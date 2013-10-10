using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Services.Authorization.Roles
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
    public class ClaimDetailsAttribute : Attribute
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Hidden { get; set; }

        public ClaimDetailsAttribute(string Name, string Description, bool Hidden = false)
        {
            this.Name = Name;
            this.Description = Description;
            this.Hidden = Hidden;
        }
    }
}
