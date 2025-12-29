using System;
using System.ComponentModel.DataAnnotations;

namespace Disco.Web.Areas.API.Models.Activation
{
    public class CallbackModel
    {
        public string Origin { get; set; }
        public Guid DeploymentId { get; set; }
        public Guid CorrelationId { get; set; }
        [StringLength(50)]
        public string UserId { get; set; }
        public long Timestamp { get; set; }
        public string Proof { get; set; }
    }
}
