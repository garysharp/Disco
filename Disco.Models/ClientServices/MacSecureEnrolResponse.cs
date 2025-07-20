namespace Disco.Models.ClientServices
{
    public class MacSecureEnrolResponse
    {
        public string DeviceComputerName { get; set; }

        public string DeviceAssignedUserDomain { get; set; }
        public string DeviceAssignedUserName { get; set; }
        public string DeviceAssignedUserSID { get; set; }
        public string DeviceAssignedUserUsername { get; set; }

        public string ErrorMessage { get; set; }

        public static MacSecureEnrolResponse FromMacEnrolResponse(MacEnrolResponse mer)
        {
            return new MacSecureEnrolResponse
            {
                DeviceComputerName = mer.DeviceComputerName,
                DeviceAssignedUserDomain = mer.DeviceAssignedUserDomain,
                DeviceAssignedUserName = mer.DeviceAssignedUserName,
                DeviceAssignedUserSID = mer.DeviceAssignedUserSID,
                DeviceAssignedUserUsername = mer.DeviceAssignedUserUsername
            };
        }

    }
}
