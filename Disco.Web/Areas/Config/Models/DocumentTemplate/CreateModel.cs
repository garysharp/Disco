using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using Disco.Data.Repository;
using Disco.Models.UI.Config.DocumentTemplate;

namespace Disco.Web.Areas.Config.Models.DocumentTemplate
{
    [CustomValidation(typeof(CreateModelValidation), "ValidateCreateModel")]
    public class CreateModel : ConfigDocumentTemplateCreateModel
    {
        public Disco.Models.Repository.DocumentTemplate DocumentTemplate { get; set; }

        [Required]
        public HttpPostedFileBase Template { get; set; }

        public List<string> Types { get; set; }
        public List<string> SubTypes { get; set; }

        public List<Disco.Models.Repository.JobType> JobTypes { get; set; }
        public List<Disco.Models.Repository.JobSubType> JobSubTypes { get; set; }

        public List<string> Scopes
        {
            get
            {
                return Disco.Models.Repository.DocumentTemplate.DocumentTemplateScopes.ToList();
            }
        }

        public List<Disco.Models.Repository.JobType> GetJobTypes()
        {
            var types = Types;
            if (types != null && types.Count > 0)
                return JobTypes.Where(m => types.Contains(m.Id)).ToList();
            else
                return new List<Disco.Models.Repository.JobType>(JobTypes);
        }

        public List<Disco.Models.Repository.JobSubType> GetJobSubTypes()
        {
            var subTypes = SubTypes;
            if (subTypes != null && subTypes.Count > 0)
                return JobSubTypes.Where(m => subTypes.Contains($"{m.JobTypeId}_{m.Id}")).ToList();
            else
                return new List<Disco.Models.Repository.JobSubType>(JobSubTypes);
        }

        public void UpdateModel(DiscoDataContext Database)
        {
            if (JobTypes == null)
                JobTypes = Database.JobTypes.ToList();
            if (JobSubTypes == null)
                JobSubTypes = Database.JobSubTypes.ToList();
        }

    }

    public class CreateModelValidation
    {

        public static ValidationResult ValidateCreateModel(CreateModel model)
        {

            if (model.DocumentTemplate != null && model.DocumentTemplate.Scope == Disco.Models.Repository.DocumentTemplate.DocumentTemplateScopes.Job)
            {
                if (model.Types != null && model.SubTypes != null)
                {
                    var typeId = string.Format("{0}_", model.Types);
                    model.SubTypes = model.SubTypes.Where(m => model.Types.Contains(m.Substring(0, m.IndexOf("_")))).ToList();
                }
            }

            return ValidationResult.Success;
        }

    }

}