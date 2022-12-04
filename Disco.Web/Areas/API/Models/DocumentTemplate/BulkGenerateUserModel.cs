namespace Disco.Web.Areas.API.Models.DocumentTemplate
{
    public class BulkGenerateUserModel
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public string Scope { get; set; }
        public bool IsError { get; set; }
    }
}