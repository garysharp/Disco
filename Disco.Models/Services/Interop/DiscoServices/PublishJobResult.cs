
namespace Disco.Models.Services.Interop.DiscoServices
{
    public class PublishJobResult
    {
        public bool Success { get; set; }

        public int Id { get; set; }
        public string Secret { get; set; }
        public string DeepLink { get; set; }
        public string PublishMessage { get; set; }

        public string ErrorMessage { get; set; }
    }
}
