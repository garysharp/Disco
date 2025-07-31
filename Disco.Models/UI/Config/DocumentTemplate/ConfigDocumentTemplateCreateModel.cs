﻿using System.Collections.Generic;

namespace Disco.Models.UI.Config.DocumentTemplate
{
    public interface ConfigDocumentTemplateCreateModel : BaseUIModel
    {
        string Id { get; set; }
        string Description { get; set; }
        string Scope { get; set; }

        List<string> Types { get; set; }
        List<string> SubTypes { get; set; }

        List<Repository.JobType> JobTypes { get; set; }
        List<Repository.JobSubType> JobSubTypes { get; set; }

        List<string> Scopes { get; }

        List<Repository.JobType> GetJobTypes();
        List<Repository.JobSubType> GetJobSubTypes();
    }
}
