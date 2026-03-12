namespace Disco.Models.Services.Interop
{
    public class SsoConfiguration
    {
        public string Authority { get; set; }
        public string ClientId { get; set; }
        public string UpnClaimName { get; set; } = "preferred_username";

        public SsoConfiguration Clone()
        {
            return new SsoConfiguration()
            {
                Authority = Authority,
                ClientId = ClientId,
                UpnClaimName = UpnClaimName,
            };
        }
    }
}
