using System.ComponentModel.DataAnnotations;

namespace Disco.Web.Areas.Config.Models.SystemConfig
{
    public class SsoModel
    {
        public SsoMode Mode { get; set; }
        [Required, Display(Name = "Tenant ID"), RegularExpression(@"^[0-9a-f]{8}-([0-9a-f]{4}-){3}[0-9a-f]{12}$", ErrorMessage = "Expected 00000000-0000-0000-0000-000000000000 format.")]
        public string TenantId { get; set; }
        [Required, Display(Name = "Client ID"), RegularExpression(@"^[0-9a-f]{8}-([0-9a-f]{4}-){3}[0-9a-f]{12}$", ErrorMessage = "Expected 00000000-0000-0000-0000-000000000000 format.")]
        public string ClientId { get; set; }
        public string Authority { get; set; }
        public string TestedSession { get; set; }
    }

    public enum SsoMode
    {
        WindowsAuthentication,
        Testing,
        OpenIdConnect,
    }
}
