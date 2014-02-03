using System.Collections.Generic;

namespace Disco.Models.Services.Authorization
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
