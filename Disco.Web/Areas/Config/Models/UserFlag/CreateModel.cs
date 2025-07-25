using Disco.Models.UI.Config.UserFlag;
using System.ComponentModel.DataAnnotations;

namespace Disco.Web.Areas.Config.Models.UserFlag
{
    public class CreateModel : ConfigUserFlagCreateModel
    {
        [Required, StringLength(100)]
        public string Name { get; set; }

        [StringLength(500), DataType(DataType.MultilineText)]
        public string Description { get; set; }
    }
}