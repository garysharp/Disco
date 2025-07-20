using Disco.Models.UI.Config.DeviceModel;

namespace Disco.Web.Areas.Config.Models.DeviceModel
{
    public class _IndexModelItem : ConfigDeviceModelIndexModelItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Manufacturer { get; set; }
        public string Model { get; set; }
        public string ModelType { get; set; }
        public int DeviceCount { get; set; }
        public int DeviceDecommissionedCount { get; set; }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(Name))
                return $"{Manufacturer} {Model}";
            else
                return Name;
        }
    }
}