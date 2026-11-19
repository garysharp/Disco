namespace Disco.Models.Services.Devices.Importing
{
    public class IntuneDeviceImportConfiguration
    {
        public bool AssignDeviceToPrimaryUser { get; set; } = true;
        public int? DefaultDeviceProfileId { get; set; }
    }
}
