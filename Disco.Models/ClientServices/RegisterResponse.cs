
namespace Disco.Models.ClientServices
{
    public class RegisterResponse
    {
        public string SessionId { get; set; }

        public string DeviceDomainName { get; set; }
        public string DeviceComputerName { get; set; }

        public string ErrorMessage { get; set; }
    }
}
