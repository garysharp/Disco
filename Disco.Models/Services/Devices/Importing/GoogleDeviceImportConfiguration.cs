namespace Disco.Models.Services.Devices.Importing
{
    public class GoogleDeviceImportConfiguration
    {
        public int? DefaultDeviceProfileId { get; set; }
        public GoogleDeviceImportUserSource AssignedUserSource { get; set; }
    }

    public enum GoogleDeviceImportUserSource
    {
        None,
        AnnotatedUser,
        MostRecentUser,
    }
}
