using System;
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

        public List<Disco.Models.Repository.JobType> GetJobTypes
        {
            get
            {
                if (Types != null)
                {
                    var types = this.Types;
                    return this.JobTypes.Where(m => types.Contains(m.Id)).ToList();
                }
                return null;
            }
        }
        public List<Disco.Models.Repository.JobSubType> GetJobSubTypes
        {
            get
            {
                if (SubTypes != null)
                {
                    var subTypes = this.SubTypes;
                    return this.JobSubTypes.Where(m => subTypes.Contains(String.Format("{0}_{1}", m.JobTypeId, m.Id))).ToList();
                }
                return null;
            }
        }

        public void UpdateModel(DiscoDataContext dbContext)
        {
            if (this.JobTypes == null)
                JobTypes = dbContext.JobTypes.ToList();
            if (this.JobSubTypes == null)
                JobSubTypes = dbContext.JobSubTypes.ToList();
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