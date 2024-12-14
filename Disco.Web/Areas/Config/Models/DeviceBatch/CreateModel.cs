using Disco.Models.UI.Config.DeviceBatch;

namespace Disco.Web.Areas.Config.Models.DeviceBatch
{
    public class CreateModel : ConfigDeviceBatchCreateModel
    {
        public Disco.Models.Repository.DeviceBatch DeviceBatch { get; set; }
    }
}