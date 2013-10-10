using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Models.Authorization
{
    public interface IClaimNavigatorItem
    {
        string Key { get; }
        string Name { get; }
        string Description { get; }
        bool Hidden { get; }

        List<IClaimNavigatorItem> Children { get; }
        bool IsGroup { get; }
        bool? Value { get; }
    }
}
