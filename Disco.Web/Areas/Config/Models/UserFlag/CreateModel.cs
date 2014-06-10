using Disco.Models.UI.Config.UserFlag;

namespace Disco.Web.Areas.Config.Models.UserFlag
{
    public class CreateModel : ConfigUserFlagCreateModel
    {
        public Disco.Models.Repository.UserFlag UserFlag { get; set; }
    }
}