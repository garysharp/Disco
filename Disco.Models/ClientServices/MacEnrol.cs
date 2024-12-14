namespace Disco.Models.ClientServices
{
    public class MacEnrol : ServiceBase<MacEnrolResponse>
    {
        public override string Feature
        {
            get { return "MacEnrol"; }
        }

        public string DeviceSerialNumber { get; set; }
        public string DeviceUUID { get; set; }

        public string DeviceComputerName { get; set; }
        
        public string DeviceManufacturer { get; set; }
        public string DeviceModel { get; set; }
        public string DeviceModelType { get; set; }

        public string DeviceLanMacAddress { get; set; }

        public string DeviceWlanMacAddress { get; set; }
    }
}
