using System.Collections.Generic;

namespace Disco.Models.UI.Config.UserFlag
{
    public interface ConfigUserFlagIndexModel : BaseUIModel
    {
        Dictionary<Repository.UserFlag, int> UserFlags { get; set; }
    }
}