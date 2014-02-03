using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Services.Authorization.Roles.ClaimGroups.Job
{
    [ClaimDetails("Job Queue Properties", "Permissions related to Job Queue Job Properties")]
    public class JobQueuePropertiesClaims : BaseRoleClaimGroup
    {
        [ClaimDetails("Edit Any Comments", "Can edit any job queue comments")]
        public bool EditAnyComments { get; set; }
        [ClaimDetails("Edit Own Comments", "Can edit own job queue comments")]
        public bool EditOwnComments { get; set; }

        [ClaimDetails("Edit Any SLA", "Can edit any job queue SLA")]
        public bool EditAnySLA { get; set; }
        [ClaimDetails("Edit Own SLA", "Can edit own job queue SLA")]
        public bool EditOwnSLA { get; set; }

        [ClaimDetails("Edit Any Priority", "Can edit any job queue Priority")]
        public bool EditAnyPriority { get; set; }
        [ClaimDetails("Edit Own Priority", "Can edit own job queue Priority")]
        public bool EditOwnPriority { get; set; }
    }
}
