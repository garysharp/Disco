using Disco.Models.UI.Config.JobQueue;
using System.ComponentModel.DataAnnotations;

namespace Disco.Web.Areas.Config.Models.JobQueue
{
    public class CreateModel : ConfigJobQueueCreateModel
    {
        [Required, StringLength(100)]
        public string Name { get; set; }
        [StringLength(500), DataType(DataType.MultilineText)]
        public string Description { get; set; }
    }
}