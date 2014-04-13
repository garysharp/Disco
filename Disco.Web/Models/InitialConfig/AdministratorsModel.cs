using Disco.Web.Areas.Config.Models.AuthorizationRole;
using System.Collections.Generic;

namespace Disco.Web.Models.InitialConfig
{
    public class AdministratorsModel
    {
        public List<SubjectDescriptorModel> AdministratorSubjects { get; set; }
    }
}