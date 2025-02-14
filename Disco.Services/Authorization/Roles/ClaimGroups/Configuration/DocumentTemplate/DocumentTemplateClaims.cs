namespace Disco.Services.Authorization.Roles.ClaimGroups.Configuration.DocumentTemplate
{
    [ClaimDetails("Document Templates", "Permissions related to Document Templates")]
    public class DocumentTemplateClaims : BaseRoleClaimGroup
    {
        [ClaimDetails("Configure Document Templates", "Can configure document templates")]
        public bool Configure { get; set; }

        [ClaimDetails("Configure Advanced Expression", "Can configure filter, generate and import expressions for document templates")]
        public bool ConfigureFilterExpression { get; set; }

        [ClaimDetails("Upload Document Templates", "Can upload document templates")]
        public bool Upload { get; set; }

        [ClaimDetails("Create Document Templates", "Can create document templates")]
        public bool Create { get; set; }

        [ClaimDetails("Delete Document Templates", "Can delete document templates")]
        public bool Delete { get; set; }

        [ClaimDetails("Show Document Templates", "Can show document templates")]
        public bool Show { get; set; }

        [ClaimDetails("Show Document Template Import Status", "Can show the document template import status")]
        public bool ShowStatus { get; set; }

        [ClaimDetails("Bulk Generate Document Templates", "Can bulk generate document templates")]
        public bool BulkGenerate { get; set; }
        
        [ClaimDetails("Export Attachment Instances", "Can export document attachment instances")]
        public bool Export { get; set; }

        [ClaimDetails("Process Undetected Pages", "Can show and assign imported documents which were not undetected")]
        public bool UndetectedPages { get; set; }
    }
}
