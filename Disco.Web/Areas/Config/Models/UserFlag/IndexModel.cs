using Disco.Models.UI.Config.UserFlag;
using System.Collections.Generic;

namespace Disco.Web.Areas.Config.Models.UserFlag
{
    public class IndexModel : ConfigUserFlagIndexModel
    {
        public List<Disco.Models.Repository.UserFlag> UserFlags { get; set; }
    }
}