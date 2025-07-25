using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Models.Services.Documents;
using Disco.Models.UI.Config.DocumentTemplate;
using Disco.Services;
using Disco.Services.Documents.ManagedGroups;
using Disco.Services.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Disco.Web.Areas.Config.Models.DocumentTemplate
{
    public class ShowModel : ConfigDocumentTemplateShowModel
    {
        public Disco.Models.Repository.DocumentTemplate DocumentTemplate { get; set; }

        public int StoredInstanceCount { get; set; }

        public List<bool> TemplatePagesHaveAttachmentId { get; set; }
        public List<Expression> TemplateExpressions { get; set; }
        public int TemplatePageCount { get { return TemplatePagesHaveAttachmentId?.Count() ?? 0; } }

        public List<Disco.Models.Repository.UserFlag> UserFlags { get; set; }
        public List<OnImportUserFlagRule> OnImportUserFlagRules { get; set; }

        public List<JobType> JobTypes { get; set; }

        public List<string> Scopes
        {
            get
            {
                return Disco.Models.Repository.DocumentTemplate.DocumentTemplateScopes.ToList();
            }
        }

        public DocumentTemplateDevicesManagedGroup DevicesLinkedGroup { get; set; }
        public DocumentTemplateUsersManagedGroup UsersLinkedGroup { get; set; }

        public Guid? BulkGenerateDownloadId { get; set; }

        public string BulkGenerateDownloadFilename { get; set; }

        public void UpdateModel(DiscoDataContext Database)
        {

            switch (DocumentTemplate.Scope)
            {
                case Disco.Models.Repository.DocumentTemplate.DocumentTemplateScopes.Device:
                    StoredInstanceCount = Database.DeviceAttachments.Count(a => a.DocumentTemplateId == DocumentTemplate.Id);
                    break;
                case Disco.Models.Repository.DocumentTemplate.DocumentTemplateScopes.Job:
                    StoredInstanceCount = Database.JobAttachments.Count(a => a.DocumentTemplateId == DocumentTemplate.Id);
                    break;
                case Disco.Models.Repository.DocumentTemplate.DocumentTemplateScopes.User:
                    StoredInstanceCount = Database.UserAttachments.Count(a => a.DocumentTemplateId == DocumentTemplate.Id);
                    break;
            }

            if (JobTypes == null)
                JobTypes = Database.JobTypes.Include("JobSubTypes").ToList();

            UserFlags = Database.UserFlags.ToList();
            OnImportUserFlagRules = DocumentTemplate.GetOnImportUserFlagRuleDetails(Database).ToList();
        }
    }
}