using System.Collections.Generic;

namespace Disco.Models.UI.Config.UserFlag
{
    public interface ConfigUserFlagIndexModel : BaseUIModel
    {
        List<Repository.UserFlag> UserFlags { get; set; }
    }
}