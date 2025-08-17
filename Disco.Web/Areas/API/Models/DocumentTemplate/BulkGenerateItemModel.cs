namespace Disco.Web.Areas.API.Models.DocumentTemplate
{
    public class BulkGenerateItemModel
    {
        public string Id { get; set; }
        public string UserEmailAddress { get; set; }
        public string UserDisplayName { get; set; }
        public string Scope { get; set; }
        public bool IsError { get; set; }
    }
}
